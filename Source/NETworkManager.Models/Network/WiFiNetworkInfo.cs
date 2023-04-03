using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network;

public class WiFiNetworkInfo
{
    #region Variables
    public WiFiAvailableNetwork AvailableNetwork { get; set; }
   
    public bool IsConnected { get; set; }
    #endregion

    public WiFiNetworkInfo()
    {
        
    }
}
