namespace NETworkManager.Models.Network;

public enum WiFiConnectMode
{
    /// <summary>
    /// WiFi network is open. 
    /// </summary>
    Open,

    /// <summary>
    /// WiFi network is protected with Pre-Shared Key (PSK).
    /// </summary>
    Psk,

    /// <summary>
    /// WiFi network is protected with User-Password-Authentication (EAP).
    /// </summary>
    Eap
}
