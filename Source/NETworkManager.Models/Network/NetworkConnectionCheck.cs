using System.Net;
using System.Net.Sockets;

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
}
