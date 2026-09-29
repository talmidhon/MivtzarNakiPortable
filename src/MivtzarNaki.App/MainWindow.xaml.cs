using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MivtzarNaki.Core;
using MivtzarNaki.Windows;

namespace MivtzarNaki.App;

public sealed partial class MainWindow : Window
{
    private readonly UpdateSession _session = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMinutes(15) };
    private readonly DispatcherTimer _contrastTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    private bool _highContrast = RTLHelper.HighContrast;
    private CancellationTokenSource? _operation;
    private bool _busy, _installing, _closed, _checking, _localLoading;
    private string? _fixture = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--ui-fixture="))?.Split('=')[1];
    public MainWindow()
    {
        InitializeComponent();
        RTLHelper.Apply(this, Root);
        AppWindow.Resize(new global::Windows.Graphics.SizeInt32(660, 860));
        if (Enum.TryParse<ElementTheme>(_session.Settings.Theme, out var theme)) Root.RequestedTheme = theme;
        if (Environment.GetCommandLineArgs().Contains("--theme=dark")) Root.RequestedTheme = ElementTheme.Dark;
        if (Environment.GetCommandLineArgs().Contains("--theme=light")) Root.RequestedTheme = ElementTheme.Light;
        Viewport.RequestedTheme = Root.RequestedTheme;
        Root.KeyDown += async (_, args) =>
        {
            if (_fixture is null) return;
            if (args.Key == global::Windows.System.VirtualKey.F6)
            {
                string[] states = ["loading", "missing", "success", "offline", "failure", "progress"];
                _fixture = states[(Array.IndexOf(states, _fixture) + 1) % states.Length]; Render(); args.Handled = true;
            }
            if (args.Key == global::Windows.System.VirtualKey.F7) { Theme_Click(this, new RoutedEventArgs()); args.Handled = true; }
            if (args.Key == global::Windows.System.VirtualKey.F9) { args.Handled = true; await Confirm("בדיקת דיאלוג — ללא פעולת מערכת", "שירות Defender אינו פועל. זהו דיאלוג בדיקה בלבד; לחיצה לא תשנה את המחשב.", "בדיקה בלבד"); }
        };
        if (Environment.GetCommandLineArgs().Contains("--small-window")) AppWindow.Resize(new global::Windows.Graphics.SizeInt32(460, 650));
        LocationText.Text = _session.Environment.Payloads;
        Root.ActualThemeChanged += (_, _) => Render();
        _contrastTimer.Tick += (_, _) => { var current = RTLHelper.HighContrast; if (current != _highContrast) { _highContrast = current; Render(); } };
        _contrastTimer.Start();
        Root.Loaded += Loaded;
        AppWindow.Closing += (_, args) =>
        {
            if (_installing) { args.Cancel = true; StatusText.Text = "יש להמתין לסיום הפעולה לפני סגירה."; return; }
            _operation?.Cancel();
        };
        Closed += (_, _) => { _closed = true; _timer.Stop(); _contrastTimer.Stop(); _lifetime.Cancel(); };
        _timer.Tick += async (_, _) => { if (!_busy && !_checking) await RefreshAsync(false); };
    }
    private async void Loaded(object sender, RoutedEventArgs args)
    {
        Root.Loaded -= Loaded;
        var scale = Root.XamlRoot.RasterizationScale;
        var small = Environment.GetCommandLineArgs().Contains("--small-window");
        var area = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary).WorkArea;
        AppWindow.Resize(new global::Windows.Graphics.SizeInt32(Math.Min((int)((small ? 440 : 640) * scale), area.Width - 24), Math.Min((int)((small ? 600 : 800) * scale), area.Height - 24)));
        // Acknowledge actual XAML load, without waiting for a network connection.
        var command = Environment.GetCommandLineArgs();
        var ack = Array.IndexOf(command, "--update-ack");
        if (ack >= 0 && ack + 1 < command.Length)
        {
            var path = Path.GetFullPath(command[ack + 1]);
            var allowed = Path.Combine(_session.Environment.Root, ".updates") + Path.DirectorySeparatorChar;
            if (path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) && Path.GetFileName(path).StartsWith("started-")) await File.WriteAllTextAsync(path, "UI loaded");
        }
        if (_fixture is not null)
        {
            RenderFixture();
            if (command.Contains("--smoke-test")) await CompleteSmokeAsync();
            return;
        }
        await RefreshAsync(true);
        if (_closed) return;
        Viewport.RequestedTheme = Root.RequestedTheme;
        _timer.Start();
        var updateError = Path.Combine(_session.Environment.Root, ".updates", "update-error.log");
        if (File.Exists(updateError)) ShowProblem("עדכון מבצר נקי לא הושלם. הגרסה הקודמת נשמרה; יש לבדוק את תיקיית הגיבוי לפני ניסיון נוסף.");
        if (command.Contains("--smoke-test"))
        {
            await CompleteSmokeAsync();
        }
    }
    private async Task CompleteSmokeAsync()
    {
        await Task.Delay(500);
        await File.WriteAllTextAsync(Path.Combine(_session.Environment.Data, "smoke-test.txt"), $"UI loaded\nPC={ComputerVersion.Text}\nUSB={UsbVersion.Text}\nServer={ServerVersion.Text}\nRoot={_session.Environment.Root}\nElevated={DefenderSystem.IsAdministrator()}\nRTL={Root.FlowDirection}\nScale={Root.XamlRoot.RasterizationScale}\nTheme={Root.ActualTheme}\nViewportTheme={Viewport.ActualTheme}\nFixture={_fixture ?? "none"}\nComputerStatus={ComputerStatus.Text}\nUsbStatus={UsbStatus.Text}\nWindow={AppWindow.Size.Width}x{AppWindow.Size.Height}");
        Close();
    }
    private async Task RefreshAsync(bool local)
    {
        if (_checking || _busy || _fixture is not null) return;
        _checking = true;
        try
        {
            var network = _session.CheckNetworkAsync(_lifetime.Token);
            try
            {
                if (local)
                {
                    _localLoading = true; Render();
                    await _session.ReadLocalAsync(_lifetime.Token);
                }
            }
            finally { _localLoading = false; Render(); }
            await network;
        }
        catch (OperationCanceledException) when (_closed) { }
        catch (Exception ex) { if (!_closed) ShowProblem("לא ניתן לקרוא את המידע. נסה לרענן שוב."); _session.Log.Write("בדיקה נכשלה", ex); }
        finally { _checking = false; if (!_closed) { Render(); if (StatusText.Text == "קורא נתונים…") StatusText.Text = "הורדה והתקנה מתבצעות רק בלחיצה שלך."; } }
    }
    private async Task RunAsync(Func<CancellationToken, Task> action, bool installing = false)
    {
        if (_busy || _localLoading || _fixture is not null) return;
        _busy = true; _installing = installing;
        _operation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        Progress.Opacity = 1; Progress.IsIndeterminate = true;
        StatusText.Foreground = Brush(StatusTone.Action);
        Render();
        try { await action(_operation.Token); StatusText.Foreground = Brush(StatusTone.Success); }
        catch (OperationCanceledException) when (_closed) { }
        catch (OperationCanceledException) { StatusText.Text = "הפעולה בוטלה. העדכון התקין הקודם נשמר."; StatusText.Foreground = Brush(StatusTone.Unknown); }
        catch (Exception ex) { if (!_closed) ShowProblem("הפעולה לא הושלמה. " + FriendlyError(ex)); _session.Log.Write("הפעולה נכשלה", ex); }
        finally
        {
            _busy = _installing = false;
            _operation.Dispose(); _operation = null;
            if (!_closed) { Progress.Opacity = 0; Render(); }
        }
    }
    private static string FriendlyError(Exception ex) => ex switch
    {
        HttpRequestException => "לא ניתן להתחבר לשרת. אפשר לנסות שוב או להשתמש בחבילה המקומית.",
        UnauthorizedAccessException => "אין הרשאת כתיבה לתיקייה או הרשאה לביצוע הפעולה.",
        IOException => "לא ניתן לקרוא או לכתוב את הקבצים. בדוק שה־USB מחובר ויש בו מקום פנוי.",
        System.Security.Cryptography.CryptographicException => "לא ניתן לאמת את חתימת Microsoft. החבילה לא תופעל.",
        _ => ex.Message
    };
    private Progress<TransferProgress> CreateProgress() => new(p =>
    {
        if (_closed || !_busy) return;
        Progress.IsIndeterminate = p.Total is null; Progress.Value = p.Percent;
        StatusText.Text = p.Total > 0 ? $"{p.Message}  \u2066{p.Percent:0}%\u2069" : p.Message;
    });
    private Brush Brush(StatusTone tone)
    {
        var key = _highContrast ? "HighContrast" : Root.ActualTheme == ElementTheme.Light ? "Light" : "Default";
        return (Brush)((ResourceDictionary)Application.Current.Resources.ThemeDictionaries[key])[tone + "Brush"];
    }
    private void ApplyRows(StatusPresentation view)
    {
        ComputerStatus.Text = view.Computer.Text; ComputerDetail.Text = view.Computer.Detail;
        UsbStatus.Text = view.Usb.Text; UsbDetail.Text = view.Usb.Detail;
        ComputerIcon.Foreground = ComputerStatus.Foreground = Brush(view.Computer.Tone);
        UsbIcon.Foreground = UsbStatus.Foreground = Brush(view.Usb.Tone);
        static string Glyph(StatusTone tone) => tone switch { StatusTone.Success => "\uE73E", StatusTone.Error => "\uEA39", StatusTone.Attention => "\uE7BA", StatusTone.Action => "\uE895", _ => "\uE946" };
        ComputerIcon.Glyph = Glyph(view.Computer.Tone); UsbIcon.Glyph = Glyph(view.Usb.Tone);
        InstallButton.Visibility = view.Computer.Action ? Visibility.Visible : Visibility.Collapsed;
        DownloadButton.Visibility = view.Usb.Action ? Visibility.Visible : Visibility.Collapsed;
    }
    private void Render()
    {
        if (_closed) return;
        Viewport.RequestedTheme = Root.RequestedTheme;
        if (_fixture is not null) { RenderFixture(); return; }
        var view = StatusPresentation.Create(_session.Computer, _session.Local, _session.Remote, _session.LocalError);
        if (_localLoading) view = view with { Computer = new("קורא את מצב המחשב וה־USB…", "מאמת את חבילת העדכון המקומית.", StatusTone.Action) };
        if (_checking) view = view with { Usb = new("בודק עדכניות מול Microsoft…", "אפשר לעדכן את המחשב מחבילה מקומית תקינה.", StatusTone.Action) };
        ApplyRows(view);
        ComputerVersion.Text = _session.Computer.Version?.ToString() ?? "לא ידועה";
        UsbVersion.Text = _session.Local?.Version.ToString() ?? "אין חבילה מאומתת";
        ServerVersion.Text = (_session.Remote ?? _session.LastRemote)?.Version?.ToString() ?? "לא ידועה";
        NetworkDetail.Text = _session.NetworkMessage + (_session.Remote is null && _session.LastCheck is { } last ? $"\nמידע מהבדיקה האחרונה בלבד: {last:g}" : "");
        DownloadButton.IsEnabled = !_busy && !_checking && !_localLoading && _session.CanDownload;
        InstallButton.IsEnabled = !_busy && !_localLoading && _session.CanInstall;
        CheckButton.IsEnabled = !_busy && !_checking;
        CancelButton.Visibility = _busy && !_installing ? Visibility.Visible : Visibility.Collapsed;
        RepairButton.Visibility = _session.Computer.Running == false && _session.Local is not null && _session.Computer.Version is not null && _session.Local.Version > _session.Computer.Version ? Visibility.Visible : Visibility.Collapsed;
        RepairButton.IsEnabled = !_busy && !_localLoading;
        AppStatus.Text = _session.AppMessage;
        AppUpdateButton.Visibility = _session.NewApp is null ? Visibility.Collapsed : Visibility.Visible;
        AppUpdateButton.IsEnabled = !_busy && !_checking;
    }
    private void RenderFixture()
    {
        var usb = new Payload("fixture", new(1, 2, 3, 4), 1, "");
        var server = new RemotePayload(MicrosoftCatalog.DownloadUri, new(1, 2, 3, 5), 1, null, null, false);
        var view = StatusPresentation.Create(new(new(1, 2, 3, 3), true, ""), _fixture == "missing" ? null : usb, _fixture == "offline" ? null : server, _fixture == "failure" ? "fixture" : null);
        if (_fixture == "success") view = StatusPresentation.Create(new(usb.Version, true, ""), usb, server with { Version = usb.Version });
        if (_fixture == "loading") view = new(new("קורא את מצב המחשב…", "מאמת את החבילה המקומית.", StatusTone.Action), new("בודק עדכניות מול Microsoft…", "עד שלושה ניסיונות קצרים.", StatusTone.Action));
        ApplyRows(view);
        CheckButton.IsEnabled = DownloadButton.IsEnabled = InstallButton.IsEnabled = AppUpdateButton.IsEnabled = false;
        AppStatus.Text = "עדכון מבצר נקי: בדיקת רשת מושבתת בתצוגת הבדיקה";
        StatusText.Text = _fixture == "progress" ? "מוריד עדכון Defender ל־USB…  ⁦42%⁩" : _fixture == "failure" ? "הפעולה לא הושלמה. החבילה הקודמת נשמרה." : _fixture == "success" ? "העדכון מוכן להעברה למחשב היעד" : "תצוגת בדיקה — פעולות מערכת מושבתות";
        StatusText.Foreground = Brush(_fixture == "failure" ? StatusTone.Error : StatusTone.Unknown);
        Progress.Opacity = _fixture is "progress" or "loading" ? 1 : 0;
        Progress.IsIndeterminate = _fixture == "loading"; Progress.Value = 42;
        ComputerVersion.Text = "1.2.3.3"; UsbVersion.Text = "1.2.3.4"; ServerVersion.Text = "1.2.3.5";
    }
    private void ShowProblem(string message) { StatusText.Text = message; StatusText.Foreground = Brush(StatusTone.Error); }
    private async Task<bool> Confirm(string title, string content, string action)
    {
        var dialog = new ContentDialog { XamlRoot = Root.XamlRoot, FlowDirection = FlowDirection.RightToLeft, Title = title, Content = content, PrimaryButtonText = action, CloseButtonText = "ביטול", DefaultButton = ContentDialogButton.Close, RequestedTheme = Root.ActualTheme };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
    private async void Check_Click(object sender, RoutedEventArgs args) => await RefreshAsync(true);
    private async void Download_Click(object sender, RoutedEventArgs args) => await RunAsync(async token =>
    {
        await _session.PrepareAsync(CreateProgress(), token);
        StatusText.Text = "העדכון מוכן להעברה למחשב היעד";
    });
    private async void Install_Click(object sender, RoutedEventArgs args) => await RunAsync(async token =>
    {
        StatusText.Text = "מאמת ומתקין את העדכון המקומי…";
        try { await _session.InstallAsync(false, token); }
        catch (ElevationNeededException)
        {
            if (!await Confirm("נדרשות הרשאות מנהל", "Windows דורש הרשאות מנהל להתקנת חתימות Defender במחשב הזה. ייפתח חלון הרשאות והעדכון המקומי יופעל.", "המשך להתקנה")) { StatusText.Text = "ההתקנה בוטלה."; return; }
            await _session.InstallAsync(true, token);
        }
        StatusText.Text = "העדכון הותקן וגרסת Defender אומתה.";
    }, installing: true);
    private async void Repair_Click(object sender, RoutedEventArgs args)
    {
        if (_busy || _fixture is not null) return;
        await RunAsync(async _ =>
        {
            if (!await Confirm("הפעלת שירות Defender", "שירות Defender אינו פועל וחוסם את העדכון. הפעולה תבקש מ־Windows להפעיל אותו, עשויה להפעיל הגנת אנטי־וירוס ודורשת מנהל. לא נשנה מדיניות ארגונית ולא נסיר אנטי־וירוס אחר.", "הפעל את השירות")) { StatusText.Text = "התיקון בוטל."; return; }
            await _session.StartDefenderAsync();
            StatusText.Text = "מצב Defender נבדק מחדש. התקנת העדכון דורשת לחיצה נפרדת.";
        }, installing: true);
    }
    private async void AppUpdate_Click(object sender, RoutedEventArgs args) => await RunAsync(async token =>
    {
        StatusText.Text = "מכין את עדכון מבצר נקי…";
        await _session.UpdateAppAsync(CreateProgress(), token);
        _installing = false; Close();
    });
    private void Cancel_Click(object sender, RoutedEventArgs args) => _operation?.Cancel();
    private void Theme_Click(object sender, RoutedEventArgs args)
    {
        Root.RequestedTheme = Root.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        _session.Settings = _session.Settings with { Theme = Root.RequestedTheme.ToString() };
        if (_fixture is null) { try { _session.SaveSettings(); } catch (Exception ex) { _session.Log.Write("שמירת העיצוב נכשלה.", ex); } }
        Render();
    }
}
