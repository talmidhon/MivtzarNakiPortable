using MivtzarNaki.Core;
using MivtzarNaki.Windows;

// Test-only driver in a copy of the real baseline; never invokes Defender operations.
if (args.FirstOrDefault() == "--apply-update")
{
    var code = await PortableAppUpdate.ApplyAsync(args);
    await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(args[3])!, "helper-result.txt"), code.ToString());
    return code;
}
if (args is not ["--live-update", var feed]) return 23;
using var session = new UpdateSession();
session.Settings = session.Settings with { UpdateFeed = feed };
await session.CheckNetworkAsync(default);
if (session.NewApp is null) throw new InvalidDataException("Real source did not offer a newer version to baseline " + UpdateSession.AppVersion);
await File.WriteAllTextAsync(Path.Combine(session.Environment.Data, "live-metadata.txt"), $"Current={UpdateSession.AppVersion}\nVersion={session.NewApp.Version}\nAsset={session.NewApp.DownloadUrl}\nSHA256={session.NewApp.Sha256}");
// This invocation represents explicit consent in an isolated test copy, never automatic UI behavior.
await session.UpdateAppAsync(new Progress<TransferProgress>(), default);
await File.WriteAllTextAsync(Path.Combine(session.Environment.Data, "live-session.txt"), "Real download, checksum, manifest, staging and StartReplacement completed; parent exiting.");
return 0;
