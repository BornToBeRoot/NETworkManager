using DnsClient;
using DnsClient.Protocol;
using System.Net;

namespace NETworkManager.Models.Network
{
    public class DNSLookupRecordArgs : System.EventArgs
    {        
        public string DomainName { get; set; }
        public int TTL { get; set; }
        public QueryClass Class { get; set; }
        public ResourceRecordType Type { get; set; }        
        public string Result { get; set; }
        public IPEndPoint DNSServer { get; set; }

        public DNSLookupRecordArgs()
        {

        }

        public DNSLookupRecordArgs(string domainName, int ttl, QueryClass queryClass, ResourceRecordType queryType, string result, IPEndPoint dnsServer)
        {
            DomainName = domainName;
            TTL = ttl;
            Class = queryClass;
            Type = queryType;
            Result = result;
            DNSServer = dnsServer;
        }
    }
}
