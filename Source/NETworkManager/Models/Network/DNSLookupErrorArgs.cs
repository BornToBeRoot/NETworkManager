using System.Net;

namespace NETworkManager.Models.Network
{
    public class DNSLookupErrorArgs : System.EventArgs
    {
        public string ErrorCode { get; set; }
        public string DNSServer { get; set; }

        public DNSLookupErrorArgs()
        {

        }

        public DNSLookupErrorArgs(string errorCode, string dnsServer)
        {
            ErrorCode = errorCode;
            DNSServer = dnsServer;
        }
    }
}
