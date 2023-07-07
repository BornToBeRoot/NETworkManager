namespace NETworkManager.Models.IPApi;

/// <summary>
/// Class for (E)DNS server data used for deserialization.
/// </summary>
public class DNSResolverDeserializationInfo
{
    /// <summary>
    /// DNS server data.
    /// </summary>
    public DNSResolverDeserializationBaseInfo Dns { get; set; }

    /// <summary>
    /// EDNS server data.
    /// </summary>
    public DNSResolverDeserializationBaseInfo Edns { get; set; }
}
