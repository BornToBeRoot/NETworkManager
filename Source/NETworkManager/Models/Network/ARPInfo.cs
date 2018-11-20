using NETworkManager.Utilities;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class ARPInfo
    {
        public IPAddress IPAddress { get; set; }
        public PhysicalAddress MACAddress { get; set; }
        public bool IsMulticast { get; set; }

        public int IPAddressInt32 => IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(IPAddress) : 0;

        public ARPInfo()
        {

        }

        public ARPInfo(IPAddress ipAddress, PhysicalAddress macAddress, bool isMulticast)
        {
            IPAddress = ipAddress;
            MACAddress = macAddress;
            IsMulticast = isMulticast;
        }
    }
}