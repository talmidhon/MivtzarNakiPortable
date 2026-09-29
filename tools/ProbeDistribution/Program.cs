using System.Text.Json;
using MivtzarNaki.Core;

using var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
http.DefaultRequestHeaders.UserAgent.ParseAdd("MivtzarNaki-release-verification");
var updates = new AppUpdates(http, new(Distribution.UpdateFeed));
var release = await updates.CheckAsync(new(0, 1, 0), default) ?? throw new InvalidDataException("No update for real baseline");
if (await updates.CheckAsync(Version.Parse(release.Version), default) is not null) throw new InvalidDataException("Same version offered");
if (await updates.CheckAsync(new(99, 0, 0), default) is not null) throw new InvalidDataException("Downgrade offered");
Console.WriteLine(JsonSerializer.Serialize(new { Feed = Distribution.UpdateFeed, release.Version, release.DownloadUrl, release.Sha256, SameVersionBlocked = true, DowngradeBlocked = true }));
