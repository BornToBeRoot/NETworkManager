using System;
using System.Net;

namespace NETworkManager.Models.Network
{
    public class PortInfo
    {
        public Tuple<IPAddress, string> Host { get; set; }
        public int Port { get; set; }
        public PortLookupInfo LookupInfo { get; set; }
        public PortStatus Status { get; set; }

        public PortInfo()
        {

        }

        public PortInfo(Tuple<IPAddress, string> host, int port, PortLookupInfo lookupInfo, PortStatus status)
        {
            Host = host;
            Port = port;
            LookupInfo = lookupInfo;
            Status = status;
        }

        public static PortInfo Parse(PortScannedArgs e)
        {
            return new PortInfo(e.Host, e.Port, e.LookupInfo, e.Status);
        }

        public enum PortStatus
        {
            Open,
            Closed
        }
    }
}
