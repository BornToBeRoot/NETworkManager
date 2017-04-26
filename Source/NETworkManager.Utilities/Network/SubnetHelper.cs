using System.Net;

namespace NETworkManager.Utilities.Network
{
    public static class SubnetHelper
    {
        // Source: https://blogs.msdn.microsoft.com/knom/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks/
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
        public static IPAddress GetIPv4Broadcast(IPAddress ipv4Address, IPAddress subnetmask)
        {
            byte[] ipv4AddressBytes = ipv4Address.GetAddressBytes();
            byte[] subnetmaskBytes = subnetmask.GetAddressBytes();

            byte[] broadcastBytes = new byte[ipv4AddressBytes.Length];

            for (int i = 0; i < broadcastBytes.Length; i++)
                broadcastBytes[i] = (byte)(ipv4AddressBytes[i] | (subnetmaskBytes[i] ^ 255));

            return new IPAddress(broadcastBytes);
        }
    }
}
