using System.Security.Cryptography;
using System.Text.Json;

namespace MivtzarNaki.Core;

public sealed class PayloadStore(string directory, IPayloadVerifier verifier)
{
    private string Pointer => Path.Combine(directory, "current.json");

    public async Task<Payload?> ReadAsync(CancellationToken token)
    {
        if (!File.Exists(Pointer)) return null;
        var record = JsonSerializer.Deserialize(await File.ReadAllTextAsync(Pointer, token), JsonModels.Default.PayloadRecord)
            ?? throw new InvalidDataException("מידע העדכון ב־USB פגום.");
        if (!IsHash(record.Sha256)) throw new InvalidDataException("מזהה הקובץ המקומי אינו תקין.");
        var path = Path.Combine(directory, record.Sha256, "mpam-fe-x64.exe");
        var payload = await verifier.VerifyAsync(path, token);
        if (!payload.Sha256.Equals(record.Sha256, StringComparison.OrdinalIgnoreCase) || payload.Size != record.Size || payload.Version.ToString() != record.Version)
            throw new InvalidDataException("קובץ העדכון השתנה מאז שנשמר. יש להכין אותו מחדש במחשב המחובר.");
        return payload;
    }

    public async Task<Payload> CommitAsync(string candidate, Version? expectedVersion, CancellationToken token)
    {
        var payload = await verifier.VerifyAsync(candidate, token);
        if (expectedVersion is not null && payload.Version < expectedVersion) throw new InvalidDataException("הקובץ שהורד ישן מהגרסה שפורסמה. יש לבדוק שוב.");
        Payload? previous = null;
        try { previous = await ReadAsync(token); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or JsonException or System.Security.Cryptography.CryptographicException) { /* Allow recovery from corrupt local data. */ }
        if (previous is not null && previous.Version > payload.Version) throw new InvalidDataException("לא ניתן להחליף עדכון בגרסה ישנה יותר.");
        if (!IsHash(payload.Sha256)) throw new InvalidDataException("גיבוב הקובץ אינו תקין.");
        var destinationDirectory = Path.Combine(directory, payload.Sha256);
        Directory.CreateDirectory(destinationDirectory);
        var destination = Path.Combine(destinationDirectory, "mpam-fe-x64.exe");
        // Immutable content-addressed payloads + one pointer avoid mismatched payload/metadata after USB removal.
        File.Move(candidate, destination, true);
        var record = new PayloadRecord(payload.Version.ToString(), payload.Size, payload.Sha256);
        var temp = Pointer + ".new";
        await File.WriteAllTextAsync(temp, JsonSerializer.Serialize(record, JsonModels.Default.PayloadRecord), token);
        token.ThrowIfCancellationRequested();
        File.Move(temp, Pointer, true);
        // Retain current and previous payload; remove only our exact content-addressed files.
        foreach (var oldDirectory in Directory.EnumerateDirectories(directory))
        {
            var hash = Path.GetFileName(oldDirectory);
            if (!IsHash(hash) || hash.Equals(payload.Sha256, StringComparison.OrdinalIgnoreCase) || hash.Equals(previous?.Sha256, StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                if ((File.GetAttributes(oldDirectory) & FileAttributes.ReparsePoint) != 0) continue;
                File.Delete(Path.Combine(oldDirectory, "mpam-fe-x64.exe"));
                if (!Directory.EnumerateFileSystemEntries(oldDirectory).Any()) Directory.Delete(oldDirectory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(ex); }
        }
        return payload with { Path = destination };
    }

    public static bool IsHash(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);
    public static async Task<string> HashAsync(string path, CancellationToken token)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, true);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream, token));
    }
}
