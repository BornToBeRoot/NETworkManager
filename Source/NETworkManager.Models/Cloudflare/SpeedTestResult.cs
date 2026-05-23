namespace NETworkManager.Models.Cloudflare;

/// <summary>
/// Final result of a Cloudflare speed test run.
/// </summary>
public class SpeedTestResult
{
    /// <summary>
    /// Download throughput in megabits per second (Mbps). <c>null</c> when no
    /// samples were collected (e.g. test cancelled before any download).
    /// </summary>
    public double? DownloadMbps { get; set; }

    /// <summary>
    /// Upload throughput in megabits per second (Mbps). <c>null</c> when no
    /// samples were collected.
    /// </summary>
    public double? UploadMbps { get; set; }

    /// <summary>
    /// Unloaded latency in milliseconds (50th percentile of latency probes).
    /// <c>null</c> when no probes were collected.
    /// </summary>
    public double? LatencyMs { get; set; }

    /// <summary>
    /// Average consecutive delta between latency samples, in milliseconds.
    /// <c>null</c> when fewer than two probes were collected.
    /// </summary>
    public double? JitterMs { get; set; }

    /// <summary>
    /// ISP name (Cloudflare meta <c>asOrganization</c>).
    /// </summary>
    public string Isp { get; set; }

    /// <summary>
    /// City of the client (Cloudflare meta <c>city</c>).
    /// </summary>
    public string ClientCity { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code of the client (Cloudflare meta <c>country</c>).
    /// </summary>
    public string ClientCountry { get; set; }

    /// <summary>
    /// City of the Cloudflare PoP serving the test, e.g. "Frankfurt-am-Main".
    /// </summary>
    public string ServerCity { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code of the Cloudflare PoP, e.g. "DE".
    /// </summary>
    public string ServerCountry { get; set; }

    /// <summary>
    /// IATA code of the Cloudflare PoP, e.g. "FRA".
    /// </summary>
    public string ServerIata { get; set; }

    /// <summary>
    /// Indicates that the speed test run failed.
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Error message when <see cref="HasError"/> is <c>true</c>.
    /// </summary>
    public string ErrorMessage { get; set; }
}
