using System.Collections.Generic;
using MahApps.Metro.IconPacks;
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
        new(SettingsName.General, new PackIconModern { Kind = PackIconModernKind.Layer },
            SettingsGroup.General),
        new(SettingsName.Window, new PackIconOcticons { Kind = PackIconOcticonsKind.Browser },
            SettingsGroup.General),
        new(SettingsName.Appearance, new PackIconMaterial { Kind = PackIconMaterialKind.Palette },
            SettingsGroup.General),
        new(SettingsName.Language, new PackIconMaterial { Kind = PackIconMaterialKind.Translate },
            SettingsGroup.General),
        new(SettingsName.Network, new PackIconModern { Kind = PackIconModernKind.Network },
            SettingsGroup.General),
        new(SettingsName.Status, new PackIconMaterial { Kind = PackIconMaterialKind.Pulse },
            SettingsGroup.General),
        new(SettingsName.HotKeys,
            new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.KeyboardRegular },
            SettingsGroup.General),
        new(SettingsName.Autostart, new PackIconMaterial { Kind = PackIconMaterialKind.Power },
            SettingsGroup.General),
        new(SettingsName.Update,
            new PackIconMaterial { Kind = PackIconMaterialKind.RocketLaunchOutline }, SettingsGroup.General),
        new(SettingsName.Profiles,
            new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.ServerSolid }, SettingsGroup.General),
        new(SettingsName.Settings, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Cog },
            SettingsGroup.General),

        // Applications
        new(SettingsName.Dashboard, ApplicationManager.GetIcon(ApplicationName.Dashboard),
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
        new(SettingsName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN),
            SettingsGroup.Application),
        new(SettingsName.BitCalculator, ApplicationManager.GetIcon(ApplicationName.BitCalculator),
            SettingsGroup.Application),
    ];
}
