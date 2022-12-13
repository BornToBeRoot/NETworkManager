using System.Net;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class DNSResultIPAddress : DNSResult
    {
        public IPAddress Value { get; set; }

        public DNSResultIPAddress(bool hasError, string errorMessage) : base(hasError, errorMessage)
        {

        }

        public DNSResultIPAddress(IPAddress value)
        {
            Value = value;
        }
    }
}
