using System.Collections.Generic;
using NETworkManager.Controls;
using NETworkManager.Models;

namespace NETworkManager.Settings;

/// <summary>
/// Provides access to a categorized list of all available settings views for the application.
/// </summary>
/// <remarks>The settings views are grouped into general and application-specific categories, each represented by
/// a SettingsViewInfo object containing the view's name, icon, and group classification. This class is intended to
/// facilitate configuration and display of settings within the application.</remarks>
public static class SettingsViewManager
{
    /// <summary>
    /// Gets a static, categorized list of available settings view information for the application.
    /// </summary>
    /// <remarks>The collection includes both general and application-specific settings, each represented by a
    /// SettingsViewInfo object containing the setting's name, icon, and group. This list enables easy access to
    /// configuration options and is intended for use in settings navigation or display scenarios.</remarks>
    public static List<SettingsViewInfo> List =>
    [
        // General
        new(SettingsName.General, new LucideIcon { Kind = LucideIconKind.Layers },
            SettingsGroup.General),
        new(SettingsName.Window, new LucideIcon { Kind = LucideIconKind.AppWindow },
            SettingsGroup.General),
        new(SettingsName.Appearance, new LucideIcon { Kind = LucideIconKind.Palette },
            SettingsGroup.General),
        new(SettingsName.Language, new LucideIcon { Kind = LucideIconKind.Languages },
            SettingsGroup.General),
        new(SettingsName.Network, new LucideIcon { Kind = LucideIconKind.EthernetPort },
            SettingsGroup.General),
        new(SettingsName.Status, new LucideIcon { Kind = LucideIconKind.Activity },
            SettingsGroup.General),
        new(SettingsName.HotKeys,
            new LucideIcon { Kind = LucideIconKind.Keyboard },
            SettingsGroup.General),
        new(SettingsName.Autostart, new LucideIcon { Kind = LucideIconKind.Power },
            SettingsGroup.General),
        new(SettingsName.Update,
            new LucideIcon { Kind = LucideIconKind.Rocket }, SettingsGroup.General),
        new(SettingsName.Profiles,
            new LucideIcon { Kind = LucideIconKind.Server }, SettingsGroup.General),
        new(SettingsName.Settings, new LucideIcon { Kind = LucideIconKind.Settings },
            SettingsGroup.General),

        // Applications
        new(SettingsName.Dashboard, ApplicationManager.GetIcon(ApplicationName.Dashboard),
            SettingsGroup.Application),
        new(SettingsName.NetworkInterface, ApplicationManager.GetIcon(ApplicationName.NetworkInterface),
            SettingsGroup.Application),
        new(SettingsName.WiFi, ApplicationManager.GetIcon(ApplicationName.WiFi),
            SettingsGroup.Application),
        new(SettingsName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner),
            SettingsGroup.Application),
        new(SettingsName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner),
            SettingsGroup.Application),
        new(SettingsName.PingMonitor, ApplicationManager.GetIcon(ApplicationName.PingMonitor),
            SettingsGroup.Application),
        new(SettingsName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute),
            SettingsGroup.Application),
        new(SettingsName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup),
            SettingsGroup.Application),
        new(SettingsName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop),
            SettingsGroup.Application),
        new(SettingsName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell),
            SettingsGroup.Application),
        new(SettingsName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY),
            SettingsGroup.Application),
        new(SettingsName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC),
            SettingsGroup.Application),
        new(SettingsName.WebConsole, ApplicationManager.GetIcon(ApplicationName.WebConsole),
            SettingsGroup.Application),
        new(SettingsName.SNMP, ApplicationManager.GetIcon(ApplicationName.SNMP),
            SettingsGroup.Application),
        new(SettingsName.SNTPLookup, ApplicationManager.GetIcon(ApplicationName.SNTPLookup),
            SettingsGroup.Application),
        new(SettingsName.Firewall, ApplicationManager.GetIcon(ApplicationName.Firewall),
            SettingsGroup.Application),
        new(SettingsName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN),
            SettingsGroup.Application),
        new(SettingsName.RDAP, ApplicationManager.GetIcon(ApplicationName.RDAP),
            SettingsGroup.Application),
        new(SettingsName.BitCalculator, ApplicationManager.GetIcon(ApplicationName.BitCalculator),
            SettingsGroup.Application),
    ];
}
