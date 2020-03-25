using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network
{
    public class WiFiAdapterInfo
    {
        public NetworkInterfaceInfo NetworkInterfaceInfo { get; set; }
        public WiFiAdapter WiFiAdapter { get; set; }

        public WiFiAdapterInfo()
        {
            
        }
    }
}
