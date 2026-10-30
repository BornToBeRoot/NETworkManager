namespace NETworkManager.Models.Network;

/// <summary>
///     Reachability state of an IP neighbor entry.
///     Values match the <c>NL_NEIGHBOR_STATE</c> enumeration (<c>State</c> field of the <c>MIB_IPNET_ROW2</c>
///     structure returned by <c>GetIpNetTable2</c>, starts at 0), so a direct cast is safe.
/// </summary>
public enum NeighborState
{
    Unreachable = 0,
    Incomplete = 1,
    Probe = 2,
    Delay = 3,
    Stale = 4,
    Reachable = 5,
    Permanent = 6
}
