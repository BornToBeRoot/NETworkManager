namespace NETworkManager.Models.RDAP;

/// <summary>
///     Types of RDAP queries supported by the RDAP tool.
/// </summary>
public enum RDAPQueryType
{
    /// <summary>
    ///     Detects the type from the input (see <see cref="RDAPQueryParser.DetectType" />).
    /// </summary>
    Auto,

    /// <summary>
    ///     Domain name (e.g. example.com), resolved via the IANA DNS bootstrap.
    /// </summary>
    Domain,

    /// <summary>
    ///     Nameserver (e.g. a.iana-servers.net), resolved via the IANA DNS bootstrap (registry of its TLD).
    /// </summary>
    Nameserver,

    /// <summary>
    ///     Top-level domain (e.g. de), queried directly against the IANA root zone RDAP server.
    /// </summary>
    TLD,

    /// <summary>
    ///     IPv4 or IPv6 address or CIDR prefix, resolved via the IANA IPv4/IPv6 bootstrap.
    /// </summary>
    IPAddress,

    /// <summary>
    ///     Autonomous system number, resolved via the IANA ASN bootstrap.
    /// </summary>
    ASN,

    /// <summary>
    ///     Entity handle (e.g. ORG-RIEN1-RIPE), resolved via the IANA object tags bootstrap.
    /// </summary>
    Entity
}
