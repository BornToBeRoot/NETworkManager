using DnsClient;
using System;
using System.Net;
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
        /// <param name="host">example.com</param>
        /// <param name="preferIPv4">true</param>
        /// <returns></returns>
        public async static Task<Tuple<string, IPAddress>> ResolveHost(string host, bool preferIPv4 = true)
        {
            // Try to parse the string into an IP-Address
            var hostIsIP = IPAddress.TryParse(host, out var ipAddress);

            if (!hostIsIP) // Lookup
            {
                try
                {

                    // Try to resolve the hostname
                    var ipHostEntrys = await DnsLookupClient.GetHostEntryAsync(host);

                    if (ipHostEntrys.AddressList.Length == 0)
                        return new Tuple<string, IPAddress>(host, null);

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
                    return new Tuple<string, IPAddress>(host, null);
                }
            }
            else // Reverse lookup
            {
                try
                {
                    var hostname = await DnsLookupClient.GetHostNameAsync(ipAddress);

                    if (!string.IsNullOrEmpty(hostname))
                        host = hostname;
                }
                catch
                {

                }
            }

            return new Tuple<string, IPAddress>(host, ipAddress);
        }
    }
}
