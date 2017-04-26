using System.Net.NetworkInformation;
using System;

namespace NETworkManager.Model.Network
{
    public class HostFoundArgs : EventArgs
    {
        public PingInfo PingInfo { get; set; }
        public string Hostname { get; set; }
        public PhysicalAddress MACAddress { get; set; }

        public HostFoundArgs()
        {

        }

        public HostFoundArgs(PingInfo pingInfo, string hostname, PhysicalAddress macAddress)
        {
            PingInfo = pingInfo;
            Hostname = hostname;
            MACAddress = macAddress;
        }       
    }
}
