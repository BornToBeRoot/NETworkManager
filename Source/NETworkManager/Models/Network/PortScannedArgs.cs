using System.Net;

namespace NETworkManager.Models.Network
{
    public class PortScannedArgs : System.EventArgs
    {
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
        public PortLookupInfo LookupInfo { get; set; }
        public PortInfo.PortStatus Status { get; set; }

        public PortScannedArgs()
        {

        }

        public PortScannedArgs(IPAddress ipAddress, int port, PortLookupInfo lookupInfo, PortInfo.PortStatus status)
        {
            IPAddress = ipAddress;
            Port = port;
            LookupInfo = lookupInfo;
            Status = status;
        }
    }
}
