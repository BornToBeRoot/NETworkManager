namespace NETworkManager.Models.Network;

/// <summary>
/// Enum for the WiFi connect mode.
/// </summary>
public enum WiFiConnectMode
{
    /// <summary>
    /// WiFi network is open. 
    /// </summary>
    Open,

    /// <summary>
    /// WiFi network is protected with Pre-Shared Key (PSK).
    /// E.g WpaPsk or RsnaPsk.
    /// </summary>
    Psk,

    /// <summary>
    /// WiFi network is protected with User-Password-Authentication (EAP).
    /// E.g. Wpa or Rsna.
    /// </summary>
    Eap
}
