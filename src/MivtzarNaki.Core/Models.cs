using System.Text.Json.Serialization;

namespace MivtzarNaki.Core;

public sealed record RemotePayload(Uri Uri, Version? Version, long? Size, string? ETag, DateTimeOffset? Modified, bool Ranges)
{
    public bool Resumable => Ranges && Size > 0 && (ETag is not null || Modified is not null);
    public string Identity => $"{Uri}|{Size}|{ETag}|{Modified:O}";
}
public sealed record Payload(string Path, Version Version, long Size, string Sha256);
public sealed record DefenderStatus(Version? Version, bool? Running, string Detail);
public sealed record TransferProgress(long Bytes, long? Total, string Message)
{
    public double Percent => Total > 0 ? Math.Clamp(Bytes * 100d / Total.Value, 0, 100) : 0;
}
public sealed record PayloadRecord(string Version, long Size, string Sha256);
public sealed record DownloadState(string Identity, int Parts);
public sealed record AppRelease(
    [property: JsonPropertyName("latest_version")] string Version,
    [property: JsonPropertyName("download_url")] string DownloadUrl,
    [property: JsonPropertyName("sha256")] string Sha256,
    [property: JsonPropertyName("message")] string Message);
public sealed record AppSettings(string Theme = "Default", string UpdateFeed = "");

public interface IPayloadVerifier
{
    Task<Payload> VerifyAsync(string path, CancellationToken cancellationToken);
}

public static class UpdatePolicy
{
    public static bool NeedsDownload(Payload? local, RemotePayload remote) => remote.Version is not null && (local is null || remote.Version > local.Version);
    public static bool CanInstall(Payload? local, DefenderStatus status) => local is not null && status.Version is not null && local.Version > status.Version && status.Running == true;
    public static bool ReachedTarget(Version? installed, Version target) => installed is not null && installed >= target;
}

[JsonSerializable(typeof(PayloadRecord))]
[JsonSerializable(typeof(DownloadState))]
[JsonSerializable(typeof(AppRelease))]
[JsonSerializable(typeof(AppSettings))]
public partial class JsonModels : JsonSerializerContext;
