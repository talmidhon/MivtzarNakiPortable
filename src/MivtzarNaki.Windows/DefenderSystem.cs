using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using MivtzarNaki.Core;

namespace MivtzarNaki.Windows;

public sealed class ElevationNeededException : Exception;

public interface IDefenderOperations
{
    DefenderStatus ReadStatus();
    Task InstallAsync(Payload payload, CancellationToken token);
    Task InstallElevatedAsync(Payload payload);
    Task StartServiceAsync();
}

public sealed class DefenderSystem(MicrosoftSignatureVerifier verifier) : IDefenderOperations
{
    public Task StartServiceAsync() => StartServiceWithConsentAsync();
    public DefenderStatus ReadStatus()
    {
        try
        {
            using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var signatures = machine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Signature Updates");
            var raw = signatures?.GetValue("AVSignatureVersion")?.ToString() ?? signatures?.GetValue("ASSignatureVersion")?.ToString();
            Version.TryParse(raw, out var version);
            bool? running = null;
            try { using var service = new System.ServiceProcess.ServiceController("WinDefend"); running = service.Status == System.ServiceProcess.ServiceControllerStatus.Running; }
            catch (InvalidOperationException) { }
            return new(version, running, version is null ? "לא ניתן לזהות חתימות Defender במחשב." : running == false ? "שירות Defender אינו פועל." : "");
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        { return new(null, null, "לא ניתן לקרוא את מצב Defender: " + ex.Message); }
    }

    public async Task InstallAsync(Payload payload, CancellationToken token)
    {
        var status = ReadStatus();
        if (status.Version is null) throw new InvalidOperationException("לא זוהתה גרסת Defender. יש לפתור את בעיית הזיהוי לפני התקנה.");
        if (status.Version >= payload.Version) throw new InvalidOperationException("גרסת המחשב כבר שווה לגרסת הקובץ או חדשה ממנה.");
        var work = Path.Combine(Path.GetTempPath(), "MivtzarNaki", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(work);
        var staged = Path.Combine(work, "mpam-fe-x64.exe");
        try
        {
            await using (var input = File.OpenRead(payload.Path))
            await using (var output = new FileStream(staged, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, true))
                await input.CopyToAsync(output, token);
            await using var lockFile = new FileStream(staged, FileMode.Open, FileAccess.Read, FileShare.Read);
            var validated = await verifier.VerifyAsync(staged, token);
            if (validated.Sha256 != payload.Sha256 || validated.Version != payload.Version) throw new InvalidDataException("קובץ העדכון השתנה. ההתקנה בוטלה.");
            // No cancellation/forced termination once the installer is running.
            var info = new ProcessStartInfo(staged) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = work };
            info.ArgumentList.Add("-q");
            Process process;
            try { process = Process.Start(info) ?? throw new IOException("לא ניתן להפעיל את העדכון."); }
            catch (Win32Exception ex) when (ex.NativeErrorCode is 740 or 5) { throw new ElevationNeededException(); }
            using (process)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    if (!IsAdministrator() && process.ExitCode is 5 or 740) throw new ElevationNeededException();
                    throw new InvalidOperationException($"מתקין Defender החזיר קוד {process.ExitCode}. לא בוצע תיקון אוטומטי.");
                }
            }
            await VerifyTargetAsync(payload.Version);
        }
        finally
        {
            try { File.Delete(staged); Directory.Delete(work); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Debug.WriteLine(ex); }
        }
    }

    public async Task VerifyTargetAsync(Version target)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (UpdatePolicy.ReachedTarget(ReadStatus().Version, target)) return;
            await Task.Delay(500);
        }
        throw new InvalidOperationException("המתקין הסתיים, אך Defender עדיין לא מציג את גרסת היעד. יש לבדוק את מצב ההגנה.");
    }

    public async Task InstallElevatedAsync(Payload payload)
    {
        var code = await RunElevatedAsync("--install-local", payload.Path, payload.Sha256);
        if (code != 0) throw new InvalidOperationException($"ההתקנה בהרשאות מנהל לא הושלמה (קוד {code}).");
        await VerifyTargetAsync(payload.Version);
    }

    public static async Task StartServiceWithConsentAsync()
    {
        var code = await RunElevatedAsync("--start-defender");
        if (code != 0) throw new InvalidOperationException("Windows לא אפשר להפעיל את Defender. ייתכן שהוא מנוהל במדיניות או על ידי אנטי־וירוס אחר.");
    }

    public static async Task<int> RunWorkerAsync(string[] args)
    {
        try
        {
            if (args is ["--start-defender"])
            {
                using var service = new System.ServiceProcess.ServiceController("WinDefend");
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    service.Start();
                    await Task.Run(() => service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, TimeSpan.FromSeconds(20)));
                }
                return 0;
            }
            if (args is ["--install-local", var path, var hash] && PayloadStore.IsHash(hash))
            {
                var verifier = new MicrosoftSignatureVerifier();
                var payload = await verifier.VerifyAsync(path, CancellationToken.None);
                if (!payload.Sha256.Equals(hash, StringComparison.OrdinalIgnoreCase)) return 3;
                await new DefenderSystem(verifier).InstallAsync(payload, CancellationToken.None);
                return 0;
            }
            return 2;
        }
        catch (Exception ex) { Debug.WriteLine(ex); return 1; }
    }

    private static async Task<int> RunElevatedAsync(params string[] args)
    {
        var info = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = true, Verb = "runas", WindowStyle = ProcessWindowStyle.Hidden };
        foreach (var arg in args) info.ArgumentList.Add(arg);
        using var process = Process.Start(info) ?? throw new IOException("לא ניתן להפעיל את הפעולה בהרשאות מנהל.");
        await process.WaitForExitAsync();
        return process.ExitCode;
    }
    public static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
