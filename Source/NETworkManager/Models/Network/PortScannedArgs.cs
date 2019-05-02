using NETworkManager.Models.Lookup;
using System;
using System.Net;

namespace NETworkManager.Models.Network
{
    public class PortScannedArgs : EventArgs
    {
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public PortLookupInfo LookupInfo { get; set; }
        public PortInfo.PortStatus Status { get; set; }

        public PortScannedArgs()
        {

        }

        public PortScannedArgs(IPAddress ipAddres, string hostname, int port, PortLookupInfo lookupInfo, PortInfo.PortStatus status)
        {
            IPAddress = ipAddres;
            Hostname = hostname;
            Port = port;
            LookupInfo = lookupInfo;
            Status = status;
        }
    }
}
