namespace NETworkManager.Models.Firewall;

/// <summary>
/// Represents the action, if the rule filter applies.
/// </summary>
public enum FirewallRuleAction
{
    /// <summary>
    /// Represents the action to block network traffic in a firewall rule.
    /// </summary>
    Block,

    /// <summary>
    /// Represents the action to allow network traffic.
    /// </summary>
    Allow,
    // Unsupported for now
    //AllowIPsec
}