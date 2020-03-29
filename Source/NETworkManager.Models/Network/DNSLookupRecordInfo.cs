using DnsClient;
using DnsClient.Protocol;
using System;
using System.Net;

namespace NETworkManager.Models.Network
{
    public class DNSLookupRecordInfo
    {

        public string DomainName { get; set; }
        public int TTL { get; set; }
        public string Class { get; set; }
        public string Type { get; set; }        
        public string Result { get; set; }
        public string DNSServer { get; set; }
        public int Port { get; set; }

        public DNSLookupRecordInfo()
        {

        }

        public DNSLookupRecordInfo(string domainName, int ttl, string xclass, string type, string result, string dnsServer, int port)
        {
            DomainName = domainName;
            TTL = ttl;
            Class = xclass;
            Type = type;
            Result = result;
            DNSServer = dnsServer;
            Port = port;
        }

        public static DNSLookupRecordInfo Parse(DNSLookupRecordArgs e)
        {
            return new DNSLookupRecordInfo(e.DomainName, e.TTL, e.Class.ToString(), e.Type.ToString(), e.Result, e.DNSServer.Address.ToString(), e.DNSServer.Port);
        }
    }
}