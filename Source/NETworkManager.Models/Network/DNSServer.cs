using System;
using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class provides static informations about dns servers.
    /// </summary>
    public static class DNSServer
    {
        /// <summary>
        /// Method will return a default list of common dns servers.
        /// </summary>
        /// <returns>List of common dns servers.</returns>
        public static List<DNSServerInfo> DefaultList()
        {
            return new List<DNSServerInfo>
            {
                // Windows DNS server
                new DNSServerInfo() { UseWindowsDNSServer = true },

                // Classic DNS servers
                new DNSServerInfo("Cloudflare", new List<DNSServerClassicInfo>{
                    new DNSServerClassicInfo("1.1.1.1", 53),
                    new DNSServerClassicInfo("1.0.0.1", 53)
                }),
                new DNSServerInfo("DNS.Watch", new List<DNSServerClassicInfo>{
                    new DNSServerClassicInfo("84.200.69.80", 53),
                    new DNSServerClassicInfo("84.200.70.40", 53)
                }),
                new DNSServerInfo("Google Public DNS", new List<DNSServerClassicInfo>{
                    new DNSServerClassicInfo("8.8.8.8", 53),
                    new DNSServerClassicInfo("8.8.4.4", 53)
                }),
                new DNSServerInfo("Level3", new List<DNSServerClassicInfo>{
                    new DNSServerClassicInfo("209.244.0.3", 53),
                    new DNSServerClassicInfo("209.244.0.4", 53)
                }),
                new DNSServerInfo("Verisign", new List<DNSServerClassicInfo>{
                    new DNSServerClassicInfo("64.6.64.6", 53),
                    new DNSServerClassicInfo("64.6.65.6", 53)
                }),

                // DoH servers
                new DNSServerInfo("Cloudflare [DoH]", new List<DNSServerDoHInfo>
                {
                    new DNSServerDoHInfo("https://cloudflare-dns.com/dns-query"),
                }),
                new DNSServerInfo("DNS.Watch [DoH]", new List<DNSServerDoHInfo>
                {
                   new DNSServerDoHInfo( "https://resolver2.dns.watch/dns-query"),
                })
            };
        }
    }
}
