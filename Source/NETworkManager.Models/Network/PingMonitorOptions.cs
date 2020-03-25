using System.Net;

namespace NETworkManager.Models.Network
{
    public class PingMonitorOptions
    {
        public string Host { get; set; }
        public IPAddress IPAddress { get; set; }

        public PingMonitorOptions(string host, IPAddress ipAddress)
        {
            Host = host;
            IPAddress = ipAddress;
        }        
    }
}
