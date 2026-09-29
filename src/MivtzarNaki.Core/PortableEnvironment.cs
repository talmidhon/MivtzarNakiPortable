using System.Text;
using System.Text.Json;

namespace MivtzarNaki.Core;

public sealed class PortableEnvironment
{
    public string Root { get; }
    public string Payloads => Path.Combine(Root, "OfflinePayloads");
    public string Data { get; }
    public string SettingsPath => Path.Combine(Data, "settings.json");
    public bool UsingLocalFallback => !Data.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    public PortableEnvironment(string? root = null)
    {
        Root = Path.GetFullPath(root ?? Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory);
        if (root is null && Path.GetFileName(Root).Equals("App", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(Root, FolderPackage.ManifestName))) Root = Path.GetDirectoryName(Root)!;
        Data = Path.Combine(Root, "Data");
        try { EnsureWritable(Data); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MivtzarNaki");
            EnsureWritable(Data);
        }
    }

    public static void EnsureWritable(string directory)
    {
        Directory.CreateDirectory(directory);
        using var probe = new FileStream(Path.Combine(directory, $".probe-{Guid.NewGuid():N}"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose);
    }

    public AppSettings LoadSettings()
    {
        try { return File.Exists(SettingsPath) ? JsonSerializer.Deserialize(File.ReadAllText(SettingsPath), JsonModels.Default.AppSettings) ?? new() : new(); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return new(); }
    }
    public void SaveSettings(AppSettings settings)
    {
        var temp = SettingsPath + ".new";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, JsonModels.Default.AppSettings));
        File.Move(temp, SettingsPath, true);
    }
}

public sealed class ActivityLog(PortableEnvironment environment)
{
    private readonly object _gate = new();
    public event Action<string>? Added;
    public void Write(string message, Exception? error = null)
    {
        if (message.Length > 2000) message = message[..2000];
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        lock (_gate)
        {
            try
            {
                var path = Path.Combine(environment.Data, "diagnostic.log");
                if (File.Exists(path) && new FileInfo(path).Length > 512 * 1024) File.Move(path, path + ".previous", true);
                var detail = error?.Message;
                if (detail?.Length > 2000) detail = detail[..2000];
                File.AppendAllText(path, line + (detail is null ? "" : $" | {detail}") + "\n", Encoding.UTF8);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(ex); }
        }
        Added?.Invoke(line);
    }
}
