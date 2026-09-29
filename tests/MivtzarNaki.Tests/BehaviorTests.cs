using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using MivtzarNaki.Core;
using MivtzarNaki.Windows;
using Xunit;

namespace MivtzarNaki.Tests;

public sealed class BehaviorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "MivtzarTests", Guid.NewGuid().ToString("N"));
    private static readonly IProgress<TransferProgress> Progress = new InlineProgress(_ => { });
    private static RemotePayload Remote(int size) => new(new("https://download.microsoft.com/test.exe"), new(1, 2, 3, 4), size, "\"one\"", null, true);
    public BehaviorTests() => Directory.CreateDirectory(_root);
    public void Dispose() => Directory.Delete(_root, true);

    [Fact]
    public async Task OfflineSessionRetainsLocalOperationWithoutOfferingDownload()
    {
        using var session = new UpdateSession(new HttpClient(new Handler(_ => throw new HttpRequestException("offline"))), _root);
        await session.ReadLocalAsync(default);
        await session.CheckNetworkAsync(default);
        Assert.Null(session.Remote);
        Assert.False(session.CanDownload);
        Assert.Contains("מקומי", session.NetworkMessage);
        Assert.True(Directory.Exists(session.Environment.Data));
    }

    [Fact]
    public async Task ReplacementRejectsCorruptCandidateBeforeTouchingCurrentExecutable()
    {
        Directory.CreateDirectory(Path.Combine(_root, "App"));
        var target = Path.Combine(_root, "App", "MivtzarNaki.exe");
        var directory = Path.Combine(_root, ".updates");
        Directory.CreateDirectory(directory);
        var candidate = Path.Combine(directory, "MivtzarNaki.next.zip");
        await File.WriteAllTextAsync(target, "current fixture");
        await File.WriteAllTextAsync(candidate, "corrupt fixture");
        var code = await PortableAppUpdate.ApplyAsync(["--apply-update", int.MaxValue.ToString(), target, candidate, new string('A', 64)]);
        Assert.Equal(1, code);
        Assert.Equal("current fixture", await File.ReadAllTextAsync(target));
        Assert.False(File.Exists(target + ".previous"));
    }

    [Fact]
    public void CatalogDoesNotMistakeEngineVersionForSignatureVersion()
    {
        Assert.Equal(new Version(1, 459, 442, 0), MicrosoftCatalog.ParseVersion("Engine 4.18.2.0 <h2>Latest security intelligence update</h2><b>Version:</b> 1.459.442.0"));
        Assert.Null(MicrosoftCatalog.ParseVersion("Engine version: 4.18.2.0"));
        Assert.Throws<InvalidDataException>(() => MicrosoftCatalog.EnsureMicrosoft(new("https://microsoft.com.evil.example/file")));
    }

    [Fact]
    public async Task TemporaryDisconnectRetriesAndCompletes()
    {
        var calls = 0;
        var bytes = new byte[1000];
        using var http = new HttpClient(new Handler(r => ++calls == 1 ? throw new HttpRequestException("disconnected") : Response(r, bytes)));
        var file = await new ResumableDownloader(http, TimeSpan.Zero).DownloadAsync(Remote(bytes.Length), _root, Progress, default);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(file));
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task TruncatedResponseResumesAtLastWrittenByte()
    {
        var bytes = RandomNumberGenerator.GetBytes(1000);
        var starts = new List<long>();
        using var http = new HttpClient(new Handler(r =>
        {
            starts.Add(r.Headers.Range!.Ranges.Single().From!.Value);
            var response = Response(r, bytes);
            if (starts.Count == 1)
            {
                response.Content = new ByteArrayContent(bytes[..400]);
                response.Content.Headers.ContentRange = new(0, 999, 1000);
            }
            return response;
        }));
        var file = await new ResumableDownloader(http, TimeSpan.Zero).DownloadAsync(Remote(bytes.Length), _root, Progress, default);
        Assert.Equal(new long[] { 0, 400 }, starts);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(file));
    }

    [Fact]
    public async Task NewVerifiedDownloadRecoversFromCorruptPointer()
    {
        await File.WriteAllTextAsync(Path.Combine(_root, "current.json"), "{\"Version\":\"1.0.0.0\",\"Size\":1,\"Sha256\":\"invalid\"}");
        var candidate = Path.Combine(_root, "fixture");
        await File.WriteAllTextAsync(candidate, "1.2.3.4");
        var store = new PayloadStore(_root, new FixtureVerifier());
        await store.CommitAsync(candidate, null, default);
        Assert.Equal(new Version(1, 2, 3, 4), (await store.ReadAsync(default))!.Version);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("1.2.3.3", true)]
    [InlineData("1.2.3.4", false)]
    [InlineData("1.2.3.5", false)]
    public void InstallationRequiresKnownOlderVersion(string? installed, bool expected)
    {
        var payload = new Payload("unused", new(1, 2, 3, 4), 1, "");
        Assert.Equal(expected, UpdatePolicy.CanInstall(payload, new(installed is null ? null : Version.Parse(installed), true, "")));
        Assert.False(UpdatePolicy.CanInstall(payload, new(new(1, 0), false, "")));
        Assert.False(UpdatePolicy.ReachedTarget(null, payload.Version));
        Assert.False(UpdatePolicy.ReachedTarget(new(1, 2, 3, 3), payload.Version));
        Assert.True(UpdatePolicy.ReachedTarget(new(1, 2, 3, 4), payload.Version));
    }

    [Fact]
    public async Task ParallelPartsProduceExactFile()
    {
        var bytes = RandomNumberGenerator.GetBytes(9 * 1024 * 1024);
        var calls = 0;
        using var http = new HttpClient(new Handler(r => { Interlocked.Increment(ref calls); return Response(r, bytes); }));
        var file = await new ResumableDownloader(http).DownloadAsync(Remote(bytes.Length), _root, Progress, default);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(file));
        Assert.Equal(4, calls);
    }

    [Fact]
    public async Task CancellationRetainsBytesAndNextCallResumes()
    {
        var bytes = RandomNumberGenerator.GetBytes(1024 * 1024);
        using var cancel = new CancellationTokenSource();
        var starts = new List<long>();
        using var http = new HttpClient(new Handler(r => { starts.Add(r.Headers.Range!.Ranges.Single().From!.Value); return Response(r, bytes); }));
        var downloader = new ResumableDownloader(http, TimeSpan.Zero);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => downloader.DownloadAsync(Remote(bytes.Length), _root, new InlineProgress(p => { if (p.Bytes > 0) cancel.Cancel(); }), cancel.Token));
        var file = await downloader.DownloadAsync(Remote(bytes.Length), _root, Progress, default);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(file));
        Assert.Contains(starts, s => s > 0);
    }

    [Fact]
    public async Task IgnoredRangeFallsBackToFullDownload()
    {
        var bytes = new byte[1000];
        var calls = 0;
        using var http = new HttpClient(new Handler(r => { calls++; return new(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes), RequestMessage = r }; }));
        var file = await new ResumableDownloader(http).DownloadAsync(Remote(bytes.Length), _root, Progress, default);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(file));
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task ChangedValidatorIsRejected()
    {
        using var http = new HttpClient(new Handler(r => { var response = Response(r, new byte[100]); response.Headers.ETag = new("\"two\""); return response; }));
        await Assert.ThrowsAsync<InvalidDataException>(() => new ResumableDownloader(http).DownloadAsync(Remote(100), _root, Progress, default));
        Assert.False(File.Exists(Path.Combine(_root, "candidate.exe")));
    }

    [Fact]
    public async Task FailedOrOlderCandidatePreservesCurrentPayload()
    {
        var verifier = new FixtureVerifier();
        var store = new PayloadStore(_root, verifier);
        var candidate = Path.Combine(_root, "fixture");
        await File.WriteAllTextAsync(candidate, "1.2.3.4");
        var original = await store.CommitAsync(candidate, null, default);
        await File.WriteAllTextAsync(candidate, "1.2.3.3");
        await Assert.ThrowsAsync<InvalidDataException>(() => store.CommitAsync(candidate, null, default));
        await File.WriteAllTextAsync(candidate, "corrupt");
        await Assert.ThrowsAsync<InvalidDataException>(() => store.CommitAsync(candidate, null, default));
        Assert.Equal(original, await store.ReadAsync(default));
        await File.WriteAllTextAsync(original.Path, "1.2.3.5");
        await Assert.ThrowsAsync<InvalidDataException>(() => store.ReadAsync(default));
    }

    [Fact]
    public async Task UnsignedFileCannotPassWindowsTrust()
    {
        var file = Path.Combine(_root, "mpam-fe.exe");
        await File.WriteAllTextAsync(file, "not a Microsoft signed program");
        await Assert.ThrowsAsync<CryptographicException>(() => new MicrosoftSignatureVerifier().VerifyAsync(file, default));
    }

    [Fact]
    public async Task SelfUpdateRejectsDifferentRepositoryAndBadHash()
    {
        using var http = new HttpClient(new Handler(r => new(HttpStatusCode.OK) { RequestMessage = r, Content = new ByteArrayContent([1, 2, 3]) }));
        var updates = new AppUpdates(http, new("https://raw.githubusercontent.com/talmidhon/new-app/main/version.json"));
        var release = new AppRelease("1.0.0", "https://github.com/talmidhon/new-app/releases/download/v1.0.0/MivtzarNaki-win-x64.zip", new string('A', 64), "test");
        Assert.Throws<InvalidDataException>(() => updates.Validate(release with { DownloadUrl = release.DownloadUrl.Replace("new-app", "other-app") }));
        await Assert.ThrowsAsync<InvalidDataException>(() => updates.StageAsync(release, _root, Progress, default));
        Assert.False(File.Exists(Path.Combine(_root, "MivtzarNaki.next.zip")));
    }

    private static HttpResponseMessage Response(HttpRequestMessage request, byte[] bytes)
    {
        var range = request.Headers.Range!.Ranges.Single();
        var from = (int)range.From!.Value;
        var to = (int)range.To!.Value;
        var response = new HttpResponseMessage(HttpStatusCode.PartialContent) { RequestMessage = request, Content = new ByteArrayContent(bytes[from..(to + 1)]) };
        response.Content.Headers.ContentRange = new(from, to, bytes.Length);
        response.Headers.ETag = new("\"one\"");
        return response;
    }
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(response(request));
    }
    private sealed class InlineProgress(Action<TransferProgress> action) : IProgress<TransferProgress> { public void Report(TransferProgress value) => action(value); }
    private sealed class FixtureVerifier : IPayloadVerifier
    {
        public async Task<Payload> VerifyAsync(string path, CancellationToken cancellationToken)
        {
            if (!Version.TryParse(await File.ReadAllTextAsync(path, cancellationToken), out var version)) throw new InvalidDataException();
            return new(path, version, new FileInfo(path).Length, await PayloadStore.HashAsync(path, cancellationToken));
        }
    }
}

