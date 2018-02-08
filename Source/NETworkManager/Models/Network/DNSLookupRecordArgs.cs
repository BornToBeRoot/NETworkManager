using Heijden.DNS;

namespace NETworkManager.Models.Network
{
    public class DNSLookupRecordArgs : System.EventArgs
    {        
        public RR ResourceRecord { get; set; }
        public string Result { get; set; }
        public string DNSServer { get; set; }
        public int Port { get; set; }

        public DNSLookupRecordArgs()
        {

        }

        public DNSLookupRecordArgs(RR resourceRecord, object result, string dnsServer, int port)
        {
            ResourceRecord = resourceRecord;
            Result = result.ToString().TrimEnd();
            DNSServer = dnsServer;
            Port = port;
        }
    }
}
