using MivtzarNaki.Core;

namespace MivtzarNaki.Windows;

public sealed class UpdateSession : IDisposable
{
    private readonly HttpClient _http;
    private readonly IPayloadVerifier _verifier;
    private readonly MicrosoftCatalog _catalog;
    private readonly ResumableDownloader _downloader;
    private readonly PayloadStore _store;
    private readonly IDefenderOperations _defender;
    private readonly SemaphoreSlim _networkGate = new(1, 1);
    public string? LocalError { get; private set; }
    public RemotePayload? LastRemote { get; private set; }
    public DateTimeOffset? LastCheck { get; private set; }
    public string AppMessage { get; private set; } = "בודק עדכון למבצר נקי…";
    public PortableEnvironment Environment { get; }
    public ActivityLog Log { get; }
    public AppSettings Settings { get; set; }
    public DefenderStatus Computer { get; private set; } = new(null, null, "בודק…");
    public Payload? Local { get; private set; }
    public RemotePayload? Remote { get; private set; }
    public AppRelease? NewApp { get; private set; }
    public string NetworkMessage { get; private set; } = "בודק חיבור…";
    public static Version AppVersion => AppIdentity.Version;
    public string EffectiveUpdateFeed => string.IsNullOrWhiteSpace(Settings.UpdateFeed) ? Distribution.UpdateFeed : Settings.UpdateFeed;
    public bool CanDownload => Remote is not null && UpdatePolicy.NeedsDownload(Local, Remote);
    public bool CanInstall => UpdatePolicy.CanInstall(Local, Computer);

