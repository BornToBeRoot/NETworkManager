using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolInfo
    {
        public string Device { get; set; }
        public string Port { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string VLAN { get; set; }
        public string IPAddress { get; set; }
        public string Protocol { get; set; }
        public string Time { get; set; }

        public DiscoveryProtocolInfo()
        {

        }

    }
}