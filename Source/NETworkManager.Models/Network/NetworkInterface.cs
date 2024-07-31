using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Win32;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

public sealed class NetworkInterface
{
    #region Events

    public event EventHandler UserHasCanceled;

    private void OnUserHasCanceled()
    {
        UserHasCanceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Methods

    public static Task<List<NetworkInterfaceInfo>> GetNetworkInterfacesAsync()
    {
        return Task.Run(GetNetworkInterfaces);
    }

    public static List<NetworkInterfaceInfo> GetNetworkInterfaces()
    {
        List<NetworkInterfaceInfo> listNetworkInterfaceInfo = new();

        foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            // NetworkInterfaceType 53 is proprietary virtual/internal interface
            // https://docs.microsoft.com/en-us/windows-hardware/drivers/network/ndis-interface-types
            if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                (int)networkInterface.NetworkInterfaceType != 53)
                continue;

            var listIPv4Address = new List<Tuple<IPAddress, IPAddress>>();
            var listIPv6AddressLinkLocal = new List<IPAddress>();
            var listIPv6Address = new List<IPAddress>();

            var dhcpLeaseObtained = new DateTime();
            var dhcpLeaseExpires = new DateTime();

            var ipProperties = networkInterface.GetIPProperties();

            foreach (var unicastIPAddrInfo in ipProperties.UnicastAddresses)
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

            var listIPv4Gateway = new List<IPAddress>();
            var listIPv6Gateway = new List<IPAddress>();

            foreach (var gatewayIPAddrInfo in ipProperties.GatewayAddresses)
                switch (gatewayIPAddrInfo.Address.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        listIPv4Gateway.Add(gatewayIPAddrInfo.Address);
                        break;
                    case AddressFamily.InterNetworkV6:
                        listIPv6Gateway.Add(gatewayIPAddrInfo.Address);
                        break;
                }

            // Check if autoconfiguration for DNS is enabled (only via registry key)
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
                IPv4Address = listIPv4Address.ToArray(),
                IPv4Gateway = listIPv4Gateway.ToArray(),
                DhcpEnabled = ipv4Properties is { IsDhcpEnabled: true },
                DhcpServer = ipProperties.DhcpServerAddresses.Where(dhcpServerIPAddress =>
                    dhcpServerIPAddress.AddressFamily == AddressFamily.InterNetwork).ToArray(),
                DhcpLeaseObtained = dhcpLeaseObtained,
                DhcpLeaseExpires = dhcpLeaseExpires,
                IPv6ProtocolAvailable = ipv6ProtocolAvailable,
                IPv6AddressLinkLocal = listIPv6AddressLinkLocal.ToArray(),
                IPv6Address = listIPv6Address.ToArray(),
                IPv6Gateway = listIPv6Gateway.ToArray(),
                DNSAutoconfigurationEnabled = dnsAutoconfigurationEnabled,
                DNSSuffix = ipProperties.DnsSuffix,
                DNSServer = ipProperties.DnsAddresses.ToArray()
            });
        }

        return listNetworkInterfaceInfo;
    }

    public static Task<IPAddress> DetectLocalIPAddressBasedOnRoutingAsync(IPAddress remoteIPAddress)
    {
        return Task.Run(() => DetectLocalIPAddressBasedOnRouting(remoteIPAddress));
    }

    private static IPAddress DetectLocalIPAddressBasedOnRouting(IPAddress remoteIPAddress)
    {
        var isIPv4 = remoteIPAddress.AddressFamily == AddressFamily.InterNetwork;

        using var socket = new Socket(isIPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6,
            SocketType.Dgram, ProtocolType.Udp);

        // return null on error...
        try
        {
            socket.Bind(new IPEndPoint(isIPv4 ? IPAddress.Any : IPAddress.IPv6Any, 0));
            socket.Connect(new IPEndPoint(remoteIPAddress, 0));

            if (socket.LocalEndPoint is IPEndPoint ipAddress)
                return ipAddress.Address;
        }
        catch (SocketException)
        {
        }

        return null;
    }

    public static Task<IPAddress> DetectGatewayBasedOnLocalIPAddressAsync(IPAddress localIPAddress)
    {
        return Task.Run(() => DetectGatewayBasedOnLocalIPAddress(localIPAddress));
    }

    private static IPAddress DetectGatewayBasedOnLocalIPAddress(IPAddress localIPAddress)
    {
        foreach (var networkInterface in GetNetworkInterfaces())
            if (localIPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (networkInterface.IPv4Address.Any(x => x.Item1.Equals(localIPAddress)))
                    return networkInterface.IPv4Gateway.FirstOrDefault();
            }
            else if (localIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (networkInterface.IPv6Address.Contains(localIPAddress))
                    return networkInterface.IPv6Gateway.FirstOrDefault();
            }
            else
            {
                throw new Exception("IPv4 or IPv6 address is required to detect the gateway.");
            }

        return null;
    }

    public Task ConfigureNetworkInterfaceAsync(NetworkInterfaceConfig config)
    {
        return Task.Run(() => ConfigureNetworkInterface(config));
    }

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
}