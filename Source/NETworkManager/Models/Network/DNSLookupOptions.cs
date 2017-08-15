using Heijden.DNS;

namespace NETworkManager.Models.Network
{
    public class DNSLookupOptions
    {
        public bool UseCustomDNSServer { get; set; }
        public string CustomDNSServer { get; set; }
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
