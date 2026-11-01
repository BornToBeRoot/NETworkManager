namespace NETworkManager.Models.Network;

/// <summary>
///     Change of a host since the previous scan of the same input in the same tab (see
///     <see cref="IPScannerScanComparer" />).
/// </summary>
public enum IPScannerHostChange
{
    /// <summary>
    ///     No change, or there is no previous scan to compare with.
    /// </summary>
    None,

    /// <summary>
    ///     The host is up now, it was not up in the previous scan.
    /// </summary>
    New,

    /// <summary>
    ///     The host was up in the previous scan, it is not up now.
    /// </summary>
    Gone,

    /// <summary>
    ///     The host is up with another MAC address than in the previous scan (both known), e.g. a replaced device, an
    ///     IP address conflict or ARP spoofing.
    /// </summary>
    MACChanged
}
