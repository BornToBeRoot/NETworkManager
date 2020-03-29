using NETworkManager.Models.Lookup;
using System.Net;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network
{
    public partial class PortInfo
    {
        public IPAddress IPAddress { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public PortLookupInfo LookupInfo { get; set; }
        public PortState State { get; set; }

        public int IPAddressInt32 => IPAddress != null && IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4Address.ToInt32(IPAddress) : 0;

        public PortInfo()
        {

        }

        public PortInfo(IPAddress ipAddress, string hostname, int port, PortLookupInfo lookupInfo, PortState status)
        {
            IPAddress = ipAddress;
            Hostname = hostname;
            Port = port;
            LookupInfo = lookupInfo;
            State = status;
        }

        public static PortInfo Parse(PortScannedArgs e)
        {
            return new PortInfo(e.IPAddress, e.Hostname, e.Port, e.LookupInfo, e.State);
        }
    }
}
