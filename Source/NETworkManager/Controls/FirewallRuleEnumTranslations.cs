using NETworkManager.Models.Firewall;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

/// <summary>
/// Static class to profile strings for Enum translations.
/// </summary>
public static class FirewallRuleEnumTranslation
{
    /// <summary>
    /// Names of the firewall rule actions.
    /// </summary>
    public static string[] ActionNames =>
        FirewallRuleViewModel.GetEnumTranslation(typeof(FirewallRuleAction));

    /// <summary>
    /// Names of the directions.
    /// </summary>
    public static string[] DirectionNames =>
        FirewallRuleViewModel.GetEnumTranslation(typeof(FirewallRuleDirection));

    /// <summary>
    /// Names of the protocols.
    /// </summary>
    public static string[] ProtocolNames =>
        FirewallRuleViewModel.GetEnumTranslation(typeof(FirewallProtocol));

    /// <summary>
    /// Names of the interface types.
    /// </summary>
    public static string[] InterfaceTypeNames =>
        FirewallRuleViewModel.GetEnumTranslation(typeof(FirewallInterfaceType));
}

