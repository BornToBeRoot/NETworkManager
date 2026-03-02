namespace NETworkManager.Models.Firewall;

/// <summary>
/// Ports of local host or remote host. 
/// </summary>
public enum FirewallPortLocation
{
    /// <summary>
    /// Ports of local host.
    /// </summary>
    LocalPorts,
    /// <summary>
    /// Ports of remote host.
    /// </summary>
    RemotePorts
}