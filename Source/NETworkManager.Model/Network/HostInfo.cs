using System.Net.NetworkInformation;

namespace NETworkManager.Model.Network
{
    public class HostInfo
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }

        public HostInfo()
        {

        }

        public HostInfo(PingInfo pingInfo, string hostname, PhysicalAddress macAddress)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
        }
        
        public static HostInfo Parse(HostFoundArgs e)
        {
            return new HostInfo(e.PingInfo, e.Hostname, e.MACAddress);
        }
    }
}
