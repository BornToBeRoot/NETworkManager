namespace NETworkManager.Models.Network;

/// <summary>
///     Reachability state of an IP neighbor entry.
///     Values match the <c>State</c> field of the <c>MIB_IPNET_ROW2</c> structure
///     returned by <c>GetIpNetTable2</c>, so a direct cast is safe.
/// </summary>
public enum NeighborState
{
    Unreachable = 1,
    Incomplete = 2,
    Probe = 3,
    Delay = 4,
    Stale = 5,
    Reachable = 6,
    Permanent = 7
}
