using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// <returns>Roles per IP address (without IPv6 scope id, see <see cref="GetRole" />).</returns>
    public static Dictionary<IPAddress, IPScannerHostRole> GetRoles(
        IEnumerable<NetworkInterfaceInfo> networkInterfaces)
    {
        var roles = new Dictionary<IPAddress, IPScannerHostRole>();

        void Add(IEnumerable<IPAddress> ipAddresses, IPScannerHostRole role)
        {
            foreach (var ipAddress in ipAddresses ?? [])
            {
                var key = IPAddressHelper.WithoutScopeId(ipAddress);

                roles[key] = roles.GetValueOrDefault(key) | role;
            }
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
    ///     Gets the roles of a host.
    /// </summary>
    /// <param name="roles">Roles per IP address, see <see cref="GetRoles" />.</param>
    /// <param name="ipAddress">IP address of the host (an IPv6 address may have a scope id).</param>
    /// <returns>Roles of the host, <see cref="IPScannerHostRole.None" /> if it has none.</returns>
    public static IPScannerHostRole GetRole(IReadOnlyDictionary<IPAddress, IPScannerHostRole> roles,
        IPAddress ipAddress)
    {
        return roles.GetValueOrDefault(IPAddressHelper.WithoutScopeId(ipAddress));
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
