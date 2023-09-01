namespace NETworkManager.Models.IPApi;

/// <summary>
/// Class contains the IP geolocation information.
/// </summary>
public class IPGeolocationInfo
{
    /// <summary>
    /// Status of the IP geolocation information retrieval.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Continent where the IP address is located.
    /// </summary>
    public string Continent { get; set; }

    /// <summary>
    /// Continent code where the IP address is located.
    /// </summary>
    public string ContinentCode { get; set; }

    /// <summary>
    /// Country where the IP address is located.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// Country code where the IP address is located.
    /// </summary>
    public string CountryCode { get; set; }

    /// <summary>
    /// Region where the IP address is located.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Region name where the IP address is located.
    /// </summary>
    public string RegionName { get; set; }

    /// <summary>
    /// City where the IP address is located.
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// District where the IP address is located.
    /// </summary>
    public string District { get; set; }

    /// <summary>
    /// Zip code of the location where the IP address is located.
    /// </summary>
    public string Zip { get; set; }

    /// <summary>
    /// Latitude of the location where the IP address is located.
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    /// Longitude of the location where the IP address is located.
    /// </summary>
    public double Lon { get; set; }

    /// <summary>
    /// Timezone of the location where the IP address is located.
    /// </summary>
    public string Timezone { get; set; }

    /// <summary>
    /// Offset from UTC in seconds for the location where the IP address is located.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Currency used in the country where the IP address is located.
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Internet Service Provider (ISP) of the IP address.
    /// </summary>
    public string Isp { get; set; }

    /// <summary>
    /// Organization associated with the IP address.
    /// </summary>
    public string Org { get; set; }

    /// <summary>
    /// Autonomous System (AS) number and name associated with the IP address.
    /// </summary>
    public string As { get; set; }

    /// <summary>
    /// Name of the Autonomous System (AS) associated with the IP address.
    /// </summary>
    public string Asname { get; set; }

    /// <summary>
    /// Reverse DNS hostname associated with the IP address.
    /// </summary>
    public string Reverse { get; set; }

    /// <summary>
    /// Indicates whether the IP address is associated with a mobile network.
    /// </summary>
    public bool Mobile { get; set; }

    /// <summary>
    /// Indicates whether the IP address is a proxy server.
    /// </summary>
    public bool Proxy { get; set; }

    /// <summary>
    /// Indicates whether the IP address is associated with hosting services.
    /// </summary>
    public bool Hosting { get; set; }

    /// <summary>
    /// IP address used for the query.
    /// </summary>
    public string Query { get; set; }
}
