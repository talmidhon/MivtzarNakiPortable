using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MivtzarNaki.Core;

public sealed class ResumableDownloader(HttpClient http, TimeSpan? retryDelay = null)
{
    private sealed class RangeRejectedException : IOException { }

    public async Task<string> DownloadAsync(RemotePayload remote, string directory, IProgress<TransferProgress> progress, CancellationToken token)
    {
        MicrosoftCatalog.EnsureMicrosoft(remote.Uri);
        PortableEnvironment.EnsureWritable(directory);
        // Cross-process lock covers resumable state and assembly. The UI separately locks its complete workflow.
        await using var gate = new FileStream(Path.Combine(directory, ".download.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try { return await DownloadInternalAsync(remote, directory, progress, token); }
        catch (RangeRejectedException)
        {
            progress.Report(new(0, remote.Size, "השרת אינו מאפשר המשך; מתחיל הורדה מלאה."));
            return await DownloadInternalAsync(remote with { Ranges = false }, directory, progress, token);
        }
    }

    private async Task<string> DownloadInternalAsync(RemotePayload remote, string directory, IProgress<TransferProgress> progress, CancellationToken token)
    {
        var work = Path.Combine(directory, ".download");
        Directory.CreateDirectory(work);
        var statePath = Path.Combine(work, "state.json");
        var count = remote.Resumable && remote.Size >= 8 * 1024 * 1024 ? 4 : 1;
        var desired = new DownloadState(remote.Identity, count);
        DownloadState? previous = null;
        try { if (File.Exists(statePath)) previous = JsonSerializer.Deserialize(await File.ReadAllTextAsync(statePath, token), JsonModels.Default.DownloadState); }
        catch (JsonException) { }
        if (!remote.Resumable || previous != desired) ClearParts(work);
        await File.WriteAllTextAsync(statePath, JsonSerializer.Serialize(desired, JsonModels.Default.DownloadState), token);
        if (remote.Size is > 0)
        {
            var retained = Directory.EnumerateFiles(work, "part-*.bin").Sum(p => new FileInfo(p).Length);
            var candidate = Path.Combine(directory, "candidate.exe");
            var reclaim = File.Exists(candidate) ? new FileInfo(candidate).Length : 0;
            if (new DriveInfo(Path.GetPathRoot(Path.GetFullPath(directory))!).AvailableFreeSpace + reclaim < remote.Size * 2 - retained + 1024 * 1024)
                throw new IOException("אין מספיק מקום בכונן להכנת העדכון. פנה מקום ונסה שוב.");
        }
        var completed = new long[count];
        using var siblings = CancellationTokenSource.CreateLinkedTokenSource(token);
        var tasks = Enumerable.Range(0, count).Select(async index =>
        {
            try
            {
                var start = remote.Size > 0 ? remote.Size.Value * index / count : 0;
                long? end = remote.Size > 0 ? remote.Size.Value * (index + 1) / count - 1 : null;
                await DownloadPartAsync(remote, Path.Combine(work, $"part-{index}.bin"), start, end, bytes =>
                {
                    Interlocked.Exchange(ref completed[index], bytes);
                    long total = 0;
                    for (var i = 0; i < completed.Length; i++) total += Interlocked.Read(ref completed[i]);
                    progress.Report(new(total, remote.Size, "מוריד עדכון ל־USB…"));
                }, siblings.Token);
            }
            catch { siblings.Cancel(); throw; }
        }).ToArray();
        try { await Task.WhenAll(tasks); }
        catch
        {
            token.ThrowIfCancellationRequested();
            if (tasks.Any(t => t.Exception?.Flatten().InnerExceptions.Any(e => e is RangeRejectedException) == true)) throw new RangeRejectedException();
            var failure = tasks.SelectMany(t => t.Exception?.Flatten().InnerExceptions ?? Enumerable.Empty<Exception>()).FirstOrDefault(e => e is not OperationCanceledException);
            if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            throw;
        }
        var outputPath = Path.Combine(directory, "candidate.exe");
        await using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 128 * 1024, true))
        {
            for (var i = 0; i < count; i++)
            {
                await using var input = File.OpenRead(Path.Combine(work, $"part-{i}.bin"));
                await input.CopyToAsync(output, token);
            }
            await output.FlushAsync(token);
            if (remote.Size is not null && output.Length != remote.Size) throw new InvalidDataException("גודל הקובץ שהורד אינו תואם לגודל שפורסם.");
        }
        return outputPath;
    }

