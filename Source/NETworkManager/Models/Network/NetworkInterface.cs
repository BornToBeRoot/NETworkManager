using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;

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
            List<NetworkInterfaceInfo> listNetworkInterfaceInfo = new List<NetworkInterfaceInfo>();

            foreach (System.Net.NetworkInformation.NetworkInterface networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    continue;

                List<IPAddress> listIPv4Address = new List<IPAddress>();
                List<IPAddress> listSubnetmask = new List<IPAddress>();
                List<IPAddress> listIPv6AddressLinkLocal = new List<IPAddress>();
                List<IPAddress> listIPv6Address = new List<IPAddress>();

                DateTime dhcpLeaseObtained = new DateTime();
                DateTime dhcpLeaseExpires = new DateTime();

                foreach (UnicastIPAddressInformation unicastIPAddrInfo in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        listIPv4Address.Add(unicastIPAddrInfo.Address);
                        listSubnetmask.Add(unicastIPAddrInfo.IPv4Mask);

                        dhcpLeaseExpires = (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressPreferredLifetime)).ToLocalTime();
                        dhcpLeaseObtained = (DateTime.UtcNow + TimeSpan.FromSeconds(unicastIPAddrInfo.AddressValidLifetime) - TimeSpan.FromSeconds(unicastIPAddrInfo.DhcpLeaseLifetime)).ToLocalTime();
                    }
                    else if (unicastIPAddrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        if (unicastIPAddrInfo.Address.IsIPv6LinkLocal)
                            listIPv6AddressLinkLocal.Add(unicastIPAddrInfo.Address);
                        else
                            listIPv6Address.Add(unicastIPAddrInfo.Address);
                    }
                }

                List<IPAddress> listIPv4Gateway = new List<IPAddress>();
                List<IPAddress> listIPv6Gateway = new List<IPAddress>();

                foreach (GatewayIPAddressInformation gatewayIPAddrInfo in networkInterface.GetIPProperties().GatewayAddresses)
                {
                    if (gatewayIPAddrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        listIPv4Gateway.Add(gatewayIPAddrInfo.Address);
                    else if (gatewayIPAddrInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        listIPv6Gateway.Add(gatewayIPAddrInfo.Address);
                }

                List<IPAddress> listDhcpServer = new List<IPAddress>();

                foreach (IPAddress dhcpServerIPAddress in networkInterface.GetIPProperties().DhcpServerAddresses)
                {
                    if (dhcpServerIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        listDhcpServer.Add(dhcpServerIPAddress);
                }

                // Check if autoconfiguration for DNS is enabled (only via registry key)
                RegistryKey nameServerKey = Registry.LocalMachine.OpenSubKey(String.Format(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{0}", networkInterface.Id));
                bool DNSAutoconfigurationEnabled = string.IsNullOrEmpty(nameServerKey.GetValue("NameServer").ToString());

                List<IPAddress> listDNSServer = new List<IPAddress>();

                foreach (IPAddress DNSServerIPAddress in networkInterface.GetIPProperties().DnsAddresses)
                {
                    listDNSServer.Add(DNSServerIPAddress);
                }

                listNetworkInterfaceInfo.Add(new NetworkInterfaceInfo
                {
                    Id = networkInterface.Id,
                    Name = networkInterface.Name,
                    Description = networkInterface.Description,
                    Type = networkInterface.NetworkInterfaceType.ToString(),
                    PhysicalAddress = networkInterface.GetPhysicalAddress(),
                    Status = networkInterface.OperationalStatus,
                    IsOperational = networkInterface.OperationalStatus == OperationalStatus.Up ? true : false,
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
                    DNSAutoconfigurationEnabled = DNSAutoconfigurationEnabled,
                    DNSSuffix = networkInterface.GetIPProperties().DnsSuffix,
                    DNSServer = listDNSServer.ToArray()
                });
            }

            return listNetworkInterfaceInfo;
        }

        public Task ConfigureNetworkInterfaceAsync(NetworkInterfaceConfig config)
        {
            return Task.Run(() => ConfigureNetworkInterface(config));
        }

        public void ConfigureNetworkInterface(NetworkInterfaceConfig config)
        {
            // IP
            string command = string.Format("netsh interface ipv4 set address name=\"{0}\" ", config.Name);
            command += config.EnableStaticIPAddress ? string.Format("source=static address={0} mask={1} gateway={2}", config.IPAddress, config.Subnetmask, config.Gateway) : "source=dhcp";

            // DNS
            command += string.Format(";netsh interface ipv4 set DNSservers name=\"{0}\" ", config.Name);
            command += config.EnableStaticDNS ? string.Format("source=static address={0} register=primary validate=no", config.PrimaryDNSServer) : "source=dhcp";
            command += (config.EnableStaticDNS && !string.IsNullOrEmpty(config.SecondaryDNSServer)) ? string.Format(";netsh interface ipv4 add DNSservers name=\"{0}\" address={1} index=2 validate=no", config.Name, config.SecondaryDNSServer) : "";

            // Start process with elevated rights...
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                Verb = "runas",
                FileName = "powershell.exe",
                Arguments = string.Format("-NoProfile -NoLogo -Command {0}", command)
            };
            
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;

                try
                {
                    process.Start();
                    process.WaitForExit();
                }
                catch (Win32Exception win32ex)
                {
                    switch (win32ex.NativeErrorCode)
                    {
                        case 1223:
                            OnUserHasCanceled();
                            break;
                        default:
                            throw;
                    }
                }
            }
        }
        #endregion
    }
}
