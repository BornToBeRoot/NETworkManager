using System;

namespace NETworkManager.Models.Firewall;

/// <summary>
/// Exception thrown when one or more firewall rules no longer exist (e.g. deleted in the Windows Firewall
/// management console). Nothing is changed in this case. Used to show a localized message.
/// </summary>
public class FirewallRuleNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FirewallRuleNotFoundException"/> class.
    /// </summary>
    public FirewallRuleNotFoundException() : base("One or more firewall rules no longer exist.")
    {
    }
}