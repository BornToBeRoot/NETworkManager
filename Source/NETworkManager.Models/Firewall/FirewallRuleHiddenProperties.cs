using System;

namespace NETworkManager.Models.Firewall;

/// <summary>
/// Properties of a firewall rule that are not supported (and not shown) by NETworkManager, but restrict
/// the traffic the rule applies to. If they are lost (e.g. when a rule is copied), the new rule applies
/// to more traffic than the original rule.
/// </summary>
[Flags]
public enum FirewallRuleHiddenProperties
{
    /// <summary>
    /// No hidden properties.
    /// </summary>
    None = 0,

    /// <summary>
    /// ICMP types (e.g. only echo request).
    /// </summary>
    IcmpTypes = 1,

    /// <summary>
    /// Windows service (e.g. only a single service hosted by svchost.exe).
    /// </summary>
    Service = 2,

    /// <summary>
    /// App package (e.g. only a single Microsoft Store app).
    /// </summary>
    Package = 4,

    /// <summary>
    /// Network adapters (interface aliases, e.g. only the VPN adapter).
    /// </summary>
    InterfaceAlias = 8,

    /// <summary>
    /// Security conditions (local / remote users or computers, required authentication or encryption).
    /// </summary>
    Security = 16
}