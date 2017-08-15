using System;
using System.Net;

namespace NETworkManager.Models.Network
{
    public class PortScannedArgs : EventArgs
    {
        public Tuple <IPAddress, string> Host { get; set; }
        public int Port { get; set; }
        public PortLookupInfo LookupInfo { get; set; }
        public PortInfo.PortStatus Status { get; set; }

        public PortScannedArgs()
        {

        }

        public PortScannedArgs(Tuple<IPAddress, string> host, int port, PortLookupInfo lookupInfo, PortInfo.PortStatus status)
        {
            Host = host;
            Port = port;
            LookupInfo = lookupInfo;
            Status = status;
        }
    }
}
