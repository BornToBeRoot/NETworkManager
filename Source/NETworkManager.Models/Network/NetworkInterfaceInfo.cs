using System;
using System.Diagnostics.CodeAnalysis;
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
        public bool IPv4ProtocolAvailable { get; set; }
        public Tuple<IPAddress, IPAddress>[] IPv4Address {get;set;}
        public IPAddress[] IPv4Gateway { get; set; }
        public bool DhcpEnabled { get; set; }
        public IPAddress[] DhcpServer { get; set; }
        public DateTime DhcpLeaseObtained { get; set; }
        public DateTime DhcpLeaseExpires { get; set; }
        public bool IPv6ProtocolAvailable { get; set; }
        public IPAddress[] IPv6Address { get; set; }
        public IPAddress[] IPv6AddressLinkLocal { get; set; }
        public IPAddress[] IPv6Gateway { get; set; }
        public bool DNSAutoconfigurationEnabled { get; set; }
        public string DNSSuffix { get; set; }
        public IPAddress[] DNSServer { get; set; }

        public NetworkInterfaceInfo()
        {
                
        }
    }
}
