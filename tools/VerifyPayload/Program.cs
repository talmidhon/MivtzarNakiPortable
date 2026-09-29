using MivtzarNaki.Core;
using MivtzarNaki.Windows;

// Read-only integration probe: never executes a payload or changes Defender.
if (args.Length != 1) throw new ArgumentException("Supply a temporary download directory.");
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
var remote = await new MicrosoftCatalog(http).CheckAsync(default);
Console.WriteLine($"Server={remote.Version}; Size={remote.Size}; Ranges={remote.Resumable}");
var path = await new ResumableDownloader(http).DownloadAsync(remote, Path.GetFullPath(args[0]), new Progress<TransferProgress>(), default);
var payload = await new MicrosoftSignatureVerifier().VerifyAsync(path, default);
Console.WriteLine($"Verified Microsoft payload: {payload.Version}; SHA256={payload.Sha256}");
if (remote.Version is not null && payload.Version < remote.Version) throw new InvalidDataException("Payload older than catalog");
