using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class IPScannerHostFoundArgs : System.EventArgs
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }
        public string Vendor { get; set; }

        public IPScannerHostFoundArgs()
        {

        }

        public IPScannerHostFoundArgs(PingInfo pingInfo, string hostname, PhysicalAddress macAddress, string vendor)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
            Vendor = vendor;
        }
    }
}
