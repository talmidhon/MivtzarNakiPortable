using System.IO.Compression;
using System.Text.Json;

namespace MivtzarNaki.Core;

public sealed record FolderManifest(string Version, Dictionary<string, string> Files);

public static class FolderPackage
{
    public const string ManifestName = "portable.manifest.json";
    private static string SafePath(string root, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\\') || name.Contains(':') || name.StartsWith('/') ||
            name.Split('/').Any(s => s is "" or "." or ".." || s.EndsWith('.') || s.EndsWith(' ') ||
                s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)) throw new InvalidDataException("נתיב לא בטוח בחבילת התוכנה.");
        var path = Path.GetFullPath(Path.Combine(root, name.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("נתיב מחוץ לתיקיית התוכנה.");
        return path;
    }
    public static void RejectLinks(string root)
    {
        for (var directory = new DirectoryInfo(root); directory is not null; directory = directory.Parent)
            if (directory.Exists && directory.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new IOException("אין לעדכן דרך קישור תיקייה.");
        if (!Directory.Exists(root)) return;
        foreach (var entry in new DirectoryInfo(root).EnumerateFileSystemInfos())
        {
            if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new IOException("קישור אינו מותר בחבילת התוכנה.");
            if (entry is DirectoryInfo child) RejectLinks(child.FullName);
        }
    }
    public static async Task<FolderManifest> ValidateAsync(string directory, CancellationToken token)
    {
        RejectLinks(directory);
        var manifestPath = Path.Combine(directory, ManifestName);
        if (new FileInfo(manifestPath).Length > 1024 * 1024) throw new InvalidDataException("מניפסט גדול מדי.");
        var manifest = JsonSerializer.Deserialize<FolderManifest>(await File.ReadAllTextAsync(manifestPath, token)) ?? throw new InvalidDataException("חסר מניפסט.");
        if (!Version.TryParse(manifest.Version, out _) || manifest.Files.Count is < 3 or > 5000 ||
            !manifest.Files.ContainsKey("MivtzarNaki.exe") || !manifest.Files.ContainsKey("MivtzarNaki.dll") ||
            !manifest.Files.ContainsKey("MivtzarNaki.runtimeconfig.json")) throw new InvalidDataException("חבילת התוכנה אינה שלמה.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, hash) in manifest.Files)
        {
            var path = SafePath(directory, name);
            if (!names.Add(name) || !PayloadStore.IsHash(hash) || name.Equals(ManifestName, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("מניפסט לא תקין.");
            if (!string.Equals(await PayloadStore.HashAsync(path, token), hash, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("חבילת התוכנה חלקית או פגומה.");
        }
        var actual = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(directory, p).Replace('\\', '/')).Where(p => p != ManifestName);
        if (!names.SetEquals(actual)) throw new InvalidDataException("קבצים לא צפויים בחבילת התוכנה.");
        return manifest;
    }
    public static async Task ExtractAsync(string archive, string destination, string hash, CancellationToken token)
    {
        if (Directory.Exists(destination)) throw new IOException("תיקיית ההכנה כבר קיימת.");
        RejectLinks(Path.GetDirectoryName(destination)!);
        await using var input = new FileStream(archive, FileMode.Open, FileAccess.Read, FileShare.Read);
        var actualHash = Convert.ToHexString(await System.Security.Cryptography.SHA256.HashDataAsync(input, token));
        if (!actualHash.Equals(hash, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("אימות חבילת התוכנה נכשל.");
        input.Position = 0;
        using var zip = new ZipArchive(input, ZipArchiveMode.Read);
        if (zip.Entries.Count > 5000) throw new InvalidDataException("יותר מדי קבצים בחבילה.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long size = 0;
        foreach (var entry in zip.Entries)
        {
            // Delivery ZIP contains App/ only; mutable user data is never accepted.
            if (!entry.FullName.StartsWith("App/", StringComparison.Ordinal) || entry.FullName.EndsWith('/') ||
                ((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000 || (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("מבנה חבילת התוכנה אינו תקין.");
            var name = entry.FullName[4..];
            _ = SafePath(destination, name);
            if (!names.Add(name) || (size += entry.Length) > 1024L * 1024 * 1024) throw new InvalidDataException("חבילה גדולה מדי או כפולה.");
        }
        Directory.CreateDirectory(destination);
        foreach (var entry in zip.Entries)
        {
            token.ThrowIfCancellationRequested();
            var path = SafePath(destination, entry.FullName[4..]);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var source = entry.Open();
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await source.CopyToAsync(output, token);
        }
        await ValidateAsync(destination, token);
    }

    // Only application directories participate. Data, OfflinePayloads and other root files are untouched.
    public static async Task ReplaceAsync(string root, string staged, Func<string, Task> verifyLaunch, CancellationToken token, TimeSpan? lockBudget = null)
    {
        RejectLinks(root);
        await ValidateAsync(staged, token);
        var current = Path.Combine(root, "App");
        var backup = Path.Combine(root, ".updates", "previous-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
        // Detect locked files before moving anything; no forced termination of another app instance.
        var wait = System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                foreach (var file in Directory.GetFiles(current, "*", SearchOption.AllDirectories))
                {
                    try { using var check = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
                    catch (IOException ex) { throw new IOException($"Cannot release application file: {file}. {ex.Message}", ex.HResult); }
                }
                token.ThrowIfCancellationRequested();
                Directory.Move(current, backup);
                break;
            }
            catch (IOException ex) when ((ex.HResult & 0xffff) is 32 or 33 && wait.Elapsed < (lockBudget ?? TimeSpan.Zero))
            {
                var remaining = (lockBudget ?? TimeSpan.Zero) - wait.Elapsed;
                if (remaining > TimeSpan.Zero) await Task.Delay(remaining < TimeSpan.FromMilliseconds(200) ? remaining : TimeSpan.FromMilliseconds(200), token);
            }
        }
        try
        {
            Directory.Move(staged, current);
            await verifyLaunch(Path.Combine(current, "MivtzarNaki.exe"));
        }
        catch
        {
            if (Directory.Exists(current)) Directory.Move(current, staged);
            Directory.Move(backup, current);
            throw;
        }
    }
}
