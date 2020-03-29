using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    public static class DNSServer
    {
        public static List<DNSServerInfo> DefaultList()
        {
            return new List<DNSServerInfo>
            {
                new DNSServerInfo { UseWindowsDNSServer = true },
                new DNSServerInfo("Cloudflare", new List<string>{"1.1.1.1", "1.0.0.1"}),
                new DNSServerInfo("DNS.Watch", new List<string>{"84.200.69.80", "84.200.70.40"}),
                new DNSServerInfo("Google Public DNS", new List<string>{"8.8.8.8", "8.8.4.4"}),
                new DNSServerInfo("Level3", new List<string> {"209.244.0.3", "209.244.0.4"}),
                new DNSServerInfo("Verisign", new List<string> {"64.6.64.6", "64.6.65.6"})
            };
        }
    }
}