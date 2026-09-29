using System.Text.Json;

namespace MivtzarNaki.Core;

public sealed class AppUpdates(HttpClient http, Uri feed)
{
    private string Repository
    {
        get
        {
            if (feed.Scheme != "https" || feed.Host != "raw.githubusercontent.com") throw new InvalidDataException("מקור עדכון התוכנה חייב להיות קובץ ב־GitHub.");
            var segments = feed.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 4 || segments[0] != "talmidhon") throw new InvalidDataException("מקור עדכון התוכנה אינו במאגר של המפתח.");
            return $"{segments[0]}/{segments[1]}";
        }
    }

    public Task<AppRelease?> CheckAsync(Version current, CancellationToken token) => MetadataRetry.RunAsync(ct => CheckOnceAsync(current, ct), token);
    private async Task<AppRelease?> CheckOnceAsync(Version current, CancellationToken token)
    {
        _ = Repository;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        using var request = new HttpRequestMessage(HttpMethod.Get, feed);
        request.Headers.CacheControl = new() { NoCache = true };
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        MetadataRetry.EnsureSuccess(response);
        if (response.RequestMessage?.RequestUri is { } finalFeed && (finalFeed.Scheme != "https" || finalFeed.Host != feed.Host || finalFeed.AbsolutePath != feed.AbsolutePath))
            throw new InvalidDataException("מקור מידע העדכון הפנה לכתובת אחרת.");
        await using var input = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var bounded = new MemoryStream();
        var buffer = new byte[4096];
        int read;
        while ((read = await input.ReadAsync(buffer, timeout.Token)) > 0)
        {
            if (bounded.Length + read > 64 * 1024) throw new InvalidDataException("מידע גרסת התוכנה גדול מהצפוי.");
            bounded.Write(buffer, 0, read);
        }
        var release = JsonSerializer.Deserialize(bounded.ToArray(), JsonModels.Default.AppRelease) ?? throw new InvalidDataException("מידע עדכון התוכנה חסר.");
        Validate(release);
        return Version.Parse(release.Version.TrimStart('v')) > current ? release : null;
    }

    public void Validate(AppRelease release)
    {
        if (!Version.TryParse(release.Version?.TrimStart('v'), out _) || !PayloadStore.IsHash(release.Sha256)) throw new InvalidDataException("מידע גרסה או אימות חסר בעדכון התוכנה.");
        if (!Uri.TryCreate(release.DownloadUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.Host != "github.com" || uri.AbsolutePath != $"/{Repository}/releases/download/v{release.Version.TrimStart('v')}/MivtzarNaki-win-x64.zip" || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0)
            throw new InvalidDataException("קישור עדכון התוכנה אינו שייך למאגר ההפצה שהוגדר.");
    }

    public async Task<string> StageAsync(AppRelease release, string directory, IProgress<TransferProgress> progress, CancellationToken token)
    {
        Validate(release);
        PortableEnvironment.EnsureWritable(directory);
        var path = Path.Combine(directory, "MivtzarNaki.next.zip");
        try
        {
        using var headersTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        headersTimeout.CancelAfter(TimeSpan.FromSeconds(30));
        using var response = await http.GetAsync(release.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, headersTimeout.Token);
        response.EnsureSuccessStatusCode();
        var final = response.RequestMessage?.RequestUri;
        if (final is not null && (final.Scheme != "https" || !(final.Host == "github.com" || final.Host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase))))
            throw new InvalidDataException("השרת הפנה למקור עדכון לא מאושר.");
        await using (var input = await response.Content.ReadAsStreamAsync(token))
        await using (var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 128 * 1024, true))
        {
            var buffer = new byte[128 * 1024];
            int read;
            using var stall = CancellationTokenSource.CreateLinkedTokenSource(token);
            while (true)
            {
                stall.CancelAfter(TimeSpan.FromSeconds(30));
                read = await input.ReadAsync(buffer, stall.Token);
                if (read == 0) break;
                await output.WriteAsync(buffer.AsMemory(0, read), token);
                if (output.Length > 512L * 1024 * 1024) throw new InvalidDataException("חבילת התוכנה גדולה מהצפוי.");
                progress.Report(new(output.Length, response.Content.Headers.ContentLength, "מוריד את גרסת התוכנה החדשה…"));
            }
        }
        if (!string.Equals(await PayloadStore.HashAsync(path, token), release.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(path);
            throw new InvalidDataException("אימות עדכון התוכנה נכשל. הגרסה הנוכחית נשמרה.");
        }
        return path;
        }
        catch
        {
            // An incomplete app download must never look like a verified candidate.
            try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            throw;
        }
    }
}
