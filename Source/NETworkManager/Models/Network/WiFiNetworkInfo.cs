using System;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

namespace NETworkManager.Models.Network
{
    public class WiFiNetworkInfo
    {
        public string BSSID { get; set; }
        public string SSID { get; set; }
        public int ChannelCenterFrequencyInKilohertz { get; set; }
        public byte SignalBars { get; set; }
        public bool IsWiFiDirect { get; set; }
        public double NetworkRssiInDecibelMilliwatts { get; set; }
        public WiFiPhyKind PhyKind { get; set; }
        public WiFiNetworkKind NetworkKind { get; set; }
        public NetworkSecuritySettings Security { get; set; }
        public TimeSpan BeaconInterval { get; set; }
        public TimeSpan Uptime { get; set; }

        public WiFiNetworkInfo()
        {

        }
    }
}
