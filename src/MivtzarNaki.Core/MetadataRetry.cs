using System.Net;
using System.Diagnostics;

namespace MivtzarNaki.Core;

public sealed class TemporaryServerException(TimeSpan? retryAfter) : HttpRequestException
{
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

public static class MetadataRetry
{
    public static async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken token,
        TimeSpan? budget = null, TimeSpan? attemptLimit = null, TimeSpan? delay = null)
    {
        var total = budget ?? TimeSpan.FromSeconds(15);
        var clock = Stopwatch.StartNew();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(total);
        for (var attempt = 0; ; attempt++)
        {
            using var single = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
            single.CancelAfter(attemptLimit ?? TimeSpan.FromSeconds(4));
            try { return await action(single.Token); }
            catch (Exception ex) when (attempt < 2 && !deadline.IsCancellationRequested && IsTransient(ex))
            {
                var pause = ex is TemporaryServerException { RetryAfter: { } retry } ? retry : (delay ?? TimeSpan.FromMilliseconds(300)) * (attempt + 1);
                if (pause < TimeSpan.Zero) pause = TimeSpan.Zero;
                if (pause >= total - clock.Elapsed) throw;
                await Task.Delay(pause, deadline.Token);
            }
        }
    }
    private static bool IsTransient(Exception ex) => ex is OperationCanceledException ||
        ex is HttpRequestException request && request.HttpRequestError != HttpRequestError.SecureConnectionError &&
        (request.StatusCode is null || IsTemporary(request.StatusCode.Value));
    private static bool IsTemporary(HttpStatusCode code) => code is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;
    public static void EnsureSuccess(HttpResponseMessage response)
    {
        if (IsTemporary(response.StatusCode))
            throw new TemporaryServerException(response.Headers.RetryAfter?.Delta ?? (response.Headers.RetryAfter?.Date - DateTimeOffset.UtcNow));
        response.EnsureSuccessStatusCode();
    }
}
