using Heijden.DNS;
using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    public class DNSLookupOptions
    {
        public bool UseCustomDNSServer { get; set; }
        public List<string> CustomDNSServers { get; set; }
        public bool AddDNSSuffix { get; set; }
        public bool UseCustomDNSSuffix { get; set; }
        public string CustomDNSSuffix { get; set; }
        public QClass Class { get; set; }
        public QType Type { get; set; }
        public bool Recursion { get; set; }
        public bool UseResolverCache { get; set; }
        public TransportType TransportType { get; set; }
        public int Attempts { get; set; }
        public int Timeout { get; set;}
        public bool ResolveCNAME { get; set; }

        public DNSLookupOptions()
        {

        }
    }
}
