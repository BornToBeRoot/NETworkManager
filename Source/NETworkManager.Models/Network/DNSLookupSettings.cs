using DnsClient;
using System;

namespace NETworkManager.Models.Network
{
    public class DNSLookupSettings
    {
        public bool UseCustomDNSServer { get; set; } = false;
        public DNSServerInfo CustomDNSServer { get; set; }
        public bool AddDNSSuffix { get; set; } = true;
        public bool UseCustomDNSSuffix { get; set; } = false;
        public string CustomDNSSuffix { get; set; }
        public QueryClass QueryClass { get; set; } = QueryClass.IN;
        public QueryType QueryType { get; set; } = QueryType.ANY;

        public bool UseCache { get; set; } = false;
        public bool Recursion { get; set; } = true;
        public bool UseTCPOnly { get; set; } = false;
        public int Retries { get; set; } = 3;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);        

        public DNSLookupSettings()
        {

        }
    }
}
