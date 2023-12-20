using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// IP address comparer.
    /// </summary>
    public class IPAddressComparer : IComparer<IPAddress>
    {
        /// <summary>
        /// Compare two IP addresses.
        /// </summary>
        /// <param name="x">First IP address.</param>
        /// <param name="y">Second IP address.</param>
        /// <returns>0 if the IP addresses are equal, otherwise a negative or positive value.</returns>
        public int Compare(IPAddress x, IPAddress y)
        {
            return IPAddressHelper.CompareIPAddresses(x, y);
        }
    }
}
