using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains information about a WiFi network.
/// </summary>
public class WiFiNetworkInfo
{
    /// <summary>
    ///     /// Create an instance of <see cref="WiFiNetworkInfo" />.
    /// </summary>
    public WiFiNetworkInfo()
    {
    }

    #region Variables

    /// <summary>
    ///     Information's about an available WiFi network.
    /// </summary>
    public WiFiAvailableNetwork AvailableNetwork { get; init; }

    /// <summary>
    ///     Indicates if the WiFi network Ssid is hidden.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    ///     Indicates if the WiFi network is connected to the current WiFi adapter.
    /// </summary>
    public bool IsConnected { get; set; }

    #endregion
}