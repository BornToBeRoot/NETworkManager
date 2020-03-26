using System.Net;

namespace NETworkManager.Models.Network
{
    public class DNSLookupErrorArgs : System.EventArgs
    {
        public string ErrorCode { get; set; }
        public IPEndPoint DNSServer { get; set; }

        public DNSLookupErrorArgs()
        {

        }

        public DNSLookupErrorArgs(string errorCode, IPEndPoint dnsServer)
        {
            ErrorCode = errorCode;
            DNSServer = dnsServer;
        }
    }
}
