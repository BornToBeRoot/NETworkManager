namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Second argument of a run command after the target. <see cref="Subnet" /> is a new subnet mask or a second
///     subnet (Subnet Calculator), <see cref="Duration" /> the duration in seconds (mDNS / DNS-SD).
/// </summary>
public enum RunCommandArgument
{
    None,
    Ports,
    DNSServer,
    Broadcast,
    Subnet,
    Duration
}
