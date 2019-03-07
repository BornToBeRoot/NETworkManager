using Microsoft.Win32;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class NetworkInterface
    {
        #region Events
        public event EventHandler UserHasCanceled;

        protected virtual void OnUserHasCanceled()
        {
            UserHasCanceled?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public static Task<List<NetworkInterfaceInfo>> GetNetworkInterfacesAsync()
        {
            return Task.Run(() => GetNetworkInterfaces());
        }

        public static List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var listNetworkInterfaceInfo = new List<NetworkInterfaceInfo>();

            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    continue;

                var listIPv4Address = new List<IPAddress>();
                var listSubnetmask = new List<IPAddress>();
                var listIPv6AddressLinkLocal = new List<IPAddress>();
                var listIPv6Address = new List<IPAddress>();

                var dhcpLeaseObtained = new DateTime();
                var dhcpLeaseExpires = new DateTime();

                foreach (var unicastIPAddrInfo in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    switch (unicastIPAddrInfo.Address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            listIPv4Address.Add(unicastIPAddrInfo.Address);
                            listSubnetmask.Add(unicastIPAddrInfo.IPv4Mask);
                            dhcpLeaseExpires = (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressPreferredLifetime)).ToLocalTime();
                            dhcpLeaseObtained = (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressValidLifetime) - TimeSpan.FromSeconds(unicastIPAddrInfo.DhcpLeaseLifetime)).ToLocalTime();
                            break;
                        case System.Net.Sockets.AddressFamily.InterNetworkV6 when unicastIPAddrInfo.Address.IsIPv6LinkLocal:
                            listIPv6AddressLinkLocal.Add(unicastIPAddrInfo.Address);
                            break;
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            listIPv6Address.Add(unicastIPAddrInfo.Address);
                            break;
                    }
                }

                var listIPv4Gateway = new List<IPAddress>();
                var listIPv6Gateway = new List<IPAddress>();

                foreach (var gatewayIPAddrInfo in networkInterface.GetIPProperties().GatewayAddresses)
                {
                    switch (gatewayIPAddrInfo.Address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            listIPv4Gateway.Add(gatewayIPAddrInfo.Address);
                            break;
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            listIPv6Gateway.Add(gatewayIPAddrInfo.Address);
                            break;
                    }
                }

                var listDhcpServer = new List<IPAddress>();

                foreach (var dhcpServerIPAddress in networkInterface.GetIPProperties().DhcpServerAddresses)
                {
                    if (dhcpServerIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        listDhcpServer.Add(dhcpServerIPAddress);
                }

                // Check if autoconfiguration for DNS is enabled (only via registry key)
                var nameServerKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{networkInterface.Id}");
                var dnsAutoconfigurationEnabled = nameServerKey?.GetValue("NameServer") != null && string.IsNullOrEmpty(nameServerKey.GetValue("NameServer").ToString());

                var listDNSServer = new List<IPAddress>();

                foreach (var dnsServerIPAddress in networkInterface.GetIPProperties().DnsAddresses)
                {
                    listDNSServer.Add(dnsServerIPAddress);
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
                    IPv4Address = listIPv4Address.ToArray(),
                    Subnetmask = listSubnetmask.ToArray(),
                    IPv4Gateway = listIPv4Gateway.ToArray(),
                    DhcpEnabled = networkInterface.GetIPProperties().GetIPv4Properties().IsDhcpEnabled,
                    DhcpServer = listDhcpServer.ToArray(),
                    DhcpLeaseObtained = dhcpLeaseObtained,
                    DhcpLeaseExpires = dhcpLeaseExpires,
                    IPv6AddressLinkLocal = listIPv6AddressLinkLocal.ToArray(),
                    IPv6Address = listIPv6Address.ToArray(),
                    IPv6Gateway = listIPv6Gateway.ToArray(),
                    DNSAutoconfigurationEnabled = dnsAutoconfigurationEnabled,
                    DNSSuffix = networkInterface.GetIPProperties().DnsSuffix,
                    DNSServer = listDNSServer.ToArray()
                });
            }

            return listNetworkInterfaceInfo;
        }

        public static Task<IPAddress> DetectLocalIPAddressBasedOnRoutingAsync(IPAddress remoteIPAddress)
        {
            return Task.Run(() => DetectLocalIPAddressBasedOnRouting(remoteIPAddress));
        }

        public static IPAddress DetectLocalIPAddressBasedOnRouting(IPAddress remoteIPAddress)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                socket.Connect(new IPEndPoint(remoteIPAddress, 0));

                if (socket.LocalEndPoint is IPEndPoint ipAddress)
                    return ipAddress.Address;
            }

            return null;
        }

        public static Task<IPAddress> DetectGatewayBasedOnLocalIPAddressAsync(IPAddress localIPAddress)
        {
            return Task.Run(() => DetectGatewayBasedOnLocalIPAddress(localIPAddress));
        }

        public static IPAddress DetectGatewayBasedOnLocalIPAddress(IPAddress localIPAddress)
        {
            foreach (var networkInterface in GetNetworkInterfaces())
            {
                if (networkInterface.IPv4Address.Contains(localIPAddress))
                {
                    return networkInterface.IPv4Gateway.FirstOrDefault();
                }

                if (networkInterface.IPv6Address.Contains(localIPAddress))
                {
                    return networkInterface.IPv4Gateway.FirstOrDefault();
                }
            }

            return null;
        }

        public Task ConfigureNetworkInterfaceAsync(NetworkInterfaceConfig config)
        {
            return Task.Run(() => ConfigureNetworkInterface(config));
        }

        public void ConfigureNetworkInterface(NetworkInterfaceConfig config)
        {
            // IP
            var command = @"netsh interface ipv4 set address name='" + config.Name + @"'";
            command += config.EnableStaticIPAddress ? @" source=static address=" + config.IPAddress + @" mask=" + config.Subnetmask + @" gateway=" + config.Gateway : @" source=dhcp";

            // DNS
            command += @";netsh interface ipv4 set DNSservers name='" + config.Name + @"'";
            command += config.EnableStaticDNS ? @" source=static address=" + config.PrimaryDNSServer + @" register=primary validate=no" : @" source=dhcp";
            command += (config.EnableStaticDNS && !string.IsNullOrEmpty(config.SecondaryDNSServer)) ? @";netsh interface ipv4 add DNSservers name='" + config.Name + @"' address=" + config.SecondaryDNSServer + @" index=2 validate=no" : "";

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

        public static Task FlushDnsAsync()
        {
            return Task.Run(() => FlushDns());
        }

        public static void FlushDns()
        {
            const string command = @"ipconfig /flushdns";

            PowerShellHelper.ExecuteCommand(command);
        }

        public static Task ReleaseRenewAsync(IPConfigReleaseRenewMode mode)
        {
            return Task.Run(() => ReleaseRenew(mode));
        }

        public static void ReleaseRenew(IPConfigReleaseRenewMode mode)
        {
            if (mode == IPConfigReleaseRenewMode.ReleaseRenew || mode == IPConfigReleaseRenewMode.Release)
            {
                const string command = @"ipconfig /release";

                PowerShellHelper.ExecuteCommand(command);
            }

            if (mode == IPConfigReleaseRenewMode.ReleaseRenew || mode == IPConfigReleaseRenewMode.Renew)
            {
                const string command = @"ipconfig /renew";

                PowerShellHelper.ExecuteCommand(command);
            }
        }

        public static Task AddIPAddressToNetworkInterfaceAsync(NetworkInterfaceConfig config)
        {
            return Task.Run(() => AddIPAddressToNetworkInterface(config));
        }

        public static void AddIPAddressToNetworkInterface(NetworkInterfaceConfig config)
        {
            var command = @"netsh interface ipv4 add address '" + config.Name + @"' " + config.IPAddress + @" " + config.Subnetmask;

            PowerShellHelper.ExecuteCommand(command, true);
        }
        #endregion

        #region Enum
        public enum IPConfigReleaseRenewMode
        {
            ReleaseRenew,
            Release,
            Renew
        }
        #endregion
    }
}
