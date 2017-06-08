using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class HostInfo
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }
        public string Vendor { get; set; }

        public HostInfo()
        {

        }

        public HostInfo(PingInfo pingInfo, string hostname, PhysicalAddress macAddress, string vendor)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
            Vendor = vendor;
        }

        public static HostInfo Parse(HostFoundArgs e)
        {
            return new HostInfo(e.PingInfo, e.Hostname, e.MACAddress, e.Vendor);
        }
    }
}
