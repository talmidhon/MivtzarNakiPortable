using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace MivtzarNaki.Core;

public sealed partial class MicrosoftCatalog(HttpClient http)
{
    public static readonly Uri DownloadUri = new("https://go.microsoft.com/fwlink/?LinkID=121721&clcid=0x409&arch=x64");
    public static readonly Uri VersionUri = new("https://www.microsoft.com/en-us/wdsi/defenderupdates");

    public Task<RemotePayload> CheckAsync(CancellationToken token) => MetadataRetry.RunAsync(CheckOnceAsync, token);

    private async Task<RemotePayload> CheckOnceAsync(CancellationToken token)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(HttpMethod.Head, DownloadUri);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
        MetadataRetry.EnsureSuccess(response);
        var uri = response.RequestMessage?.RequestUri ?? DownloadUri;
        EnsureMicrosoft(uri);
        var size = response.Content.Headers.ContentLength;
        var etag = response.Headers.ETag is { IsWeak: false } tag ? tag.ToString() : null;
        var modified = response.Content.Headers.LastModified;
        var rangeSupport = false;
        if (size > 0 && (etag is not null || modified is not null))
        {
            using var probe = new HttpRequestMessage(HttpMethod.Get, uri);
            probe.Headers.Range = new RangeHeaderValue(0, 0);
            SetValidator(probe, etag, modified);
            using var part = await http.SendAsync(probe, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            EnsureMicrosoft(part.RequestMessage?.RequestUri ?? uri);
            MetadataRetry.EnsureSuccess(part);
            var range = part.Content.Headers.ContentRange;
            rangeSupport = part.StatusCode == HttpStatusCode.PartialContent && range?.From == 0 && range.To == 0 && range.Length == size;
        }
        using var versionResponse = await http.GetAsync(VersionUri, deadline.Token);
        EnsureMicrosoft(versionResponse.RequestMessage?.RequestUri ?? VersionUri);
        MetadataRetry.EnsureSuccess(versionResponse);
        var version = ParseVersion(await versionResponse.Content.ReadAsStringAsync(deadline.Token));
        return new(uri, version, size, etag, modified, rangeSupport);
    }

    public static Version? ParseVersion(string html)
    {
        var text = WebUtility.HtmlDecode(Tags().Replace(html, " "));
        var match = VersionPattern().Match(text);
        return match.Success && Version.TryParse(match.Groups[1].Value, out var version) ? version : null;
    }
    public static void SetValidator(HttpRequestMessage request, string? etag, DateTimeOffset? modified)
    {
        if (etag is not null) request.Headers.IfRange = new RangeConditionHeaderValue(EntityTagHeaderValue.Parse(etag));
        else if (modified is not null) request.Headers.IfRange = new RangeConditionHeaderValue(modified.Value);
    }
    public static void EnsureMicrosoft(Uri uri)
    {
        if (uri.Scheme != "https" || !(uri.IdnHost.Equals("microsoft.com", StringComparison.OrdinalIgnoreCase) || uri.IdnHost.EndsWith(".microsoft.com", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("כתובת ההורדה אינה כתובת מאושרת של Microsoft.");
    }
    [GeneratedRegex("<[^>]+>")] private static partial Regex Tags();
    [GeneratedRegex(@"Latest security intelligence update[\s\S]*?\bVersion\s*:\s*(\d+\.\d+\.\d+\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex VersionPattern();
}
