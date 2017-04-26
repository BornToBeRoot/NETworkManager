using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Model.Network
{
    public class NetworkInterfaceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public PhysicalAddress PhysicalAddress { get; set; }
        public string Status { get; set; }
        public long Speed { get; set; }
        public IPAddress[] IPv4Address { get; set; }
        public IPAddress[] Subnetmask { get; set; }
        public IPAddress[] IPv4Gateway { get; set; }
        public bool IsDhcpEnabled { get; set; }
        public IPAddress[] DhcpServer { get; set; }
        public DateTime DhcpLeaseObtained { get; set; }
        public DateTime DhcpLeaseExpires { get; set; }
        public IPAddress[] IPv6Address { get; set; }
        public IPAddress[] IPv6AddressLinkLocal { get; set; }
        public IPAddress[] IPv6Gateway { get; set; }
        public string DnsSuffix { get; set; }
        public IPAddress[] DnsServer { get; set; }

        public NetworkInterfaceInfo()
        {
                
        }

        public NetworkInterfaceInfo(string id, string name, string description, string type, PhysicalAddress physicalAddress, string status, long speed, IPAddress[] ipv4Address, IPAddress[] subnetmask, IPAddress[] ipv4Gateway, bool isDhcpEnabled, IPAddress[] dhcpServer, DateTime dhcpLeaseObtained, DateTime dhcpLeaseExpires, IPAddress[] ipv6AddressLinkLocal, IPAddress[] ipv6Address, IPAddress[] ipv6Gateway, string dnsSuffix, IPAddress[] dnsServer)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            PhysicalAddress = physicalAddress;
            Status = status;
            Speed = Speed;
            IPv4Address = ipv4Address;
            Subnetmask = subnetmask;
            IPv4Gateway = ipv4Gateway;
            IsDhcpEnabled = isDhcpEnabled;
            DhcpServer = dhcpServer;
            DhcpLeaseObtained = dhcpLeaseObtained;
            DhcpLeaseExpires = dhcpLeaseExpires;
            IPv6AddressLinkLocal = ipv6AddressLinkLocal;
            IPv6Address = ipv6Address;            
            IPv6Gateway = ipv6Gateway;
            DnsSuffix = dnsSuffix;
            DnsServer = dnsServer;
        }
    }
}
