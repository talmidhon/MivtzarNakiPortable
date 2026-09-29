using Microsoft.UI.Xaml;

namespace MivtzarNaki.App;

public partial class App : Application
{
    private Window? _window;
    public App()
    {
        InitializeComponent();
        UnhandledException += (_, args) =>
        {
            try { var environment = new MivtzarNaki.Core.PortableEnvironment(); new MivtzarNaki.Core.ActivityLog(environment).Write("שגיאת ממשק: " + args.Exception.ToString()); }
            catch { /* Do not mask the original unhandled exception. */ }
        };
    }
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
