namespace NETworkManager.Models.Firewall;

public enum FirewallInterfaceType
{
    /// <summary>
    /// Any interface type.
    /// </summary>
    Any = -1,
    /// <summary>
    /// Wired interface types, e.g. Ethernet.
    /// </summary>
    Wired,
    /// <summary>
    /// Wireless interface types, e.g. Wi-Fi.
    /// </summary>
    Wireless,
    /// <summary>
    /// Remote interface types, e.g. VPN, L2TP, OpenVPN, etc.
    /// </summary>
    RemoteAccess
}