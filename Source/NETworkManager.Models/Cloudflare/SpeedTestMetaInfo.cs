using Newtonsoft.Json;

namespace NETworkManager.Models.Cloudflare;

/// <summary>
/// Cloudflare PoP (Point of Presence) information returned by the
/// <c>speed.cloudflare.com/meta</c> endpoint.
/// </summary>
public class SpeedTestMetaColo
{
    /// <summary>
    /// IATA airport code of the PoP (e.g. "FRA").
    /// </summary>
    [JsonProperty("iata")]
    public string Iata { get; set; }

    /// <summary>
    /// City of the PoP (e.g. "Frankfurt-am-Main").
    /// </summary>
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code of the PoP (e.g. "DE").
    /// </summary>
    [JsonProperty("cca2")]
    public string Cca2 { get; set; }
}

/// <summary>
/// Deserialized response of the <c>speed.cloudflare.com/meta</c> endpoint.
/// Provides client and Cloudflare PoP metadata used to enrich the speed
/// test result. Requires the <c>Origin: https://speed.cloudflare.com</c>
/// header on the request, otherwise an empty object is returned.
/// </summary>
public class SpeedTestMetaInfo
{
    /// <summary>
    /// Public IP address of the requesting client as seen by Cloudflare.
    /// </summary>
    [JsonProperty("clientIp")]
    public string ClientIp { get; set; }

    /// <summary>
    /// Autonomous System Number of the client's ISP.
    /// </summary>
    [JsonProperty("asn")]
    public int Asn { get; set; }

    /// <summary>
    /// Human-readable ISP name (e.g. "innogy TelNet").
    /// </summary>
    [JsonProperty("asOrganization")]
    public string AsOrganization { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code of the client (e.g. "DE").
    /// </summary>
    [JsonProperty("country")]
    public string Country { get; set; }

    /// <summary>
    /// City of the client (e.g. "Bochum").
    /// </summary>
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// Cloudflare PoP (Point of Presence) details.
    /// </summary>
    [JsonProperty("colo")]
    public SpeedTestMetaColo Colo { get; set; }
}