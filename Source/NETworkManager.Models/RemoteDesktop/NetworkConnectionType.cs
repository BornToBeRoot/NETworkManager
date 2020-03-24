namespace NETworkManager.Models.RemoteDesktop
{
    /// <summary>
    /// Enum indicates the network connection used between the client and server. 
    /// The network connection type information passed on to the server helps the server tune several parameters based on the network connection type.
    /// See also: https://docs.microsoft.com/en-us/windows/desktop/termserv/imsrdpclientadvancedsettings7-networkconnectiontype 
    /// </summary>
    public enum NetworkConnectionType
    {
        /// <summary>
        /// Detect the network connection type automatically.
        /// </summary>
        DetectAutomatically,

        /// <summary>
        /// Modem (56 Kbps).
        /// </summary>
        Modem,

        /// <summary>
        /// Low-speed broadband (256 Kbps to 2 Mbps).
        /// </summary>
        BroadbandLow,

        /// <summary>
        /// Satellite (2 Mbps to 16 Mbps, with high latency).
        /// </summary>
        Satellite,

        /// <summary>
        /// High-speed broadband (2 Mbps to 10 Mbps).
        /// </summary>
        BroadbandHigh,

        /// <summary>
        /// Wide area network (WAN) (10 Mbps or higher, with high latency).
        /// </summary>
        WAN,

        /// <summary>
        /// Local area network (LAN) (10 Mbps or higher).
        /// </summary>
        LAN
    }
}
