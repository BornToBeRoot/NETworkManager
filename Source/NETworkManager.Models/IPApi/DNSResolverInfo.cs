namespace NETworkManager.Models.IPApi;

/// <summary>
/// Class contains the DNS resolver information.
/// </summary>
public class DNSResolverInfo
{
    /// <summary>
    /// IP address of the DNS server.
    /// </summary>
    public string DnsIp { get; set; }

    /// <summary>
    /// Geographic location of the DNS server.
    /// </summary>
    public string DnsGeo { get; set; }


    /// <summary>
    /// IP address of the Edns server.
    /// </summary>
    public string EdnsIp { get; set; }

    /// <summary>
    /// Geographic location of the Edns server.
    /// </summary>
    public string EdnsGeo { get; set; }

    /// <summary>
    /// Create an instance of <see cref="DNSResolverInfo"/> with parameters.
    /// </summary>
    /// <param name="dnsIp">IP address of the DNS server.</param>
    /// <param name="dnsGeo">Geographic location of the DNS server.</param>
    /// <param name="ednsIp">IP address of the EDNS server.</param>
    /// <param name="ednsGeo">Geographic location of the EDNS server.</param>
    public DNSResolverInfo(string dnsIp, string dnsGeo, string ednsIp, string ednsGeo)
    {
        DnsIp = dnsIp;
        DnsGeo = dnsGeo;
        EdnsIp = ednsIp;
        EdnsGeo = ednsGeo;
    }

    /// <summary>
    /// Parses the DNS resolver from the deserialization info.
    /// </summary>
    /// <param name="info"><see cref="DNSResolverDeserializationInfo"/> to parse to <see cref="DNSResolverInfo"/>.</param>
    /// <returns>New instance of <see cref="DNSResolverInfo"/> from <see cref="DNSResolverDeserializationInfo"/> data.</returns>
    public static DNSResolverInfo Parse(DNSResolverDeserializationInfo info)
    {
        // Strings can be null if no data is available.
        return new DNSResolverInfo(info.Dns?.Ip, info.Dns?.Geo, info.Edns?.Ip, info.Edns?.Geo);
    }
}
