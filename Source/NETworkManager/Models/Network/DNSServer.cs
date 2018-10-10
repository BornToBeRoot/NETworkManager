using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    public static class DNSServer
    {
        public static List<DNSServerInfo> DefaultDNSServerList()
        {
            return new List<DNSServerInfo>
            {
                new DNSServerInfo { UseWindowsDNSServer = true },
                new DNSServerInfo("Google Public DNS", new List<string>{"8.8.8.8", "8.8.4.4"}),
                new DNSServerInfo("Cloudflare", new List<string>{"1.1.1.1", "1.0.0.1"}),
                new DNSServerInfo("DNS.Watch", new List<string>{"84.200.69.80", "84.200.70.40"})
            };
        }
    }
}