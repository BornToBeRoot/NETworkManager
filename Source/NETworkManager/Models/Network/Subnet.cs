using NETworkManager.Helpers;
using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Models.Network
{
    public static class Subnet
    {
        public static SubnetInfo CalculateIPv4Subnet(IPAddress ipv4Address, IPAddress subnetmask)
        {
            IPAddress networkAddress = SubnetHelper.GetIPv4NetworkAddress(ipv4Address, subnetmask);
            IPAddress broadcast = SubnetHelper.GetIPv4Broadcast(ipv4Address, subnetmask);
            int cidr = SubnetmaskHelper.ConvertSubnetmaskToCidr(subnetmask);
            int totalIPs = SubnetmaskHelper.GetNumberIPv4Addresses(cidr);

            return new SubnetInfo
            {
                NetworkAddress = networkAddress,
                Broadcast = broadcast,
                TotalIPs = totalIPs,
                Subnetmask = subnetmask,
                CIDR = cidr,
                HostFirstIP = IPv4AddressHelper.IncrementIPv4Address(networkAddress, 1),
                HostLastIP = IPv4AddressHelper.DecrementIPv4Address(broadcast, 1),
                HostIPs = totalIPs - 2
            };
        }

        public static List<SubnetInfo> SplitIPv4Subnet(IPAddress ipv45Address, IPAddress subnetmask, IPAddress newSubnetmask)
        {
            List<SubnetInfo> list = new List<SubnetInfo>();



            return list;
        }
    }
}
