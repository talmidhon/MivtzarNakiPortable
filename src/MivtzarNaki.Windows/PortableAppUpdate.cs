using System.Diagnostics;
using MivtzarNaki.Core;

namespace MivtzarNaki.Windows;

public static class PortableAppUpdate
{
    public static void StartReplacement(string candidate, string hash)
    {
        var application = Path.GetDirectoryName(Environment.ProcessPath)!;
        if (Path.GetFileName(application) != "App" || !File.Exists(Path.Combine(application, FolderPackage.ManifestName)))
            throw new InvalidOperationException("עדכון מבצר נקי זמין מתוך תיקיית ההפצה הניידת.");
        var helperDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MivtzarNaki", "Updater", Guid.NewGuid().ToString("N"));
        FolderPackage.RejectLinks(application);
        foreach (var source in Directory.GetFiles(application, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(helperDirectory, Path.GetRelativePath(application, source));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination);
        }
        var start = new ProcessStartInfo(Path.Combine(helperDirectory, "MivtzarNaki.exe")) { UseShellExecute = false, CreateNoWindow = true };
        foreach (var arg in new[] { "--apply-update", Environment.ProcessId.ToString(), Environment.ProcessPath!, candidate, hash }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new IOException("לא ניתן להתחיל את החלפת התוכנה.");
    }

    public static async Task<int> ApplyAsync(string[] args)
    {
        if (args is not ["--apply-update", var parentText, var targetText, var candidateText, var hash] || !int.TryParse(parentText, out var parent) || !PayloadStore.IsHash(hash)) return 2;
        var target = Path.GetFullPath(targetText);
        var application = Path.GetDirectoryName(target)!;
        var root = Path.GetDirectoryName(application)!;
        var candidate = Path.GetFullPath(candidateText);
        if (Path.GetFileName(application) != "App" || Path.GetFileName(target) != "MivtzarNaki.exe" || !candidate.Equals(Path.Combine(root, ".updates", "MivtzarNaki.next.zip"), StringComparison.OrdinalIgnoreCase)) return 3;
        var staged = Path.Combine(root, ".updates", "stage-" + Guid.NewGuid().ToString("N"));
        try
        {
            FolderPackage.RejectLinks(root);
            await FolderPackage.ExtractAsync(candidate, staged, hash, default);
            var incoming = await FolderPackage.ValidateAsync(staged, default);
            var current = await FolderPackage.ValidateAsync(application, default);
            if (Version.Parse(incoming.Version) <= Version.Parse(current.Version)) throw new InvalidDataException("גרסת מבצר נקי אינה חדשה יותר.");
            try
            {
                using var parentProcess = Process.GetProcessById(parent);
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                await parentProcess.WaitForExitAsync(deadline.Token);
            }
            catch (ArgumentException) { }
            await FolderPackage.ReplaceAsync(root, staged, async executable =>
            {
                var acknowledgement = Path.Combine(root, ".updates", "started-" + Guid.NewGuid().ToString("N"));
                var start = new ProcessStartInfo(executable) { UseShellExecute = false, WorkingDirectory = root };
                start.ArgumentList.Add("--update-ack");
                start.ArgumentList.Add(acknowledgement);
                using var app = Process.Start(start) ?? throw new IOException("לא ניתן לפתוח את הגרסה החדשה.");
                try
                {
                    for (var n = 0; n < 100; n++)
                    {
                        if (File.Exists(acknowledgement)) { File.Delete(acknowledgement); return; }
                        if (app.HasExited) break;
                        await Task.Delay(200);
                    }
                    throw new IOException("הגרסה החדשה לא אישרה טעינת ממשק. הגיבוי משוחזר.");
                }
                catch
                {
                    if (!app.HasExited) { app.Kill(); await app.WaitForExitAsync(); }
                    throw;
                }
            }, default);
            File.Delete(candidate);
            File.Delete(Path.Combine(root, ".updates", "update-error.log"));
            return 0;
        }
        catch (Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(root, ".updates"));
                File.WriteAllText(Path.Combine(root, ".updates", "update-error.log"), ex.Message);
                if (File.Exists(target))
                { using var restored = Process.Start(new ProcessStartInfo(target) { UseShellExecute = false, WorkingDirectory = root }); }
            }
            catch (Exception restoreError) { Debug.WriteLine(restoreError); }
            return 1;
        }
    }
}
