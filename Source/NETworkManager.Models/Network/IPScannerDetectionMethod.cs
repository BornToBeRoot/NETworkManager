using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Probes of the <see cref="IPScanner" /> a host answered, i.e. how it was detected as reachable.
/// </summary>
[Flags]
public enum IPScannerDetectionMethod
{
    /// <summary>
    ///     The host didn't answer any probe.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The host answered the ping (ICMP echo reply).
    /// </summary>
    ICMP = 1,

    /// <summary>
    ///     A TCP port of the host answered (open, or the connection was refused).
    /// </summary>
    TCP = 2,

    /// <summary>
    ///     The host answered the NetBIOS request.
    /// </summary>
    NetBIOS = 4
}
