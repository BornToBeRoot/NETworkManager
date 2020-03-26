using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class HostFoundArgs : System.EventArgs
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }
        public string Vendor { get; set; }

        public HostFoundArgs()
        {

        }

        public HostFoundArgs(PingInfo pingInfo, string hostname, PhysicalAddress macAddress, string vendor)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
            Vendor = vendor;
        }
    }
}
