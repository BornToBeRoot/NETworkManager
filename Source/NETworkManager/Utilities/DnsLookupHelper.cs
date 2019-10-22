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

        /// <summary>
        /// Resolve hostname and ip address, if only an ip or a hostname was passed
        /// </summary>
        /// <param name="hostname">example.com</param>
        /// <param name="preferIPv4">true</param>
        /// <returns>Item1: Hostname, Item2: IPAddress</returns>
        public async static Task<Tuple<string, IPAddress>> ResolveHost(string hostname, bool preferIPv4 = true)
        {
            // Try to parse the string into an IP-Address
            var hostIsIP = IPAddress.TryParse(hostname, out var ipAddress);

            if (!hostIsIP) // Lookup
            {
                try
                {
                    // Append dns suffix to hostname
                    if (hostname.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        var dnsSuffix = IPGlobalProperties.GetIPGlobalProperties().DomainName;

                        if (!string.IsNullOrEmpty(dnsSuffix))
                            hostname += $".{dnsSuffix}";
                    }

                    // Try to resolve the hostname
                    var ipHostEntrys = await DnsLookupClient.GetHostEntryAsync(hostname);

                    if (ipHostEntrys.AddressList.Length == 0)
                        return new Tuple<string, IPAddress>(hostname, null);

                    foreach (var ip in ipHostEntrys.AddressList)
                    {
                        switch (ip.AddressFamily)
                        {
                            case AddressFamily.InterNetwork when preferIPv4:
                                ipAddress = ip;
                                break;
                            // ToDo: Setting
                            case AddressFamily.InterNetworkV6 when preferIPv4:
                                ipAddress = ip;
                                break;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol for the hostname
                    foreach (var ip in ipHostEntrys.AddressList)
                    {
                        ipAddress = ip;
                        break;
                    }
                }
                catch // This will catch DNS resolve errors
                {
                    return new Tuple<string, IPAddress>(hostname, null);
                }
            }
            else // Reverse lookup
            {
                try
                {
                    var answer = await DnsLookupClient.GetHostNameAsync(ipAddress);

                    if (!string.IsNullOrEmpty(answer))
                        hostname = answer;
                }
                catch
                {

                }
            }

            return new Tuple<string, IPAddress>(hostname, ipAddress);
        }
    }
}
