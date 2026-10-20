namespace NETworkManager.Models.Firewall;

/// <summary>
/// Defines where a firewall rule comes from (based on the <c>PolicyStoreSourceType</c> of the rule).
/// Only <see cref="Local"/> rules can be changed locally.
/// </summary>
public enum FirewallRuleSource
{
    /// <summary>
    /// Rule from the local store (e.g. created by NETworkManager, an application or in the Windows Firewall management console).
    /// </summary>
    Local,

    /// <summary>
    /// Rule from a Group Policy (domain or local Group Policy).
    /// </summary>
    GroupPolicy,

    /// <summary>
    /// Rule from a Mobile Device Management (MDM) policy (e.g. Microsoft Intune).
    /// </summary>
    MDM,

    /// <summary>
    /// Rule from another source (e.g. dynamic, generated or hardcoded rules created by Windows at runtime).
    /// </summary>
    Other
}