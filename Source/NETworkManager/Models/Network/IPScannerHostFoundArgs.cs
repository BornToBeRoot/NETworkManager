using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class IPScannerHostFoundArgs : System.EventArgs
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }

        public IPScannerHostFoundArgs()
        {

        }

        public IPScannerHostFoundArgs(PingInfo pingInfo, string hostname, PhysicalAddress macAddress)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
        }       
    }
}
