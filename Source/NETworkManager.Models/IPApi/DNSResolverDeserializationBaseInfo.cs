namespace NETworkManager.Models.IPApi;

/// <summary>
///     Base class for (E)DNS server data used for deserialization.
/// </summary>
public class DNSResolverDeserializationBaseInfo
{
    /// <summary>
    ///     IP address of the (E)DNS server.
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    ///     Geographic location of the (E)DNS server.
    /// </summary>
    public string Geo { get; set; }
}