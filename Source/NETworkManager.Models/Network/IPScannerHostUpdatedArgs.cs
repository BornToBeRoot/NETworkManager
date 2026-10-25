using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Contains an update of a host found by the <see cref="IPScanner" /> (e.g. the DNS hostname or the MAC address
///     arrived). The scanner never changes a host itself after it was raised in <see cref="IPScanner.HostFound" />:
///     the consumer applies <see cref="Apply" /> on the thread that owns the host (UI thread), in the order the updates
///     were raised.
/// </summary>
/// <param name="host">Host to update.</param>
/// <param name="update">Changes to apply to the host.</param>
public class IPScannerHostUpdatedArgs(IPScannerHostInfo host, Action<IPScannerHostInfo> update) : EventArgs
{
    /// <summary>
    ///     Host to update.
    /// </summary>
    public IPScannerHostInfo Host { get; } = host;

    /// <summary>
    ///     Applies the update to <see cref="Host" />. Call it on the thread that owns the host (UI thread).
    /// </summary>
    public void Apply()
    {
        update(Host);
    }
}
