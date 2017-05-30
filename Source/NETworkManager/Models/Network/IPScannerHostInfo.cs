using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class IPScannerHostInfo
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }
        public string Vendor { get; set; }

        public IPScannerHostInfo()
        {

        }

        public IPScannerHostInfo(PingInfo pingInfo, string hostname, PhysicalAddress macAddress, string vendor)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
            Vendor = vendor;
        }

        public static IPScannerHostInfo Parse(IPScannerHostFoundArgs e)
        {
            return new IPScannerHostInfo(e.PingInfo, e.Hostname, e.MACAddress, e.Vendor);
        }
    }
}
