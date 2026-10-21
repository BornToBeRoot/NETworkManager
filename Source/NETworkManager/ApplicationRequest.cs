using DnsClient;
using NETworkManager.Models;
using NETworkManager.Models.RDAP;
using NETworkManager.Profiles;

namespace NETworkManager;

/// <summary>
///     Request to open an application (tool), optionally with a target or a profile, e.g. from the run command or from
///     another tool ("Open in ..."). Applications that support more options have a derived request.
/// </summary>
/// <param name="Application">Application to open.</param>
public record ApplicationRequest(ApplicationName Application)
{
    /// <summary>
    ///     Target of the application, e.g. host(s), IP range, query or URL. If null (and no <see cref="Profile" /> is set),
    ///     the application is only opened.
    /// </summary>
    public string Target { get; init; }

    /// <summary>
    ///     Profile to run in the application with its settings. <see cref="Target" /> is ignored if set.
    /// </summary>
    public ProfileInfo Profile { get; init; }
}

/// <summary>
///     Request to open the DNS Lookup.
/// </summary>
public sealed record DNSLookupRequest() : ApplicationRequest(ApplicationName.DNSLookup)
{
    /// <summary>
    ///     DNS server(s) to use (e.g. "1.1.1.1"). If null, the DNS server selected in the application is used.
    /// </summary>
    public string DNSServer { get; init; }

    /// <summary>
    ///     Query type. If null, the query type selected in the application is used.
    /// </summary>
    public QueryType? QueryType { get; init; }
}

/// <summary>
///     Request to open the Port Scanner.
/// </summary>
public sealed record PortScannerRequest() : ApplicationRequest(ApplicationName.PortScanner)
{
    /// <summary>
    ///     Ports to scan (e.g. "22;80;443" or "1-1024"). If null, the ports of the application are used.
    /// </summary>
    public string Ports { get; init; }
}

/// <summary>
///     Request to open RDAP.
/// </summary>
public sealed record RDAPRequest() : ApplicationRequest(ApplicationName.RDAP)
{
    /// <summary>
    ///     Query type. If null, the type is detected from the query (<see cref="RDAPQueryType.Auto" />).
    /// </summary>
    public RDAPQueryType? QueryType { get; init; }
}