    public UpdateSession(HttpClient? http = null, string? root = null, IPayloadVerifier? verifier = null, IDefenderOperations? defender = null)
    {
        _http = http ?? new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        _verifier = verifier ?? new MicrosoftSignatureVerifier();
        Environment = new(root);
        Log = new(Environment);
        Settings = Environment.LoadSettings();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"MivtzarNaki/{AppVersion}");
        _catalog = new(_http);
        _downloader = new(_http);
        _store = new(Environment.Payloads, _verifier);
        _defender = defender ?? new DefenderSystem(new MicrosoftSignatureVerifier());
    }
    public async Task ReadLocalAsync(CancellationToken token)
    {
        Computer = await Task.Run(_defender.ReadStatus, token);
        Local = null;
        LocalError = null;
        try { Local = await _store.ReadAsync(token); }
        catch (Exception ex) when (ex is not OperationCanceledException) { LocalError = "חבילת העדכון ב־USB אינה תקינה או אינה נגישה."; Log.Write(LocalError, ex); }
    }
    public async Task CheckNetworkAsync(CancellationToken token)
    {
        if (!await _networkGate.WaitAsync(0, token)) return;
        try
        {
        Remote = null;
        var appCheck = CheckAppAsync(token);
        try
        {
            Remote = await _catalog.CheckAsync(token);
            LastRemote = Remote;
            LastCheck = DateTimeOffset.Now;
            NetworkMessage = "הבדיקה האחרונה: " + DateTime.Now.ToString("HH:mm");
            Log.Write(Remote.Version is null ? "שרת ההורדה זמין; מספר הגרסה לא זוהה." : $"גרסה ב־Microsoft: {Remote.Version}");
        }
        catch (Exception ex) when (!token.IsCancellationRequested)
        { NetworkMessage = "לא ניתן לבדוק עדכניות מול Microsoft. אפשר להשתמש בעדכון המקומי."; Log.Write(NetworkMessage, ex); }
        await appCheck;
        token.ThrowIfCancellationRequested();
        }
        finally { _networkGate.Release(); }
    }
    private async Task CheckAppAsync(CancellationToken token)
    {
        NewApp = null;
        AppMessage = "לא ניתן לבדוק עדכון למבצר נקי";
        if (Uri.TryCreate(EffectiveUpdateFeed, UriKind.Absolute, out var feed))
        {
            try { NewApp = await new AppUpdates(_http, feed).CheckAsync(AppVersion, token); AppMessage = NewApp is null ? "מבצר נקי מעודכן לפי מקור ההפצה" : "זמין עדכון למבצר נקי"; }
            catch (Exception ex) when (!token.IsCancellationRequested) { AppMessage = "לא ניתן לבדוק עדכון למבצר נקי"; Log.Write(AppMessage, ex); }
        }
    }
    public async Task PrepareAsync(IProgress<TransferProgress> progress, CancellationToken token)
    {
        Remote = await _catalog.CheckAsync(token);
        if (Remote.Version is null) throw new InvalidOperationException("גרסת העדכון בשרת אינה ידועה. לא ניתן לקבוע שנדרשת הורדה.");
        if (!CanDownload) { Log.Write("הקובץ על ה־USB כבר מעודכן מול Microsoft."); return; }
        var candidate = await _downloader.DownloadAsync(Remote, Environment.Payloads, progress, token);
        progress.Report(new(0, null, "מאמת חתימה וגרסת קובץ…"));
        try { Local = await _store.CommitAsync(candidate, Remote.Version, token); }
        catch (Exception ex) when (ex is InvalidDataException or System.Security.Cryptography.CryptographicException)
        {
            // A complete but invalid transfer must not be reused on the next attempt.
            ResumableDownloader.ClearCompleted(Environment.Payloads);
            throw;
        }
        ResumableDownloader.ClearCompleted(Environment.Payloads);
        LocalError = null;
        Log.Write($"גרסה {Local.Version} מוכנה ב־USB. אפשר להעביר את הכונן למחשב המנותק.");
    }
    public async Task InstallAsync(bool elevated, CancellationToken token)
    {
        await ReadLocalAsync(token);
        if (!CanInstall) throw new InvalidOperationException(Computer.Detail.Length > 0 ? Computer.Detail : "אין קובץ חדש יותר שניתן להתקין במחשב זה.");
        if (elevated) await _defender.InstallElevatedAsync(Local!);
        else await _defender.InstallAsync(Local!, token);
        Computer = await Task.Run(_defender.ReadStatus);
        if (!UpdatePolicy.ReachedTarget(Computer.Version, Local!.Version)) throw new InvalidOperationException("Defender לא הגיע לגרסת היעד. ההתקנה לא אומתה.");
        Log.Write($"ההתקנה אומתה. גרסת המחשב: {Computer.Version}");
    }
    public async Task StartDefenderAsync()
    {
        if (Computer.Running != false || Local is null || Computer.Version is null || Local.Version <= Computer.Version) throw new InvalidOperationException("לא זוהתה בעיית שירות החוסמת עדכון.");
        await _defender.StartServiceAsync();
        Computer = await Task.Run(_defender.ReadStatus);
        Log.Write("בקשת הפעלת שירות Defender הסתיימה. מצב ההגנה נבדק מחדש.");
    }
    public async Task UpdateAppAsync(IProgress<TransferProgress> progress, CancellationToken token)
    {
        if (NewApp is null || Version.Parse(NewApp.Version.TrimStart('v')) <= AppVersion || !Uri.TryCreate(EffectiveUpdateFeed, UriKind.Absolute, out var feed)) throw new InvalidOperationException("אין עדכון תוכנה זמין.");
        var path = await new AppUpdates(_http, feed).StageAsync(NewApp, Path.Combine(Environment.Root, ".updates"), progress, token);
        var staging = Path.Combine(Environment.Root, ".updates", "verified-" + Guid.NewGuid().ToString("N"));
        try
        {
            await FolderPackage.ExtractAsync(path, staging, NewApp.Sha256, token);
            var manifest = await FolderPackage.ValidateAsync(staging, token);
            if (Version.Parse(manifest.Version) != Version.Parse(NewApp.Version.TrimStart('v'))) throw new InvalidDataException("גרסת החבילה אינה תואמת למידע ההפצה.");
            token.ThrowIfCancellationRequested();
            PortableAppUpdate.StartReplacement(path, NewApp.Sha256);
        }
        finally
        {
            // This disposable validation copy is never used for replacement or rollback.
            try { if (Directory.Exists(staging)) { FolderPackage.RejectLinks(staging); Directory.Delete(staging, true); } }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Log.Write("ניקוי עותק אימות עדכון התוכנה נכשל.", ex); }
        }
    }
    public void SaveSettings() => Environment.SaveSettings(Settings);
    public void Dispose() => _http.Dispose();
}
