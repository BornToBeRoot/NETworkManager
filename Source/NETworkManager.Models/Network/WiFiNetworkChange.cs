namespace NETworkManager.Models.Network;

/// <summary>
///     Change of a WiFi network (BSSID) since the previous scan of the same adapter (see <see cref="WiFiScanComparer" />).
/// </summary>
public enum WiFiNetworkChange
{
    /// <summary>
    ///     No change, or there is no previous scan to compare with.
    /// </summary>
    None,

    /// <summary>
    ///     The network is visible now, it was not visible in the previous scan.
    /// </summary>
    New,

    /// <summary>
    ///     The network was visible in the previous scan, it is not visible now.
    /// </summary>
    Gone,

    /// <summary>
    ///     The channel, the channel width or the security of the network changed (e.g. the access point switched the
    ///     channel because of interference or radar).
    /// </summary>
    Changed
}
