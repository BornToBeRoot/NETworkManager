using DnsClient;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NETworkManager.Utilities
{
    class DnsLookupHelper
    {
        private static LookupClient DnsLookupClient = new LookupClient();

        public async static Task<string> ResolveHostname(IPAddress ipAddress)
        {
            string hostname = string.Empty;

            try
            {
                var answer = await DnsLookupClient.GetHostNameAsync(ipAddress);

                if (!string.IsNullOrEmpty(answer))
                    hostname = answer;
            }
            catch { }

            return hostname;
        }

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
            catch { }

            return null;
        }
    }
}
