using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace MivtzarNaki.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.FirstOrDefault() is "--install-local" or "--start-defender")
        {
            Environment.ExitCode = global::MivtzarNaki.Windows.DefenderSystem.RunWorkerAsync(args).GetAwaiter().GetResult();
            return;
        }
        if (args.FirstOrDefault() == "--apply-update")
        {
            Environment.ExitCode = global::MivtzarNaki.Windows.PortableAppUpdate.ApplyAsync(args).GetAwaiter().GetResult();
            return;
        }
        using var instance = new Mutex(true, "Local\\MivtzarNaki.Portable", out var ownsInstance);
        if (!ownsInstance) return;
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(parameters =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
            _ = new App();
        });
    }
}
