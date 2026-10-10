namespace NETworkManager.Models.RDAP;

/// <summary>
///     IANA RDAP bootstrap files (RFC 9224, RFC 8521) published at https://data.iana.org/rdap/.
/// </summary>
public enum RDAPBootstrapFileType
{
    DNS,
    IPv4,
    IPv6,
    ASN,
    ObjectTags
}
