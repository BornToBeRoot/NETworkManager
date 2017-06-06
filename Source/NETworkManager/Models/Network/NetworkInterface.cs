using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;


namespace NETworkManager.Models.Network
{
    public class NetworkInterface
    {
        #region Events
        public event EventHandler<ProgressChangedArgs> ConfigureProgressChanged;

        protected virtual void OnConfigureProgressChanged(ProgressChangedArgs e)
        {
            ConfigureProgressChanged?.Invoke(this, e);
        }
        #endregion

        #region Public methods
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

                List<IPAddress> listDnsServer = new List<IPAddress>();

                foreach (IPAddress dnsServerIPAddress in networkInterface.GetIPProperties().DnsAddresses)
                {
                    listDnsServer.Add(dnsServerIPAddress);
                }

                listNetworkInterfaceInfo.Add(new NetworkInterfaceInfo
                {
                    Id = networkInterface.Id,
                    Name = networkInterface.Name,
                    Description = networkInterface.Description,
                    Type = networkInterface.NetworkInterfaceType.ToString(),
                    PhysicalAddress = networkInterface.GetPhysicalAddress(),
                    Status = networkInterface.OperationalStatus.ToString(),
                    Speed = networkInterface.Speed,
                    IPv4Address = listIPv4Address.ToArray(),
                    Subnetmask = listSubnetmask.ToArray(),
                    IPv4Gateway = listIPv4Gateway.ToArray(),
                    IsDhcpEnabled = networkInterface.GetIPProperties().GetIPv4Properties().IsDhcpEnabled,
                    DhcpServer = listDhcpServer.ToArray(),
                    DhcpLeaseObtained = dhcpLeaseObtained,
                    DhcpLeaseExpires = dhcpLeaseExpires,
                    IPv6AddressLinkLocal = listIPv6AddressLinkLocal.ToArray(),
                    IPv6Address = listIPv6Address.ToArray(),
                    IPv6Gateway = listIPv6Gateway.ToArray(),
                    DnsSuffix = networkInterface.GetIPProperties().DnsSuffix,
                    DnsServer = listDnsServer.ToArray()
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
            if (config.EnableStaticIPAddress)
            {
                OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 1 });
                SetStaticIPAddress(config.Id, config.IPAddress, config.Subnetmask, config.Gateway);

                if (config.EnableStaticDns)
                {
                    OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 3 });
                    SetStaticDNSServer(config.Id, config.PrimaryDnsServer, config.SecondaryDnsServer);
                }
                else
                {
                    OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 4 });
                    SetDynamicDNSServer(config.Id);
                }
            }
            else
            {
                OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 2 });
                SetDynamicIPAddress(config.Id);

                if (config.EnableStaticDns)
                {
                    OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 3 });
                    SetStaticDNSServer(config.Id, config.PrimaryDnsServer, config.SecondaryDnsServer);
                }

                else
                {
                    OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 4 });
                    SetDynamicDNSServer(config.Id);
                }

                // Renew dhcp release
                OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 5 });
                RenewDhcpLease(config.Id);

                // Fix gateway
                OnConfigureProgressChanged(new ProgressChangedArgs() { Value = 6 });
                FixGatewayAfterDHCPEnabled(config.Id);
            }
        }
        #endregion

        #region Private methods
        private void SetStaticIPAddress(string id, string ipAddress, string subnetmask, string gateway)
        {
            foreach (ManagementObject adapter in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if (adapter["SettingID"] as string == id)
                {
                    ManagementBaseObject newIPAddress = adapter.GetMethodParameters("EnableStatic");

                    newIPAddress["IPAddress"] = new string[] { ipAddress };
                    newIPAddress["SubnetMask"] = new string[] { subnetmask };

                    adapter.InvokeMethod("EnableStatic", newIPAddress, null);

                    ManagementBaseObject newGateway = adapter.GetMethodParameters("SetGateways");

                    newGateway["DefaultIPGateway"] = new string[] { gateway };
                    newGateway["GatewayCostMetric"] = new int[] { 1 };

                    adapter.InvokeMethod("SetGateways", newGateway, null);
                }
            }
        }

        private void SetDynamicIPAddress(string id)
        {
            foreach (ManagementObject adapter in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if (adapter["SettingID"] as string == id)
                    adapter.InvokeMethod("EnableDHCP", null, null);
            }
        }

        private void SetStaticDNSServer(string id, string primaryServer, string secondaryServer)
        {
            foreach (ManagementObject adapter in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if (adapter["SettingID"] as string == id)
                {
                    ManagementBaseObject newDNSServer = adapter.GetMethodParameters("SetDNSServerSearchOrder");

                    newDNSServer["DNSServerSearchOrder"] = new string[] { primaryServer, secondaryServer };

                    adapter.InvokeMethod("SetDNSServerSearchOrder", newDNSServer, null);
                }
            }
        }

        private void SetDynamicDNSServer(string id)
        {
            foreach (ManagementObject adapter in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if (adapter["SettingID"] as string == id)
                {
                    ManagementBaseObject newDNSServer = adapter.GetMethodParameters("SetDNSServerSearchOrder");

                    newDNSServer["DNSServerSearchOrder"] = null;

                    adapter.InvokeMethod("SetDNSServerSearchOrder", newDNSServer, null);
                }
            }
        }

        private void RenewDhcpLease(string id)
        {
            foreach (ManagementObject adapter in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if (adapter["SettingID"] as string == id)
                {
                    adapter.InvokeMethod("RenewDHCPLease", null, null);
                }
            }
        }

        private void FixGatewayAfterDHCPEnabled(string id)
        {
            foreach (ManagementObject adapter in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if (adapter["SettingID"] as string == id)
                {
                    // Bugfix - Based on https://www.codeproject.com/Questions/58393/How-to-clear-TCP-IP-Gateway-using-WMI-in-Vista-Ser
                    string[] gateways = (string[])adapter["DefaultIPGateway"];

                    string gateway = string.Empty;

                    foreach (string gw in gateways)
                    {
                        if (IPAddress.Parse(gw).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            gateway = gw;
                    }

                    ManagementBaseObject newIP = adapter.GetMethodParameters("EnableStatic");

                    adapter.InvokeMethod("EnableStatic", newIP, null);

                    ManagementBaseObject newGateway = adapter.GetMethodParameters("SetGateways");

                    newGateway["DefaultIPGateway"] = new string[] { gateway };
                    newGateway["GatewayCostMetric"] = new int[] { 1 };

                    adapter.InvokeMethod("SetGateways", newGateway, null);

                    adapter.InvokeMethod("EnableDHCP", null, null);
                }
            }
        }
        #endregion
    }
}
