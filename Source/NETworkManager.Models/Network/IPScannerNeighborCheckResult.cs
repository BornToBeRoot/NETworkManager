namespace NETworkManager.Models.Network;

/// <summary>
///     Result of <see cref="IPScannerNeighborCheck.Check" />: what the neighbor entry (ARP / NDP) of a host that
///     didn't answer any probe says about the host.
/// </summary>
public enum IPScannerNeighborCheckResult
{
    /// <summary>
    ///     The host is not reachable (no entry, the neighbor didn't answer, a static entry or a proxy ARP answer).
    /// </summary>
    Unreachable,

    /// <summary>
    ///     The host answered the ARP request / neighbor solicitation of the probes - it is reachable.
    /// </summary>
    Reachable,

    /// <summary>
    ///     The entry is older than the probes and was not verified again yet (e.g.
    ///     <see cref="NeighborState.Stale" />) - resolve it again to find out (see
    ///     <see cref="NeighborTable.ResolveEntry" />).
    /// </summary>
    Verify
}
