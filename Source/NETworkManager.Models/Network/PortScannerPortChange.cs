namespace NETworkManager.Models.Network;

/// <summary>
///     Change of a port since the previous scan of the same hosts and ports in the same tab (see
///     <see cref="PortScannerScanComparer" />).
/// </summary>
public enum PortScannerPortChange
{
    /// <summary>
    ///     No change, or there is no previous scan to compare with.
    /// </summary>
    None,

    /// <summary>
    ///     The port is open now, it was not open in the previous scan.
    /// </summary>
    Opened,

    /// <summary>
    ///     The port was open in the previous scan, it is not open now (closed, timed out or the host was not scanned).
    /// </summary>
    Closed
}
