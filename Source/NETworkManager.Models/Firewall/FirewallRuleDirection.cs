namespace NETworkManager.Models.Firewall;

/// <summary>
/// Represents a firewall rule direction that allows or processes network traffic
/// incoming to the system or network from external sources.
/// </summary>
public enum FirewallRuleDirection
{
    /// <summary>
    /// Inbound packets.
    /// </summary>
    Inbound,

    /// <summary>
    /// Outbound packets.
    /// </summary>
    Outbound
}