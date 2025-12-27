using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Win32;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     Provides functionality to manage network interfaces.
/// </summary>
public sealed class NetworkInterface
{
    #region Variables

    /* Ref #3286
    private static List<string> NetworkInterfacesBlacklist =
    [
        "Hyper-V Virtual Switch Extension Filter",
        "WFP Native MAC Layer LightWeight Filter",
        "Npcap Packet Driver (NPCAP)",
        "QoS Packet Scheduler",
        "WFP 802.3 MAC Layer LightWeight Filter",
        "Ethernet (Kerneldebugger)",
        "Filter Driver"
    ];
    */

    #endregion

    #region Methods

    /// <summary>
    ///     Gets a list of all available network interfaces asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="NetworkInterfaceInfo"/>.</returns>
    public static Task<List<NetworkInterfaceInfo>> GetNetworkInterfacesAsync()
    {
        return Task.Run(GetNetworkInterfaces);
    }

    /// <summary>
    ///     Gets a list of all available network interfaces.
    /// </summary>
    /// <returns>A list of <see cref="NetworkInterfaceInfo"/> describing the available network interfaces.</returns>
    public static List<NetworkInterfaceInfo> GetNetworkInterfaces()
    {
        List<NetworkInterfaceInfo> listNetworkInterfaceInfo = [];

        foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            // NetworkInterfaceType 53 is proprietary virtual/internal interface
            // https://docs.microsoft.com/en-us/windows-hardware/drivers/network/ndis-interface-types
            if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                (int)networkInterface.NetworkInterfaceType != 53)
                continue;

            // Check if part of the  Name is in blacklist Ref #3286
            //if (NetworkInterfacesBlacklist.Any(networkInterface.Name.Contains))
            //    continue;

            //Debug.WriteLine(networkInterface.Name);
            //Debug.WriteLine($"  Description: {networkInterface.Description}");

            var listIPv4Address = new List<Tuple<IPAddress, IPAddress>>();
            var listIPv6AddressLinkLocal = new List<IPAddress>();
            var listIPv6Address = new List<IPAddress>();

            var dhcpLeaseObtained = new DateTime();
            var dhcpLeaseExpires = new DateTime();

            var ipProperties = networkInterface.GetIPProperties();

