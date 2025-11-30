using Windows.Devices.WiFi;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains information about a Wi-Fi network.
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
    ///     Information's about an available Wi-Fi network.
    /// </summary>
    public WiFiAvailableNetwork AvailableNetwork { get; init; }

    /// <summary>
    ///    Radio that is used like 2.4 GHz, 5 GHz, etc.
    /// </summary>
    public WiFiRadio Radio { get; init; }

    /// <summary>
    ///    The channel center frequency in Gigahertz like 2.4 GHz, 5 GHz, etc.
    /// </summary>
    public double ChannelCenterFrequencyInGigahertz { get; set; }

    /// <summary>
    ///     The channel used by the Wi-Fi network.
    /// </summary>
    public int Channel { get; set; }

    /// <summary>
    ///     Indicates if the Wi-Fi network Ssid is hidden.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    ///     Indicates if the Wi-Fi network is connected to the current WiFi adapter.
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Human-readable network authentication type.
    /// </summary>
    public string NetworkAuthenticationType { get; set; }

    /// <summary>
    /// Vendor of the Wi-Fi network like Cisco, Netgear, etc.
    /// </summary>
    public string Vendor { get; set; }

    /// <summary>
    /// Human-readable phy kind.
    /// </summary>
    public string PhyKind { get; set; }
    #endregion
}