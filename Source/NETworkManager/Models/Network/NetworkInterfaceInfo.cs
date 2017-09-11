using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class NetworkInterfaceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public PhysicalAddress PhysicalAddress { get; set; }
        public OperationalStatus Status { get; set; }
        public bool IsOperational { get; set; }
        public long Speed { get; set; }
        public IPAddress[] IPv4Address { get; set; }
        public IPAddress[] Subnetmask { get; set; }
        public IPAddress[] IPv4Gateway { get; set; }
        public bool DhcpEnabled { get; set; }
        public IPAddress[] DhcpServer { get; set; }
        public DateTime DhcpLeaseObtained { get; set; }
        public DateTime DhcpLeaseExpires { get; set; }
        public IPAddress[] IPv6Address { get; set; }
        public IPAddress[] IPv6AddressLinkLocal { get; set; }
        public IPAddress[] IPv6Gateway { get; set; }
        public bool DnsAutoconfigurationEnabled { get; set; }
        public string DnsSuffix { get; set; }
        public IPAddress[] DnsServer { get; set; }

        public NetworkInterfaceInfo()
        {
                
        }
    }
}
