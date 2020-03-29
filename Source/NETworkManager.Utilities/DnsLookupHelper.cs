using DnsClient;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides static helper methods for dns lookup.
    /// </summary>
    public static class DnsLookupHelper
    {

        /// <summary>
        /// Variable holds a dns lookup client, which is initialized with the class.
        /// </summary>
        private static readonly LookupClient DnsLookupClient = new LookupClient();

        /// <summary>
        /// Task to asynchronously resolve an hostname from <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="ipAddress"><see cref="IPAddress"/> to resolve.</param>
        /// <returns>Resolved hostname.</returns>
        public async static Task<string> ResolveHostname(IPAddress ipAddress)
        {
            string hostname = string.Empty;

            try
            {
                var answer = await DnsLookupClient.GetHostNameAsync(ipAddress);

                if (!string.IsNullOrEmpty(answer))
                    hostname = answer;
            }

#pragma warning disable CA1031 // Empty string will be returned on error.
            catch { }
#pragma warning restore CA1031 

            return hostname;
        }

        /// <summary>
        /// Task to asynchronously resolve an ip address from hostname.
        /// </summary>
        /// <param name="hostname">Hostname to resolve.</param>
        /// <param name="preferIPv4">Prefer IPv4 address.</param>
        /// <returns>Resovled <see cref="IPAddress"/>.</returns>
        public async static Task<IPAddress> ResolveIPAddress(string hostname, bool preferIPv4 = true)
        {
            // Append dns suffix to hostname
            if (hostname.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
            {
                var dnsSuffix = IPGlobalProperties.GetIPGlobalProperties().DomainName;

                if (!string.IsNullOrEmpty(dnsSuffix))
                    hostname += $".{dnsSuffix}";
            }

            try
            {
                // Try to resolve the hostname
                var ipHostEntrys = await DnsLookupClient.GetHostEntryAsync(hostname);

                if (ipHostEntrys.AddressList.Length != 0)
                {
                    foreach (var ip in ipHostEntrys.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && preferIPv4)
                            return ip;

                        if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !preferIPv4)
                            return ip;
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    foreach (var ip in ipHostEntrys.AddressList)
                    {
                        return ip;
                    }
                }
            }
#pragma warning disable CA1031 // Null will be returned on error.
            catch { }
#pragma warning restore CA1031

            return null;
        }
    }
}