            foreach (var unicastIPAddrInfo in ipProperties.UnicastAddresses)
            {
                switch (unicastIPAddrInfo.Address.AddressFamily)
                {
                    case AddressFamily.InterNetwork:

                        listIPv4Address.Add(new Tuple<IPAddress, IPAddress>(unicastIPAddrInfo.Address,
                            unicastIPAddrInfo.IPv4Mask));
                        dhcpLeaseExpires =
                            (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressPreferredLifetime))
                            .ToLocalTime();
                        dhcpLeaseObtained =
                            (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressValidLifetime) -
                             TimeSpan.FromSeconds(unicastIPAddrInfo.DhcpLeaseLifetime)).ToLocalTime();
                        break;
                    case AddressFamily.InterNetworkV6 when unicastIPAddrInfo.Address.IsIPv6LinkLocal:
                        listIPv6AddressLinkLocal.Add(unicastIPAddrInfo.Address);
                        break;
                    case AddressFamily.InterNetworkV6:
                        listIPv6Address.Add(unicastIPAddrInfo.Address);
                        break;
                }
            }

            var listIPv4Gateway = new List<IPAddress>();
            var listIPv6Gateway = new List<IPAddress>();

            foreach (var gatewayIPAddrInfo in ipProperties.GatewayAddresses)
            {
                switch (gatewayIPAddrInfo.Address.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        listIPv4Gateway.Add(gatewayIPAddrInfo.Address);
                        break;
                    case AddressFamily.InterNetworkV6:
                        listIPv6Gateway.Add(gatewayIPAddrInfo.Address);
                        break;
                }
            }

            // Check if autoconfiguration for DNS is enabled (only possible via registry key)
            var nameServerKey =
                Registry.LocalMachine.OpenSubKey(
                    $@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{networkInterface.Id}");
            var dnsAutoconfigurationEnabled = nameServerKey?.GetValue("NameServer") != null &&
                                              string.IsNullOrEmpty(nameServerKey.GetValue("NameServer")?.ToString());

            // Check if IPv4 protocol is available
            var ipv4ProtocolAvailable = true;
            IPv4InterfaceProperties ipv4Properties = null;

            try
            {
                ipv4Properties = ipProperties.GetIPv4Properties();
            }
            catch (NetworkInformationException)
            {
                ipv4ProtocolAvailable = false;
            }

            // Check if IPv6 protocol is available
            var ipv6ProtocolAvailable = true;

            try
            {
                ipProperties.GetIPv6Properties();
            }
            catch (NetworkInformationException)
            {
                ipv6ProtocolAvailable = false;
            }

            listNetworkInterfaceInfo.Add(new NetworkInterfaceInfo
            {
                Id = networkInterface.Id,
                Name = networkInterface.Name,
                Description = networkInterface.Description,
                Type = networkInterface.NetworkInterfaceType.ToString(),
                PhysicalAddress = networkInterface.GetPhysicalAddress(),
                Status = networkInterface.OperationalStatus,
                IsOperational = networkInterface.OperationalStatus == OperationalStatus.Up,
                Speed = networkInterface.Speed,
                IPv4ProtocolAvailable = ipv4ProtocolAvailable,
                IPv4Address = [.. listIPv4Address],
                IPv4Gateway = [.. listIPv4Gateway],
                DhcpEnabled = ipv4Properties is { IsDhcpEnabled: true },
                DhcpServer = [.. ipProperties.DhcpServerAddresses.Where(dhcpServerIPAddress =>
                    dhcpServerIPAddress.AddressFamily == AddressFamily.InterNetwork)],
                DhcpLeaseObtained = dhcpLeaseObtained,
                DhcpLeaseExpires = dhcpLeaseExpires,
                IPv6ProtocolAvailable = ipv6ProtocolAvailable,
                IPv6AddressLinkLocal = [.. listIPv6AddressLinkLocal],
                IPv6Address = [.. listIPv6Address],
                IPv6Gateway = [.. listIPv6Gateway],
                DNSAutoconfigurationEnabled = dnsAutoconfigurationEnabled,
                DNSSuffix = ipProperties.DnsSuffix,
                DNSServer = [.. ipProperties.DnsAddresses]
            });
        }

        return listNetworkInterfaceInfo;
    }

    /// <summary>
    ///     Detects the local IP address from routing to a remote IP address asynchronously.
    /// </summary>
    /// <param name="remoteIPAddress">The remote IP address to check routing against.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the local <see cref="IPAddress"/> used to reach the remote address or null on error.</returns>
    public static Task<IPAddress> DetectLocalIPAddressBasedOnRoutingAsync(IPAddress remoteIPAddress)
    {
        return Task.Run(() => DetectLocalIPAddressFromRouting(remoteIPAddress));
    }

    /// <summary>
    ///     Detects the local IP address from routing to a remote IP address.
    /// </summary>
    /// <param name="remoteIPAddress">The remote IP address to check routing against.</param>
    /// <returns>The local <see cref="IPAddress"/> used to reach the remote address or null on error.</returns>
    private static IPAddress DetectLocalIPAddressFromRouting(IPAddress remoteIPAddress)
    {
        var isIPv4 = remoteIPAddress.AddressFamily == AddressFamily.InterNetwork;

        using var socket = new Socket(remoteIPAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        // return null on error...
        try
        {
            socket.Bind(new IPEndPoint(isIPv4 ? IPAddress.Any : IPAddress.IPv6Any, 0));
            socket.Connect(new IPEndPoint(remoteIPAddress, 0));

            if (socket.LocalEndPoint is IPEndPoint ipAddress)
                return ipAddress.Address;
        }
        catch (SocketException) { }

        return null;
    }

    /// <summary>
    /// Asynchronously detects the local IP address associated with a network interface that matches the specified
    /// address family.
    /// </summary>
    /// <param name="addressFamily">The address family to use when searching for a local IP address. Typically, use AddressFamily.InterNetwork for
    /// IPv4 or AddressFamily.InterNetworkV6 for IPv6.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the detected local IP address, or
    /// null if no suitable address is found.</returns>
    public static Task<IPAddress> DetectLocalIPAddressFromNetworkInterfaceAsync(AddressFamily addressFamily)
    {
        return Task.Run(() => DetectLocalIPAddressFromNetworkInterface(addressFamily));
    }

    /// <summary>
    /// Detects and returns the first local IP address assigned to an operational network interface that matches the
    /// specified address family.
    /// </summary>
    /// <remarks>For IPv4, the method prefers non-link-local addresses but will return a link-local address if
    /// no other is available. For IPv6, the method returns the first global or unique local address if present;
    /// otherwise, it returns a link-local address if available. The returned address is selected from operational
    /// network interfaces only.</remarks>
    /// <param name="addressFamily">The address family to search for. Specify <see cref="AddressFamily.InterNetwork"/> for IPv4 addresses or <see
    /// cref="AddressFamily.InterNetworkV6"/> for IPv6 addresses.</param>
    /// <returns>An <see cref="IPAddress"/> representing the first detected local IP address for the specified address family, or
    /// <see langword="null"/> if no suitable address is found.</returns>
    public static IPAddress DetectLocalIPAddressFromNetworkInterface(AddressFamily addressFamily)
    {
        // Filter operational network interfaces
        var networkInterfaces = GetNetworkInterfaces()
            .Where(x => x.IsOperational);

        var candidates = new List<IPAddress>();

        // IPv4
        if (addressFamily == AddressFamily.InterNetwork)
        {           
            foreach (var networkInterface in networkInterfaces)
            {
                foreach (var ipAddress in networkInterface.IPv4Address)
                    candidates.Add(ipAddress.Item1);
            }

            // Prefer non-link-local addresses
            var nonLinkLocal = candidates.Where(x =>
            {
                var bytes = x.GetAddressBytes();

                return !(bytes[0] == 169 && bytes[1] == 254);
            });

            // Return first non-link-local or first candidate if none found (might be null - no addresses at all)
            return nonLinkLocal.Any() ? nonLinkLocal.First() : candidates.First();
        }

        // IPv6
        if (addressFamily == AddressFamily.InterNetworkV6)
        {
            // First try to get global or unique local addresses
            foreach (var networkInterface in networkInterfaces)
            {
                candidates.AddRange(networkInterface.IPv6Address);
            }

            // Return first candidate if any found
            if (candidates.Count != 0)
                return candidates.First();

            // Fallback to link-local addresses
            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.IPv6AddressLinkLocal.Length != 0)
                    return networkInterface.IPv6AddressLinkLocal.First();
            }
        }

        return null;
    }

    /// <summary>
    ///     Detects the gateway IP address from a local IP address asynchronously.
    /// </summary>
    /// <param name="localIPAddress">The local IP address to find the gateway for.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the gateway as <see cref="IPAddress"/> or null if not found.</returns>
    public static Task<IPAddress> DetectGatewayFromLocalIPAddressAsync(IPAddress localIPAddress)
    {
        return Task.Run(() => DetectGatewayFromLocalIPAddress(localIPAddress));
    }

    /// <summary>
    ///     Detects the gateway IP address from a local IP address.
    /// </summary>
    /// <param name="localIPAddress">The local IP address to find the gateway for.</param>
    /// <returns>The gateway as <see cref="IPAddress"/> or null if not found.</returns>    
    private static IPAddress DetectGatewayFromLocalIPAddress(IPAddress localIPAddress)
    {
        foreach (var networkInterface in GetNetworkInterfaces())
        {
            // IPv4
            if (localIPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (networkInterface.IPv4Address.Any(x => x.Item1.Equals(localIPAddress)))
                    return networkInterface.IPv4Gateway.FirstOrDefault();
            }

            // IPv6
            if (localIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (networkInterface.IPv6Address.Contains(localIPAddress))
                    return networkInterface.IPv6Gateway.FirstOrDefault();
            }            
        }

        return null;
    }

    /// <summary>
    ///     Configures a network interface with the specified configuration asynchronously.
    /// </summary>
    /// <param name="config">The configuration to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ConfigureNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => ConfigureNetworkInterface(config));
    }

    /// <summary>
    ///     Configures a network interface with the specified configuration.
    /// </summary>
    /// <param name="config">The configuration to apply.</param>
    private void ConfigureNetworkInterface(NetworkInterfaceConfig config)
    {
        // IP
        var command = $"netsh interface ipv4 set address name='{config.Name}'";
        command += config.EnableStaticIPAddress
            ? $" source=static address={config.IPAddress} mask={config.Subnetmask} gateway={config.Gateway};"
            : " source=dhcp;";

        // DNS
        command += $"netsh interface ipv4 set DNSservers name='{config.Name}'";
        command += config.EnableStaticDNS
            ? $" source=static address={config.PrimaryDNSServer} register=primary validate=no;"
            : " source=dhcp;";
        command += config.EnableStaticDNS && !string.IsNullOrEmpty(config.SecondaryDNSServer)
            ? $"netsh interface ipv4 add DNSservers name='{config.Name}' address={config.SecondaryDNSServer} index=2 validate=no;"
            : "";

        try
        {
            PowerShellHelper.ExecuteCommand(command, true);
        }
        catch (Win32Exception win32Ex)
        {
            switch (win32Ex.NativeErrorCode)
            {
                case 1223:
                    OnUserHasCanceled();
                    break;
                default:
                    throw;
            }
        }
    }

    /// <summary>
    ///     Flush the DNS cache asynchronously.
    /// </summary>
    /// <returns>Running task.</returns>
    public static Task FlushDnsAsync()
    {
        return Task.Run(FlushDns);
    }

    /// <summary>
    ///     Flush the DNS cache.
    /// </summary>
    private static void FlushDns()
    {
        const string command = "ipconfig /flushdns;";

        PowerShellHelper.ExecuteCommand(command);
    }

    /// <summary>
    ///     Release or renew the IP address of the specified network adapter asynchronously.
    /// </summary>
    /// <param name="mode">ipconfig.exe modes which are used like /release(6) or /renew(6)</param>
    /// <param name="adapterName">Name of the ethernet adapter.</param>
    /// <returns>Running task.</returns>
    public static Task ReleaseRenewAsync(IPConfigReleaseRenewMode mode, string adapterName)
    {
        return Task.Run(() => ReleaseRenew(mode, adapterName));
    }

    /// <summary>
    ///     Release or renew the IP address of the specified network adapter.
    /// </summary>
    /// <param name="mode">ipconfig.exe modes which are used like /release(6) or /renew(6)</param>
    /// <param name="adapterName">Name of the ethernet adapter.</param>
    private static void ReleaseRenew(IPConfigReleaseRenewMode mode, string adapterName)
    {
        var command = string.Empty;

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew or IPConfigReleaseRenewMode.Release)
            command += $"ipconfig /release '{adapterName}';";

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew or IPConfigReleaseRenewMode.Renew)
            command += $"ipconfig /renew '{adapterName}';";

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew6 or IPConfigReleaseRenewMode.Release6)
            command += $"ipconfig /release6 '{adapterName}';";

        if (mode is IPConfigReleaseRenewMode.ReleaseRenew6 or IPConfigReleaseRenewMode.Renew6)
            command += $"ipconfig /renew6 '{adapterName}';";

        PowerShellHelper.ExecuteCommand(command);
    }

    /// <summary>
    ///     Add an IP address to a network interface asynchronously.
    /// </summary>
    /// <param name="config">Ethernet adapter name, IP address and subnetmask.</param>
    /// <returns>Running task.</returns>
    public static Task AddIPAddressToNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => AddIPAddressToNetworkInterface(config));
    }

    /// <summary>
    ///     Add an IP address to a network interface.
    /// </summary>
    /// <param name="config">Ethernet adapter name, IP address and subnetmask.</param>
    private static void AddIPAddressToNetworkInterface(NetworkInterfaceConfig config)
    {
        var command = string.Empty;

        if (config.EnableDhcpStaticIpCoexistence)
            command += $"netsh interface ipv4 set interface interface='{config.Name}' dhcpstaticipcoexistence=enabled;";

        command += $"netsh interface ipv4 add address '{config.Name}' {config.IPAddress} {config.Subnetmask};";

        PowerShellHelper.ExecuteCommand(command, true);
    }

    /// <summary>
    ///     Remove an IP address from a network interface asynchronously.
    /// </summary>
    /// <param name="config">Ethernet adapter name, IP address</param>
    /// <returns>Running task.</returns>
    public static Task RemoveIPAddressFromNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => RemoveIPAddressFromNetworkInterface(config));
    }

    /// <summary>
    ///     Remove an IP address from a network interface.
    /// </summary>
    /// <param name="config">Ethernet adapter name, IP address</param>
    private static void RemoveIPAddressFromNetworkInterface(NetworkInterfaceConfig config)
    {
        var command = $"netsh interface ipv4 delete address '{config.Name}' {config.IPAddress};";

        PowerShellHelper.ExecuteCommand(command, true);
    }

    #endregion


    #region Events

    /// <summary>
    ///     Occurs when the user has canceled an operation (e.g. UAC prompt).
    /// </summary>
    public event EventHandler UserHasCanceled;

    private void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

}