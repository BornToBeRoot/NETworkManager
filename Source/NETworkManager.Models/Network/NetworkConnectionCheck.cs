using System.Net;
using System.Net.Sockets;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Result of a network connection check (e.g. Computer > IPv4): the state and its reason.
/// </summary>
/// <param name="State">State of the check.</param>
/// <param name="Reason">Reason of the state (shown as tooltip), <see cref="ConnectionCheckReason.None" /> if OK.</param>
public sealed record ConnectionCheckResult(ConnectionState State, ConnectionCheckReason Reason = ConnectionCheckReason.None);

/// <summary>
///     Decisions of the network connection check (dashboard widget and status window): which state a detected value
///     gets. The detection itself is done by the view model.
/// </summary>
public static class NetworkConnectionCheck
{
    /// <summary>
    ///     State of the local IP address of the computer.
    /// </summary>
    /// <remarks>
    ///     IPv4: missing → critical; link-local (no DHCP server) or not routed (taken from the first active network
    ///     adapter) → warning. IPv6: missing, link-local only or not routed → info (the network or the internet provider
    ///     may not support IPv6, which is not an error).
    /// </remarks>
    /// <param name="ipAddress">Local IP address, or <c>null</c> if none was detected.</param>
    /// <param name="addressFamily">IPv4 or IPv6.</param>
    /// <param name="isRouted">The address was detected via the route to the public IP address (not the fallback).</param>
    /// <returns>State and reason.</returns>
    public static ConnectionCheckResult GetLocalIPAddressResult(IPAddress ipAddress, AddressFamily addressFamily,
        bool isRouted)
    {
        if (addressFamily == AddressFamily.InterNetworkV6)
            return ipAddress == null || !isRouted || NetworkInterface.IsLinkLocal(ipAddress)
                ? new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)
                : new ConnectionCheckResult(ConnectionState.OK);

        if (ipAddress == null)
            return new ConnectionCheckResult(ConnectionState.Critical, ConnectionCheckReason.NoIPv4Address);

        if (NetworkInterface.IsLinkLocal(ipAddress))
            return new ConnectionCheckResult(ConnectionState.Warning, ConnectionCheckReason.IPv4LinkLocal);

        return isRouted
            ? new ConnectionCheckResult(ConnectionState.OK)
            : new ConnectionCheckResult(ConnectionState.Warning, ConnectionCheckReason.NoRoute);
    }

    /// <summary>
    ///     Decides whether the DNS name (PTR) is also resolved for the IPv6 address: if the IPv4 address was not resolved
    ///     or has no record. Not after a DNS server error, the same server would fail again (and the timeout doubles).
    /// </summary>
    /// <param name="ipv4Result">Result of the IPv4 lookup, or <c>null</c> if there was no IPv4 address.</param>
    /// <returns><see langword="true" /> if the IPv6 address should be resolved.</returns>
    public static bool ShouldResolveIPv6(DNSClientResultString ipv4Result)
    {
        return ipv4Result is null or { HasError: true, IsNotFound: true };
    }

    /// <summary>
    ///     State of the DNS name (PTR) of the IPv4 / IPv6 address: a resolved name of either address is OK. Otherwise a
    ///     clean "not found" of either address is info (many networks have no reverse DNS), not a broken DNS setup.
    /// </summary>
    /// <param name="ipv4Result">Result of the IPv4 lookup, or <c>null</c> if not resolved.</param>
    /// <param name="ipv6Result">Result of the IPv6 lookup, or <c>null</c> if not resolved.</param>
    /// <returns>State and reason, and the resolved name (<c>null</c> if none).</returns>
    public static (ConnectionCheckResult Result, string Hostname) GetDNSResult(DNSClientResultString ipv4Result,
        DNSClientResultString ipv6Result)
    {
        if (ipv4Result == null && ipv6Result == null)
            return (new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.NoIPAddressToResolve), null);

        var resolved = ipv4Result is { HasError: false } ? ipv4Result :
            ipv6Result is { HasError: false } ? ipv6Result : null;

        if (resolved != null)
            return (new ConnectionCheckResult(ConnectionState.OK), resolved.Value);

        return ipv4Result is { IsNotFound: true } || ipv6Result is { IsNotFound: true }
            ? (new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.PTRNotFound), null)
            : (new ConnectionCheckResult(ConnectionState.Critical, ConnectionCheckReason.DNSError), null);
    }

    /// <summary>
    ///     State of the gateway (router): found and replies to the ping → OK; no reply → warning (some routers block
    ///     ICMP). Not found → warning if the local IP address is routed anyway (e.g. a VPN adapter without a gateway),
    ///     otherwise critical.
    /// </summary>
    /// <param name="gateway">Gateway, or <c>null</c> if none was found.</param>
    /// <param name="isRouted">The local IP address was detected via the route to the public IP address.</param>
    /// <param name="isReachable">The gateway replied to the ping.</param>
    /// <returns>State and reason.</returns>
    public static ConnectionCheckResult GetGatewayResult(IPAddress gateway, bool isRouted, bool isReachable)
    {
        if (gateway == null)
            return new ConnectionCheckResult(isRouted ? ConnectionState.Warning : ConnectionState.Critical,
                ConnectionCheckReason.NoGateway);

        return isReachable
            ? new ConnectionCheckResult(ConnectionState.OK)
            : new ConnectionCheckResult(ConnectionState.Warning, ConnectionCheckReason.GatewayNoReply);
    }
}