    private async Task DownloadPartAsync(RemotePayload remote, string path, long start, long? end, Action<long> report, CancellationToken token)
    {
        long? expected = end is null ? null : end - start + 1;
        for (var attempt = 0; ; attempt++)
        {
            token.ThrowIfCancellationRequested();
            var offset = remote.Resumable && File.Exists(path) ? new FileInfo(path).Length : 0;
            if (expected is not null && offset > expected) { File.Delete(path); offset = 0; }
            report(offset);
            if (offset > 0 && offset == expected) return;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, remote.Uri);
                if (remote.Resumable)
                {
                    request.Headers.Range = new RangeHeaderValue(start + offset, end);
                    MicrosoftCatalog.SetValidator(request, remote.ETag, remote.Modified);
                }
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
                deadline.CancelAfter(TimeSpan.FromSeconds(30));
                using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
                MicrosoftCatalog.EnsureMicrosoft(response.RequestMessage?.RequestUri ?? remote.Uri);
                response.EnsureSuccessStatusCode();
                if (remote.Resumable)
                {
                    var range = response.Content.Headers.ContentRange;
                    if (response.StatusCode != HttpStatusCode.PartialContent) throw new RangeRejectedException();
                    if (range?.From != start + offset || range.To != end || range.Length != remote.Size)
                        throw new InvalidDataException("השרת החזיר מקטע שונה מהמבוקש. יש לבדוק את העדכון מחדש.");
                }
                if (remote.ETag is not null && response.Headers.ETag is not null && response.Headers.ETag.ToString() != remote.ETag)
                    throw new InvalidDataException("הקובץ בשרת התחלף בזמן ההורדה. יש לבדוק מחדש.");
                if (remote.ETag is null && remote.Modified is not null && response.Content.Headers.LastModified is not null && response.Content.Headers.LastModified != remote.Modified)
                    throw new InvalidDataException("הקובץ בשרת השתנה. יש לבדוק מחדש.");
                await using var input = await response.Content.ReadAsStreamAsync(deadline.Token);
                await using var output = new FileStream(path, offset > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 128 * 1024, true);
                var buffer = new byte[128 * 1024];
                long written = offset;
                while (true)
                {
                    deadline.CancelAfter(TimeSpan.FromSeconds(30));
                    var read = await input.ReadAsync(buffer, deadline.Token);
                    if (read == 0) break;
                    if (expected is not null && written + read > expected) throw new InvalidDataException("גודל המקטע חורג מהצפוי.");
                    await output.WriteAsync(buffer.AsMemory(0, read), token);
                    written += read;
                    report(written);
                }
                await output.FlushAsync(token);
                if (expected is not null && written != expected) throw new IOException("החיבור נותק לפני סיום ההורדה.");
                return;
            }
            catch (Exception ex) when (!token.IsCancellationRequested && attempt < 2 && ex is not RangeRejectedException && ex is not InvalidDataException && ex is IOException or HttpRequestException or OperationCanceledException)
            {
                await Task.Delay((retryDelay ?? TimeSpan.FromSeconds(1)) * (1 << attempt), token);
            }
        }
    }

    private static void ClearParts(string work)
    {
        foreach (var path in Directory.EnumerateFiles(work, "part-*.bin")) File.Delete(path);
    }
    public static void ClearCompleted(string directory)
    {
        var work = Path.Combine(directory, ".download");
        if (Directory.Exists(work)) ClearParts(work);
    }
}
