using System.Net;

namespace NETworkManager.Models.Network
{
    public class SubnetInfo
    {
        public IPAddress NetworkAddress { get; set; }
        public IPAddress Broadcast { get; set; }
        public int IPAddresses { get; set; }
        public IPAddress Subnetmask { get; set; }
        public int CIDR { get; set; }
        public IPAddress HostFirstIP { get; set; }
        public IPAddress HostLastIP { get; set; }
        public int Hosts { get; set; }
    }
}
