namespace NETworkManager.Models.RemoteDesktop
{
    // https://docs.microsoft.com/en-us/windows/desktop/termserv/imsrdpclientadvancedsettings7-networkconnectiontype
    // Convert to uint
    public enum NetworkConnectionType
    {
        DetectAutomatically,
        Modem,
        BroadbandLow,
        Satellite,
        BroadbandHigh,
        WAN,
        LAN
    }
}
