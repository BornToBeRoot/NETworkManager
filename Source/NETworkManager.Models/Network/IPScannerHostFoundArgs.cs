using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains a host found by the <see cref="IPScanner" />.
/// </summary>
/// <param name="host">IP Scanner host information. Further results arrive via <see cref="IPScanner.HostUpdated" />.</param>
public class IPScannerHostFoundArgs(IPScannerHostInfo host) : EventArgs
{
    /// <summary>
    ///     IP Scanner host information.
    /// </summary>
    public IPScannerHostInfo Host { get; } = host;
}
