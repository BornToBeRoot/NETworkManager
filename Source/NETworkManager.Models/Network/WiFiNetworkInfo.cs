using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network;


/// <summary>
/// Class contains information about a WiFi network.
/// </summary>
public class WiFiNetworkInfo
{
    #region Variables
    /// <summary>
    /// Informations about an available wifi network.
    /// </summary>
    public WiFiAvailableNetwork WiFiAvailableNetwork { get; set; }

    /// <summary>
    /// Indicates if the network is connected to the current network adapter.
    /// </summary>
    public bool IsConnected { get; set; }
    #endregion

    /// <summary>
    /// /// Create an instance of <see cref="WiFiNetworkInfo"/>.
    /// </summary>
    public WiFiNetworkInfo()
    {
        
    }
}
