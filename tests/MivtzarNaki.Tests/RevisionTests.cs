using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using MivtzarNaki.Core;
using MivtzarNaki.Windows;
using Xunit;

namespace MivtzarNaki.Tests;

public sealed class RevisionTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "מבצר fixture " + Guid.NewGuid().ToString("N"));
    public RevisionTests() => Directory.CreateDirectory(_root);
    public void Dispose() => Directory.Delete(_root, true);
    private static Payload Usb => new("fixture", new(1, 2, 3, 4), 1, "");
    private static RemotePayload Server => new(MicrosoftCatalog.DownloadUri, new(1, 2, 3, 5), 1, null, null, false);
    [Theory]
    [InlineData("1.2.3.3", true, StatusTone.Attention)]
    [InlineData("1.2.3.4", false, StatusTone.Success)]
    [InlineData("1.2.3.5", false, StatusTone.Success)]
    [InlineData(null, false, StatusTone.Unknown)]
    public void ComputerComparisonAndActions(string? version, bool action, StatusTone tone)
    {
        var view = StatusPresentation.Create(new(version is null ? null : Version.Parse(version), true, ""), Usb, Server);
        Assert.Equal(action, view.Computer.Action); Assert.Equal(tone, view.Computer.Tone);
        Assert.Equal(action, UpdatePolicy.CanInstall(Usb, new(version is null ? null : Version.Parse(version), true, "")));
    }
    [Fact]
    public void UnknownCorruptAndOfflineNeverClaimCurrent()
    {
        var pc = new DefenderStatus(new(1, 0), true, "");
        Assert.Equal("אין עדכון ב־USB", StatusPresentation.Create(pc, null, Server).Computer.Text);
        Assert.Equal(StatusTone.Error, StatusPresentation.Create(pc, null, Server, "bad").Computer.Tone);
        Assert.Equal(StatusTone.Unknown, StatusPresentation.Create(pc, Usb, Server with { Version = null }).Usb.Tone);
        Assert.False(UpdatePolicy.NeedsDownload(Usb, Server with { Version = null }));
        var offline = StatusPresentation.Create(pc, Usb, null);
        Assert.True(offline.Computer.Action); Assert.False(offline.Usb.Action);
        Assert.Contains("לא ניתן", offline.Usb.Text);
        Assert.Equal(StatusTone.Success, StatusPresentation.Create(pc, Usb, Server with { Version = Usb.Version }).Usb.Tone);
        Assert.Equal(StatusTone.Unknown, StatusPresentation.Create(pc with { Running = null }, Usb, Server).Computer.Tone);
        Assert.False(UpdatePolicy.CanInstall(Usb, pc with { Running = null }));
    }
    [Fact]
    public async Task MetadataTransientThenSuccessAndExactlyThreeAttempts()
    {
        var calls = 0;
        var result = await MetadataRetry.RunAsync<int>(_ => ++calls == 1 ? throw new HttpRequestException() : Task.FromResult(7), default, delay: TimeSpan.Zero);
        Assert.Equal(7, result); Assert.Equal(2, calls);
        calls = 0;
        await Assert.ThrowsAsync<HttpRequestException>(() => MetadataRetry.RunAsync<int>(_ => { calls++; throw new HttpRequestException(); }, default, delay: TimeSpan.Zero));
        Assert.Equal(3, calls);
    }
    [Fact]
    public async Task MetadataDoesNotRetryPermanentTrustOrUserCancellation()
    {
        foreach (var error in new Exception[] { new InvalidDataException(), new HttpRequestException("404", null, HttpStatusCode.NotFound), new HttpRequestException(HttpRequestError.SecureConnectionError) })
        {
            var calls = 0;
            await Assert.ThrowsAnyAsync<Exception>(() => MetadataRetry.RunAsync<int>(_ => { calls++; throw error; }, default, delay: TimeSpan.Zero));
            Assert.Equal(1, calls);
        }
        using var cts = new CancellationTokenSource();
        var count = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => MetadataRetry.RunAsync<int>(token => { count++; cts.Cancel(); token.ThrowIfCancellationRequested(); return Task.FromResult(0); }, cts.Token));
        Assert.Equal(1, count);
    }
    [Fact]
    public async Task MetadataBudgetBoundsHangingRequestsAndRetryAfter()
    {
        var watch = Stopwatch.StartNew(); var calls = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => MetadataRetry.RunAsync<int>(async token => { calls++; await Task.Delay(10000, token); return 0; }, default, TimeSpan.FromMilliseconds(160), TimeSpan.FromMilliseconds(70), TimeSpan.Zero));
        Assert.InRange(calls, 2, 3); Assert.True(watch.Elapsed < TimeSpan.FromSeconds(2));
        calls = 0;
        await Assert.ThrowsAsync<TemporaryServerException>(() => MetadataRetry.RunAsync<int>(_ => { calls++; throw new TemporaryServerException(TimeSpan.FromMinutes(2)); }, default, TimeSpan.FromMilliseconds(100)));
        Assert.Equal(1, calls);
        watch.Restart(); calls = 0;
        await MetadataRetry.RunAsync<int>(_ => ++calls == 1 ? throw new TemporaryServerException(TimeSpan.FromMilliseconds(40)) : Task.FromResult(1), default);
        Assert.True(watch.Elapsed >= TimeSpan.FromMilliseconds(35));
    }
    [Fact]
    public async Task CatalogRetries503IncludingVersionPage()
    {
        var heads = 0; var pages = 0;
        using var http = new HttpClient(new Handler((r, _) =>
        {
            if (r.Method == HttpMethod.Head) { heads++; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = r }); }
            pages++;
            return Task.FromResult(new HttpResponseMessage(pages == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK) { RequestMessage = r, Content = new StringContent("Latest security intelligence update Version: 1.2.3.4") });
        }));
        var result = await new MicrosoftCatalog(http).CheckAsync(default);
        Assert.Equal(Usb.Version, result.Version); Assert.Equal(2, heads);
    }
    [Fact]
    public async Task OverlappingRefreshDoesNotBlockLocalInstallationAndFalseSuccessRejected()
    {
        var verifier = new Verifier(); var defender = new FakeDefender();
        var candidate = Path.Combine(_root, "fixture"); await File.WriteAllTextAsync(candidate, "1.2.3.4");
        await new PayloadStore(Path.Combine(_root, "OfflinePayloads"), verifier).CommitAsync(candidate, null, default);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        using var http = new HttpClient(new Handler(async (request, token) => { if (request.RequestUri!.Host != "raw.githubusercontent.com") calls++; started.TrySetResult(); await Task.Delay(10000, token); throw new HttpRequestException(); }));
        using var session = new UpdateSession(http, _root, verifier, defender);
        await session.ReadLocalAsync(default);
        using var cancel = new CancellationTokenSource();
        var network = session.CheckNetworkAsync(cancel.Token); await started.Task;
        await session.CheckNetworkAsync(default); Assert.Equal(1, calls);
        Assert.True(session.CanInstall);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.InstallAsync(false, default));
        defender.ReachTarget = true;
        await session.InstallAsync(false, default); Assert.Equal(Usb.Version, session.Computer.Version);
        Assert.False(session.CanInstall);
        cancel.Cancel(); await Assert.ThrowsAnyAsync<OperationCanceledException>(() => network);
    }
    [Fact]
    public async Task RepairOnlyForBlockingServiceAndNoAutomaticInstall()
    {
        var defender = new FakeDefender(); var verifier = new Verifier();
        var candidate = Path.Combine(_root, "fixture"); await File.WriteAllTextAsync(candidate, "1.2.3.4");
        await new PayloadStore(Path.Combine(_root, "OfflinePayloads"), verifier).CommitAsync(candidate, null, default);
        using var session = new UpdateSession(root: _root, verifier: verifier, defender: defender);
        await session.ReadLocalAsync(default);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.StartDefenderAsync());
        defender.Status = defender.Status with { Running = false };
        await session.ReadLocalAsync(default); await session.StartDefenderAsync();
        Assert.Equal(1, defender.Repairs); Assert.Equal(0, defender.Installs);
    }
    [Fact]
    public async Task CancelledCommitAndFailedDownloadKeepPreviousPayload()
    {
        var store = new PayloadStore(Path.Combine(_root, "OfflinePayloads"), new Verifier());
        var candidate = Path.Combine(_root, "fixture"); await File.WriteAllTextAsync(candidate, "1.2.3.4");
        var original = await store.CommitAsync(candidate, null, default);
        await File.WriteAllTextAsync(candidate, "1.2.3.5");
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.CommitAsync(candidate, null, cancel.Token));
        using var http = new HttpClient(new Handler((_, _) => throw new HttpRequestException()));
        await Assert.ThrowsAsync<HttpRequestException>(() => new ResumableDownloader(http, TimeSpan.Zero).DownloadAsync(Server, Path.Combine(_root, "OfflinePayloads"), new Progress<TransferProgress>(), default));
        Assert.Equal(original, await store.ReadAsync(default));
    }
    [Fact]
    public async Task FolderSwapPreservesUserDataAndRemovesObsoleteDependencies()
    {
        var app = Path.Combine(_root, "App"); var staged = Path.Combine(_root, ".updates", "stage");
        await MakeApp(app, "old"); await File.WriteAllTextAsync(Path.Combine(app, "obsolete.dll"), "old");
        await MakeApp(staged, "new");
        Directory.CreateDirectory(Path.Combine(_root, "Data")); await File.WriteAllTextAsync(Path.Combine(_root, "Data", "settings.json"), "user");
        Directory.CreateDirectory(Path.Combine(_root, "OfflinePayloads")); await File.WriteAllTextAsync(Path.Combine(_root, "OfflinePayloads", "fixture"), "payload");
        await FolderPackage.ReplaceAsync(_root, staged, _ => Task.CompletedTask, default);
        Assert.Equal("new", await File.ReadAllTextAsync(Path.Combine(app, "MivtzarNaki.exe")));
        Assert.False(File.Exists(Path.Combine(app, "obsolete.dll")));
        Assert.Equal("user", await File.ReadAllTextAsync(Path.Combine(_root, "Data", "settings.json")));
        Assert.Equal("payload", await File.ReadAllTextAsync(Path.Combine(_root, "OfflinePayloads", "fixture")));
        Assert.Single(Directory.GetDirectories(Path.Combine(_root, ".updates"), "previous-*"));
    }
    [Fact]
    public async Task FolderSwapRollsBackLaunchFailureAndRejectsLockedFiles()
    {
        var app = Path.Combine(_root, "App"); var staged = Path.Combine(_root, ".updates", "stage");
        await MakeApp(app, "old"); await MakeApp(staged, "new");
        await Assert.ThrowsAsync<IOException>(() => FolderPackage.ReplaceAsync(_root, staged, _ => throw new IOException("launch"), default));
        Assert.Equal("old", await File.ReadAllTextAsync(Path.Combine(app, "MivtzarNaki.exe")));
        using (var locked = new FileStream(Path.Combine(app, "MivtzarNaki.dll"), FileMode.Open, FileAccess.Read, FileShare.Read))
            await Assert.ThrowsAsync<IOException>(() => FolderPackage.ReplaceAsync(_root, staged, _ => Task.CompletedTask, default));
        Assert.Equal("old", await File.ReadAllTextAsync(Path.Combine(app, "MivtzarNaki.exe")));
    }
    [Theory]
    [InlineData("App/../escape.exe")]
    [InlineData("App/C:/escape.exe")]
    [InlineData("Data/settings.json")]
    [InlineData("App/dir/../../escape.exe")]
    [InlineData("App/test.dll:evil")]
    public async Task ZipRejectsUnsafePaths(string entry)
    {
        var archive = Path.Combine(_root, "bad.zip");
        using (var zip = ZipFile.Open(archive, ZipArchiveMode.Create)) { using var writer = new StreamWriter(zip.CreateEntry(entry).Open()); writer.Write("evil"); }
        await Assert.ThrowsAsync<InvalidDataException>(() => FolderPackage.ExtractAsync(archive, Path.Combine(_root, "stage"), PayloadStore.HashAsync(archive, default).Result, default));
        Assert.False(Directory.Exists(Path.Combine(_root, "stage")));
    }
    [Fact]
    public async Task ZipRoundTripAndPartialPackageValidation()
    {
        var container = Path.Combine(_root, "package"); var app = Path.Combine(container, "App"); await MakeApp(app, "new");
        var archive = Path.Combine(_root, "good.zip"); ZipFile.CreateFromDirectory(container, archive);
        var stage = Path.Combine(_root, "stage");
        await FolderPackage.ExtractAsync(archive, stage, await PayloadStore.HashAsync(archive, default), default);
        await FolderPackage.ValidateAsync(stage, default);
        File.Delete(Path.Combine(stage, "MivtzarNaki.dll"));
        await Assert.ThrowsAnyAsync<IOException>(() => FolderPackage.ValidateAsync(stage, default));
    }
    [Fact]
    public async Task AppFeedRetriesTransientFailureButRejectsInvalidMetadataImmediately()
    {
        var calls = 0;
        using var http = new HttpClient(new Handler((r, _) => Task.FromResult(new HttpResponseMessage(++calls == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)
        {
            RequestMessage = r,
            Content = new StringContent(JsonSerializer.Serialize(new AppRelease("0.2.0", "https://github.com/talmidhon/test/releases/download/v0.2.0/MivtzarNaki-win-x64.zip", new string('A', 64), "fixture")))
        })));
        var updates = new AppUpdates(http, new Uri("https://raw.githubusercontent.com/talmidhon/test/main/version.json"));
        Assert.NotNull(await updates.CheckAsync(new(0, 1, 0), default));
        Assert.Equal(2, calls);
        calls = 0;
        using var invalid = new HttpClient(new Handler((r, _) => { calls++; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = r, Content = new StringContent("{}") }); }));
        await Assert.ThrowsAsync<InvalidDataException>(() => new AppUpdates(invalid, new Uri("https://raw.githubusercontent.com/talmidhon/test/main/version.json")).CheckAsync(new(0, 1, 0), default));
        Assert.Equal(1, calls);
    }
    [Fact]
    public void DiagnosticLogIsBounded()
    {
        var env = new PortableEnvironment(_root); var log = new ActivityLog(env);
        for (var n = 0; n < 1200; n++) log.Write(new string('א', 10000), new IOException(new string('x', 3000)));
        Assert.True(new FileInfo(Path.Combine(env.Data, "diagnostic.log")).Length < 520 * 1024);
        Assert.True(new FileInfo(Path.Combine(env.Data, "diagnostic.log.previous")).Length < 520 * 1024);
    }
    private static async Task MakeApp(string path, string content)
    {
        Directory.CreateDirectory(path);
        var files = new Dictionary<string, string>();
        foreach (var name in new[] { "MivtzarNaki.exe", "MivtzarNaki.dll", "MivtzarNaki.runtimeconfig.json", "runtime.dll" })
        { var file = Path.Combine(path, name); await File.WriteAllTextAsync(file, content); files[name] = await PayloadStore.HashAsync(file, default); }
        await File.WriteAllTextAsync(Path.Combine(path, FolderPackage.ManifestName), JsonSerializer.Serialize(new FolderManifest("0.2.0", files)));
    }
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => response(request, cancellationToken); }
    private sealed class Verifier : IPayloadVerifier
    { public async Task<Payload> VerifyAsync(string path, CancellationToken token) => new(path, Version.Parse(await File.ReadAllTextAsync(path, token)), new FileInfo(path).Length, await PayloadStore.HashAsync(path, token)); }
    private sealed class FakeDefender : IDefenderOperations
    {
        public DefenderStatus Status = new(new(1, 2, 3, 3), true, "fixture");
        public bool ReachTarget; public int Repairs, Installs;
        public DefenderStatus ReadStatus() => Status;
        public Task InstallAsync(Payload payload, CancellationToken token) { Installs++; if (ReachTarget) Status = Status with { Version = payload.Version }; return Task.CompletedTask; }
        public Task InstallElevatedAsync(Payload payload) => InstallAsync(payload, default);
        public Task StartServiceAsync() { Repairs++; Status = Status with { Running = true }; return Task.CompletedTask; }
    }
}
