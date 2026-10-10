using System.Net;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Normalized RDAP query, created by <see cref="RDAPQueryParser" />.
/// </summary>
public class RDAPQuery
{
    /// <summary>
    ///     Type of the query.
    /// </summary>
    public RDAPQueryType Type { get; init; }

    /// <summary>
    ///     Normalized value as sent on the wire (A-label domain, asplain ASN, CIDR, handle).
    /// </summary>
    public string Value { get; init; }

    /// <summary>
    ///     Path segment that is appended to a base RDAP URL (e.g. "domain/example.com").
    /// </summary>
    public string PathSegment { get; init; }

    /// <summary>
    ///     Domain name in A-label form (Domain and TLD queries only).
    /// </summary>
    public string DomainName { get; init; }

    /// <summary>
    ///     IP address or network address of the prefix (IP address queries only).
    /// </summary>
    public IPAddress IPAddress { get; init; }

    /// <summary>
    ///     Prefix length, or null if a single address is queried (IP address queries only).
    /// </summary>
    public int? PrefixLength { get; init; }

    /// <summary>
    ///     Autonomous system number (ASN queries only).
    /// </summary>
    public uint ASN { get; init; }

    /// <summary>
    ///     Entity handle (Entity queries only).
    /// </summary>
    public string Handle { get; init; }
}
