using NETworkManager.Helpers;
using System.Net;

namespace NETworkManager.Models.Network
{
    public class SubnetInfo
    {
        public IPAddress NetworkAddress { get; set; }
        public IPAddress Broadcast { get; set; }
        public long IPAddresses { get; set; }
        public IPAddress Subnetmask { get; set; }
        public int CIDR { get; set; }
        public IPAddress HostFirstIP { get; set; }
        public IPAddress HostLastIP { get; set; }
        public long Hosts { get; set; }
        
        public int NetworkAddressInt32
        {
            get { return NetworkAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(NetworkAddress) : 0; }
        }

        public int BroadcastInt32
        {
            get { return Broadcast.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(Broadcast) : 0; }
        }

        public int HostFirstIPInt32
        {
            get { return HostFirstIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(HostFirstIP) : 0; }
        }

        public int HostLastIPInt32
        {
            get { return HostLastIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4AddressHelper.ConvertToInt32(HostLastIP) : 0; }
        }

        public SubnetInfo()
        {

        }

        public SubnetInfo(IPAddress networkAddress, IPAddress broadcast, long ipAddresses, IPAddress subnetmask, int cidr, IPAddress hostFirstIP, IPAddress hostLastIP, long hosts)
        {
            NetworkAddress = networkAddress;
            Broadcast = broadcast;
            IPAddresses = ipAddresses;
            Subnetmask = subnetmask;
            CIDR = cidr;
            HostFirstIP = hostFirstIP;
            HostLastIP = hostLastIP;
            Hosts = hosts;
        }
    }
}
