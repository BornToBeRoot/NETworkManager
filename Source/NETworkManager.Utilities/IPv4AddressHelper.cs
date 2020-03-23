using System.Net;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides static methods for testing IPv4 addresses.
    /// </summary>
    class IPv4AddressHelper
    {
        /// <summary>
        /// First possible IPv4 multicast address.
        /// </summary>
        private const int IPv4MulticastStart = -536870912;

        /// <summary>
        /// Last possible IPv4 multicast address.
        /// </summary>
        private const int IPv4MulticastEnd = -268435457;
        
        /// <summary>
        /// Method to check if an IPv4 address is a multicast address.
        /// </summary>
        /// <param name="ipAddress">IPv4 address as <see cref="IPAddress"/>.</param>
        /// <returns>True if it is a multicast address. False if not.</returns>
        public static bool IsMulticast(IPAddress ipAddress)
        {
            var ip = IPv4AddressConverter.ToInt32(ipAddress);

            return (ip >= IPv4MulticastStart && ip <= IPv4MulticastEnd);
        }
    }
}
