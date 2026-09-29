using System.Net;
using System.Text.Json;
using MivtzarNaki.Core;
using MivtzarNaki.Windows;
using Xunit;

namespace MivtzarNaki.Tests;

public sealed class DistributionTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "live-feed-fixture-" + Guid.NewGuid().ToString("N"));
    public DistributionTests() => Directory.CreateDirectory(root);
    public void Dispose() => Directory.Delete(root, true);
    private static AppRelease Release => new("0.1.1", "https://github.com/" + Distribution.Repository + "/releases/download/v0.1.1/MivtzarNaki-win-x64.zip", new string('A', 64), "fixture");
    private static HttpResponseMessage Metadata(HttpRequestMessage request) => new(HttpStatusCode.OK) { RequestMessage = request, Content = new StringContent(JsonSerializer.Serialize(Release, JsonModels.Default.AppRelease)) };

    [Theory]
    [InlineData("0.1.0", true)]
    [InlineData("0.1.1", false)]
    [InlineData("0.2.0", false)]
    public async Task RealFeedSchemaOnlyOffersNewerVersion(string current, bool available)
    {
        var downloads = 0;
        using var http = new HttpClient(new Handler((r, _) => { Assert.Equal(Distribution.UpdateFeed, r.RequestUri!.AbsoluteUri); if (r.RequestUri.Host == "github.com") downloads++; return Task.FromResult(Metadata(r)); }));
        var result = await new AppUpdates(http, new(Distribution.UpdateFeed)).CheckAsync(Version.Parse(current), default);
        Assert.Equal(available, result is not null);
        Assert.Equal(0, downloads);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, 1)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 3)]
    public async Task MissingFeedOrUnavailableGithubDoesNotClaimCurrent(HttpStatusCode code, int attempts)
    {
        var calls = 0;
        using var http = new HttpClient(new Handler((r, _) => { calls++; return Task.FromResult(new HttpResponseMessage(code) { RequestMessage = r }); }));
        using var session = new UpdateSession(http, root);
        await session.CheckNetworkAsync(default);
        Assert.Null(session.NewApp);
        Assert.Contains("לא ניתן", session.AppMessage);
        // Both independent metadata checks fail with bounded attempts.
        Assert.Equal(attempts * 2, calls);
        Assert.Empty(Directory.GetDirectories(root, ".updates"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.UpdateAppAsync(new Progress<TransferProgress>(), default));
    }

    [Fact]
    public async Task SessionUsesDefaultForLegacyEmptySettingsAndPreservesUserSettings()
    {
        new PortableEnvironment(root).SaveSettings(new AppSettings("Dark", ""));
        var downloads = 0;
        using var http = new HttpClient(new Handler((r, _) =>
        {
            if (r.RequestUri!.Host == "github.com") downloads++;
            return Task.FromResult(r.RequestUri.Host == "raw.githubusercontent.com" ? Metadata(r) : new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = r });
        }));
        using var session = new UpdateSession(http, root);
        Assert.Equal(Distribution.UpdateFeed, session.EffectiveUpdateFeed);
        await session.CheckNetworkAsync(default);
        Assert.Null(session.NewApp);
        Assert.Contains("מעודכן", session.AppMessage);
        Assert.Equal(0, downloads);
        Assert.Equal("Dark", session.Settings.Theme);
        Assert.Equal("", new PortableEnvironment(root).LoadSettings().UpdateFeed);
        session.Settings = session.Settings with { UpdateFeed = "https://raw.githubusercontent.com/talmidhon/custom/main/version.json" };
        Assert.Contains("/custom/", session.EffectiveUpdateFeed);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("{\"latest_version\":\"0.1.1\"}")]
    public async Task MalformedMetadataIsNotRetried(string body)
    {
        var calls = 0;
        using var http = new HttpClient(new Handler((r, _) => { calls++; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = r, Content = new StringContent(body) }); }));
        await Assert.ThrowsAnyAsync<Exception>(() => new AppUpdates(http, new(Distribution.UpdateFeed)).CheckAsync(new(0, 1, 0), default));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void MetadataCannotPointToWrongTagOrMissingAsset()
    {
        using var http = new HttpClient();
        var updates = new AppUpdates(http, new(Distribution.UpdateFeed));
        Assert.Throws<InvalidDataException>(() => updates.Validate(Release with { DownloadUrl = Release.DownloadUrl.Replace("v0.1.1/", "v0.1.0/") }));
        Assert.Throws<InvalidDataException>(() => updates.Validate(Release with { DownloadUrl = "" }));
        Assert.Throws<InvalidDataException>(() => updates.Validate(Release with { Sha256 = "" }));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.OK)]
    public async Task MissingAssetOrChecksumFailurePreservesExistingApp(HttpStatusCode code)
    {
        var app = Path.Combine(root, "App"); Directory.CreateDirectory(app);
        var current = Path.Combine(app, "MivtzarNaki.exe"); await File.WriteAllTextAsync(current, "current");
        using var http = new HttpClient(new Handler((r, _) => Task.FromResult(new HttpResponseMessage(code) { RequestMessage = r, Content = new ByteArrayContent([1, 2, 3]) })));
        await Assert.ThrowsAnyAsync<Exception>(() => new AppUpdates(http, new(Distribution.UpdateFeed)).StageAsync(Release, Path.Combine(root, ".updates"), new Progress<TransferProgress>(), default));
        Assert.Equal("current", await File.ReadAllTextAsync(current));
        Assert.False(File.Exists(Path.Combine(root, ".updates", "MivtzarNaki.next.zip")));
    }

    [Fact]
    public async Task MetadataTimeoutAndUserCancellationRemainBounded()
    {
        using var http = new HttpClient(new Handler(async (_, token) => { await Task.Delay(Timeout.Infinite, token); throw new Exception("unreachable"); }));
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new AppUpdates(http, new(Distribution.UpdateFeed)).CheckAsync(new(0, 1, 0), cancel.Token));
        using var timeout = new HttpClient(new Handler((_, _) => throw new TaskCanceledException("network timeout")));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new AppUpdates(timeout, new(Distribution.UpdateFeed)).CheckAsync(new(0, 1, 0), default));
    }

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request, token); }
}
