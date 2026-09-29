using System.Net;
using System.Text.Json;
using MivtzarNaki.Core;
using MivtzarNaki.Windows;

// Test-only entry point, never delivered. Default is a controlled startup failure.
if (args.FirstOrDefault() == "--apply-update")
{
    var result = await PortableAppUpdate.ApplyAsync(args);
    await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(args[3])!, "helper-result.txt"), result.ToString());
    return result;
}
if (args is not ["--exercise-update", var archive, var expectedHash]) return 23;
using var session = new UpdateSession(new HttpClient(new FixtureHttp(archive, expectedHash)));
session.Settings = session.Settings with { UpdateFeed = "https://raw.githubusercontent.com/talmidhon/fixture-only/main/version.json" };
await session.CheckNetworkAsync(default);
await session.UpdateAppAsync(new Progress<TransferProgress>(), default);
// Regression: the helper must not validate App while its parent still owns a file.
using (var parentLock = new FileStream(Path.Combine(session.Environment.Root, "App", FolderPackage.ManifestName), FileMode.Open, FileAccess.Read, FileShare.None))
    await Task.Delay(2000);
await File.WriteAllTextAsync(Path.Combine(session.Environment.Data, "session-result.txt"), "UpdateAppAsync returned; parent exiting");
return 0;

sealed class FixtureHttp(string archive, string hash) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // No external requests, no Defender reads or operations.
        HttpContent? content = request.RequestUri?.Host switch
        {
            "raw.githubusercontent.com" => new StringContent(JsonSerializer.Serialize(new AppRelease("0.2.0", "https://github.com/talmidhon/fixture-only/releases/download/v0.2.0/MivtzarNaki-win-x64.zip", hash, "fixture"), JsonModels.Default.AppRelease)),
            "github.com" => new StreamContent(File.OpenRead(archive)),
            _ => null
        };
        return Task.FromResult(new HttpResponseMessage(content is null ? HttpStatusCode.NotFound : HttpStatusCode.OK) { RequestMessage = request, Content = content });
    }
}
