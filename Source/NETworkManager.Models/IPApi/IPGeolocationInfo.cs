using System;
using Newtonsoft.Json;

namespace NETworkManager.Models.IPApi;

/// <summary>
///     Class contains the IP geolocation information.
/// </summary>
public class IPGeolocationInfo
{
    /// <summary>
    ///     Status of the IP geolocation information retrieval.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    ///     Reason if the query failed (<see cref="Status" /> <c>fail</c>), e.g. <c>private range</c>,
    ///     <c>reserved range</c> or <c>invalid query</c>.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     ip-api.com has no geolocation for the query (HTTP 200 with <see cref="Status" /> <c>fail</c>, e.g. a private
    ///     IP address), see <see cref="Message" />.
    /// </summary>
    [JsonIgnore]
    public bool HasFailed => string.Equals(Status, "fail", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Continent where the IP address is located.
    /// </summary>
    public string Continent { get; set; }

    /// <summary>
    ///     Continent code where the IP address is located.
    /// </summary>
    public string ContinentCode { get; set; }

    /// <summary>
    ///     Country where the IP address is located.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    ///     Country code where the IP address is located.
    /// </summary>
    public string CountryCode { get; set; }

    /// <summary>
    ///     Region where the IP address is located.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    ///     Region name where the IP address is located.
    /// </summary>
    public string RegionName { get; set; }

    /// <summary>
    ///     City where the IP address is located.
    /// </summary>
    public string City { get; set; }

    /// <summary>
    ///     District where the IP address is located.
    /// </summary>
    public string District { get; set; }

    /// <summary>
    ///     Zip code of the location where the IP address is located.
    /// </summary>
    public string Zip { get; set; }

    /// <summary>
    ///     Latitude of the location where the IP address is located.
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    ///     Longitude of the location where the IP address is located.
    /// </summary>
    public double Lon { get; set; }

    /// <summary>
    ///     Timezone of the location where the IP address is located.
    /// </summary>
    public string Timezone { get; set; }

    /// <summary>
    ///     Offset from UTC in seconds for the location where the IP address is located.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    ///     Currency used in the country where the IP address is located.
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    ///     Internet Service Provider (ISP) of the IP address.
    /// </summary>
    public string Isp { get; set; }

    /// <summary>
    ///     Organization associated with the IP address.
    /// </summary>
    public string Org { get; set; }

    /// <summary>
    ///     Autonomous System (AS) number and name associated with the IP address.
    /// </summary>
    public string As { get; set; }

    /// <summary>
    ///     Name of the Autonomous System (AS) associated with the IP address.
    /// </summary>
    public string Asname { get; set; }

    /// <summary>
    ///     Reverse DNS hostname associated with the IP address.
    /// </summary>
    public string Reverse { get; set; }

    /// <summary>
    ///     Indicates whether the IP address is associated with a mobile network.
    /// </summary>
    public bool Mobile { get; set; }

    /// <summary>
    ///     Indicates whether the IP address is a proxy server.
    /// </summary>
    public bool Proxy { get; set; }

    /// <summary>
    ///     Indicates whether the IP address is associated with hosting services.
    /// </summary>
    public bool Hosting { get; set; }

    /// <summary>
    ///     IP address used for the query.
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    ///     Continent with its code, e.g. <c>Europe (EU)</c>.
    /// </summary>
    [JsonIgnore]
    public string ContinentWithCode => FormatNameWithCode(Continent, ContinentCode);

    /// <summary>
    ///     Country with its code, e.g. <c>Germany (DE)</c>.
    /// </summary>
    [JsonIgnore]
    public string CountryWithCode => FormatNameWithCode(Country, CountryCode);

    /// <summary>
    ///     Region with its code, e.g. <c>Hesse (HE)</c>.
    /// </summary>
    [JsonIgnore]
    public string RegionWithCode => FormatNameWithCode(RegionName, Region);

    /// <summary>
    ///     Offset from UTC, e.g. <c>UTC+01:00</c>.
    /// </summary>
    [JsonIgnore]
    public string UtcOffset => FormatUtcOffset(Offset);

    /// <summary>
    ///     Name with its code in brackets, without empty brackets if the code is missing.
    /// </summary>
    /// <param name="name">Name, e.g. <c>Germany</c>.</param>
    /// <param name="code">Code, e.g. <c>DE</c>.</param>
    /// <returns>e.g. <c>Germany (DE)</c>, <c>null</c> if both are empty.</returns>
    public static string FormatNameWithCode(string name, string code)
    {
        if (string.IsNullOrEmpty(name))
            return string.IsNullOrEmpty(code) ? null : code;

        return string.IsNullOrEmpty(code) ? name : $"{name} ({code})";
    }

    /// <summary>
    ///     Offset from UTC (ip-api.com: seconds, incl. daylight saving time) as text.
    /// </summary>
    /// <param name="offsetSeconds">Offset in seconds, e.g. <c>3600</c>.</param>
    /// <returns>e.g. <c>UTC+01:00</c>, <c>UTC+00:00</c> for 0, <c>UTC-03:30</c>.</returns>
    public static string FormatUtcOffset(int offsetSeconds)
    {
        var offset = TimeSpan.FromSeconds(offsetSeconds);

        return $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset:hh\\:mm}";
    }
}