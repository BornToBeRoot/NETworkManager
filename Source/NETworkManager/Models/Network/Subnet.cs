using NETworkManager.Helpers;
using System;
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

        public static List<SubnetInfo> SplitIPv4Subnet(IPAddress ipv4Address, IPAddress subnetmask, IPAddress newSubnetmask)
        {
            List<SubnetInfo> list = new List<SubnetInfo>();

            // Calculate the current subnet
            SubnetInfo subnetInfo = CalculateIPv4Subnet(ipv4Address, subnetmask);

            int newCidr = SubnetmaskHelper.ConvertSubnetmaskToCidr(newSubnetmask);

            // Get new  HostBits based on SubnetBits (CIDR) // Hostbits (32 - /24 = 8 -> 00000000000000000000000011111111)
            string newHostBits = (new string('1', (32 - newCidr))).PadLeft(32, '0');

            // Convert Bits to Int64, add +1 to get all available IPs
            int newTotalIPs = Convert.ToInt32(newHostBits, 2) + 1;

            // Get bytes...
            byte[] networkAddressBytes = subnetInfo.NetworkAddress.GetAddressBytes();
            
            // Get int...
            if (BitConverter.IsLittleEndian)
                Array.Reverse(networkAddressBytes);

            int networkAddress = BitConverter.ToInt32(networkAddressBytes, 0);

            // Calculate each new subnet...
            for (int i = 0; i < subnetInfo.TotalIPs; i += newTotalIPs)
            {
                byte[] newNetworkAddressBytes = BitConverter.GetBytes(networkAddress + i);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(newNetworkAddressBytes);

                list.Add(CalculateIPv4Subnet(new IPAddress(newNetworkAddressBytes), newSubnetmask));
            }

            return list;
        }
    }
}
