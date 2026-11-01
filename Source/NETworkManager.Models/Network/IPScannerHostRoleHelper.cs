using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Gets the roles of hosts for the local computer (own IP addresses, default gateways, DNS and DHCP servers) from
///     the configuration of the local network interfaces.
/// </summary>
public static class IPScannerHostRoleHelper
{
    /// <summary>
    ///     Gets the roles of the IP addresses known from the operational local network interfaces.
    /// </summary>
    /// <param name="networkInterfaces">
    ///     Local network interfaces, see <see cref="NetworkInterface.GetNetworkInterfaces" />.
    /// </param>
    /// <returns>
    ///     Roles per IP address, IPv6 with its scope id (a link-local address is only unique on its interface, see
    ///     <see cref="GetRole" />).
    /// </returns>
    public static Dictionary<IPAddress, IPScannerHostRole> GetRoles(
        IEnumerable<NetworkInterfaceInfo> networkInterfaces)
    {
        var roles = new Dictionary<IPAddress, IPScannerHostRole>();

        void Add(IEnumerable<IPAddress> ipAddresses, IPScannerHostRole role)
        {
            foreach (var ipAddress in ipAddresses ?? [])
                roles[ipAddress] = roles.GetValueOrDefault(ipAddress) | role;
        }

        foreach (var networkInterface in networkInterfaces.Where(x => x.IsOperational))
        {
            Add(networkInterface.IPv4Address?.Select(x => x.Item1), IPScannerHostRole.LocalComputer);
            Add(networkInterface.IPv6Address, IPScannerHostRole.LocalComputer);
            Add(networkInterface.IPv6AddressLinkLocal, IPScannerHostRole.LocalComputer);
            Add(networkInterface.IPv4Gateway, IPScannerHostRole.DefaultGateway);
            Add(networkInterface.IPv6Gateway, IPScannerHostRole.DefaultGateway);
            Add(networkInterface.DNSServer, IPScannerHostRole.DNSServer);
            Add(networkInterface.DhcpServer, IPScannerHostRole.DHCPServer);
        }

        return roles;
    }

    /// <summary>
    ///     Gets the roles of a host. An IPv6 address with a scope id only matches the address on the same interface
    ///     (e.g. the gateway "fe80::1%12" is not the host "fe80::1%7" on another interface) or a known address without
    ///     scope id; an address without scope id (e.g. entered as "fe80::1") matches it on every interface - the roles
    ///     of all of them.
    /// </summary>
    /// <param name="roles">Roles per IP address, see <see cref="GetRoles" />.</param>
    /// <param name="ipAddress">IP address of the host (an IPv6 address may have a scope id).</param>
    /// <returns>Roles of the host, <see cref="IPScannerHostRole.None" /> if it has none.</returns>
    public static IPScannerHostRole GetRole(IReadOnlyDictionary<IPAddress, IPScannerHostRole> roles,
        IPAddress ipAddress)
    {
        var role = roles.GetValueOrDefault(ipAddress);

        // IPv4, or no scope id known for the address
        if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
            return role;

        var withoutScopeId = IPAddressHelper.WithoutScopeId(ipAddress);

        // With scope id: also a known address without scope id (e.g. a DNS server configured without one)
        if (ipAddress.ScopeId != 0)
            return role | roles.GetValueOrDefault(withoutScopeId);

        // Without scope id: the interface is not known - the address on every interface
        foreach (var (key, keyRole) in roles)
        {
            if (key.AddressFamily == AddressFamily.InterNetworkV6 && key.ScopeId != 0 &&
                IPAddressHelper.WithoutScopeId(key).Equals(withoutScopeId))
                role |= keyRole;
        }

        return role;
    }

    /// <summary>
    ///     Gets the most important role of a host (for its icon): local computer, default gateway, DNS server, DHCP
    ///     server.
    /// </summary>
    /// <param name="role">Roles of the host.</param>
    /// <returns>Most important role, <see cref="IPScannerHostRole.None" /> if it has none.</returns>
    public static IPScannerHostRole GetPrimaryRole(IPScannerHostRole role)
    {
        foreach (var primaryRole in (IPScannerHostRole[])
                 [IPScannerHostRole.LocalComputer, IPScannerHostRole.DefaultGateway, IPScannerHostRole.DNSServer,
                     IPScannerHostRole.DHCPServer])
        {
            if (role.HasFlag(primaryRole))
                return primaryRole;
        }

        return IPScannerHostRole.None;
    }
}
