using System;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network
{
    public class WiFiAdapterInfo
    {
        public Guid Id { get; set; }
        public NetworkInterfaceInfo NetworkInterfaceInfo { get; set; }
        
        public WiFiAdapterInfo()
        {
            
        }

    }
}
