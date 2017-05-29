
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class IPScannerHostInfo
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }

        public IPScannerHostInfo()
        {

        }

        public IPScannerHostInfo(PingInfo pingInfo, string hostname, PhysicalAddress macAddress)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
        }
        
        public static IPScannerHostInfo Parse(IPScannerHostFoundArgs e)
        {
            return new IPScannerHostInfo(e.PingInfo, e.Hostname, e.MACAddress);
        }
    }
}
