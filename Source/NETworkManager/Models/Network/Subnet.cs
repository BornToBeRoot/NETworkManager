using NETworkManager.Utilities;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Subnet
    {
        #region Static methods
        // Source: https://blogs.msdn.microsoft.com/knom/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks/
        [Obsolete("Use IPNetwork for calculation...")]
        public static IPAddress GetIPv4NetworkAddress(IPAddress ipv4Address, IPAddress subnetmask)
        {
            byte[] ipv4AdressBytes = ipv4Address.GetAddressBytes();
            byte[] subnetmaskBytes = subnetmask.GetAddressBytes();

            byte[] broadcastBytes = new byte[ipv4AdressBytes.Length];

            for (int i = 0; i < broadcastBytes.Length; i++)
                broadcastBytes[i] = (byte)(ipv4AdressBytes[i] & (subnetmaskBytes[i]));

            return new IPAddress(broadcastBytes);
        }

        // Source: https://blogs.msdn.microsoft.com/knom/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks/
        [Obsolete("Use IPNetwork for calculation...")]
        public static IPAddress GetIPv4Broadcast(IPAddress ipv4Address, IPAddress subnetmask)
        {
            byte[] ipv4AddressBytes = ipv4Address.GetAddressBytes();
            byte[] subnetmaskBytes = subnetmask.GetAddressBytes();

            byte[] broadcastBytes = new byte[ipv4AddressBytes.Length];

            for (int i = 0; i < broadcastBytes.Length; i++)
                broadcastBytes[i] = (byte)(ipv4AddressBytes[i] | (subnetmaskBytes[i] ^ 255));

            return new IPAddress(broadcastBytes);
        }

        [Obsolete("Use IPNetwork for calculation...")]
        public static SubnetInfo CalculateIPv4Subnet(IPAddress ipv4Address, IPAddress subnetmask)
        {
            IPAddress networkAddress = GetIPv4NetworkAddress(ipv4Address, subnetmask);
            IPAddress broadcast = GetIPv4Broadcast(ipv4Address, subnetmask);
            int cidr = Subnetmask.ConvertSubnetmaskToCidr(subnetmask);
            long totalIPs = Subnetmask.GetNumberIPv4Addresses(cidr);

            // Fix bug when /31 (host first/last can be null)
            IPAddress hostFirstIP = null;
            IPAddress hostLastIP = null;
            long hostIPs = 0;

            if (totalIPs == 1) // Fix bug when /32 (show range for 1 IP)
            {
                hostFirstIP = networkAddress;
                hostLastIP = broadcast;
                hostIPs = 0;
            }
            else if (totalIPs > 2) // Calculate for /0-/30
            {
                hostFirstIP = IPv4AddressHelper.IncrementIPv4Address(networkAddress, 1);
                hostLastIP = IPv4AddressHelper.DecrementIPv4Address(broadcast, 1);
                hostIPs = totalIPs - 2;
            }

            return new SubnetInfo
            {
                NetworkAddress = networkAddress,
                Broadcast = broadcast,
                IPAddresses = totalIPs,
                Subnetmask = subnetmask,
                CIDR = cidr,
                HostFirstIP = hostFirstIP,
                HostLastIP = hostLastIP,
                Hosts = hostIPs
            };
        }
        #endregion

        #region Methods
        [Obsolete("Use IPNetwork for calculation...")]
        public static Task<SubnetInfo[]> SplitIPv4SubnetAsync(IPAddress ipv4Address, IPAddress subnetmask, IPAddress newSubnetmask, CancellationToken cancellationToken)
        {
            return Task.Run(() => SplitIPv4Subnet(ipv4Address, subnetmask, newSubnetmask, cancellationToken), cancellationToken);
        }

        [Obsolete("Use IPNetwork for calculation...")]
        public static SubnetInfo[] SplitIPv4Subnet(IPAddress ipv4Address, IPAddress subnetmask, IPAddress newSubnetmask, CancellationToken cancellationToken)
        {
            ConcurrentBag<SubnetInfo> bag = new ConcurrentBag<SubnetInfo>();

            // Calculate the current subnet
            SubnetInfo subnetInfo = CalculateIPv4Subnet(ipv4Address, subnetmask);

            int newCidr = Subnetmask.ConvertSubnetmaskToCidr(newSubnetmask);

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

            Parallel.For(0, subnetInfo.IPAddresses / newTotalIPs, new ParallelOptions() { CancellationToken = cancellationToken }, i =>
            {
                byte[] newNetworkAddressBytes = BitConverter.GetBytes(networkAddress + (int)(newTotalIPs * i));

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(newNetworkAddressBytes);

                SubnetInfo info = CalculateIPv4Subnet(new IPAddress(newNetworkAddressBytes), newSubnetmask);

                bag.Add(new SubnetInfo(info.NetworkAddress, info.Broadcast, info.IPAddresses, info.Subnetmask, info.CIDR, info.HostFirstIP, info.HostLastIP, info.Hosts));
            });

            return bag.ToArray();
        }
        #endregion
    }
}
