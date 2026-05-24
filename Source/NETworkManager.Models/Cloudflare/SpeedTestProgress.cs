namespace NETworkManager.Models.Cloudflare;

/// <summary>
/// Phase of a Cloudflare speed test run, reported by
/// <see cref="SpeedTestService"/> via <see cref="System.IProgress{T}"/>.
/// </summary>
public enum SpeedTestPhase
{
    FetchingMetadata,
    MeasuringLatency,
    MeasuringDownload,
    MeasuringUpload
}

/// <summary>
/// Progress event passed by <see cref="SpeedTestService"/> to update the UI
/// with the currently running measurement phase and the latest live estimate
/// of each metric. Values are <c>null</c> until the first sample for that
/// metric has been collected.
/// </summary>
/// <param name="Phase">Current measurement phase.</param>
/// <param name="DownloadMbps">Live download throughput estimate (Mbps).</param>
/// <param name="UploadMbps">Live upload throughput estimate (Mbps).</param>
/// <param name="LatencyMs">Live latency estimate (50th percentile of probes).</param>
/// <param name="JitterMs">Live jitter estimate (average consecutive delta).</param>
/// <param name="NewDownloadSampleMbps">
/// Set when this emission marks a freshly completed download sample (one HTTP
/// request finished). Mid-stream live updates leave this <c>null</c>.
/// </param>
/// <param name="NewUploadSampleMbps">
/// Set when this emission marks a freshly completed upload sample.
/// </param>
/// <param name="Meta">
/// Cloudflare <c>/meta</c> response, emitted once after metadata is fetched
/// so ISP / location / server details can be displayed before the bandwidth
/// measurements complete.
/// </param>
public record SpeedTestProgress(
    SpeedTestPhase Phase,
    double? DownloadMbps = null,
    double? UploadMbps = null,
    double? LatencyMs = null,
    double? JitterMs = null,
    double? NewDownloadSampleMbps = null,
    double? NewUploadSampleMbps = null,
    SpeedTestMetaInfo Meta = null);
