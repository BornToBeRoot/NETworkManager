using System.Collections.Generic;
using MahApps.Metro.IconPacks;
using NETworkManager.Models;

namespace NETworkManager.Settings;

public static class SettingsViewManager
{
    // List of all applications
    public static List<SettingsViewInfo> List => new()
    {
        // General
        new SettingsViewInfo(SettingsName.General, new PackIconModern { Kind = PackIconModernKind.Layer },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Window, new PackIconOcticons { Kind = PackIconOcticonsKind.Browser },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Appearance, new PackIconMaterial { Kind = PackIconMaterialKind.Palette },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Language, new PackIconMaterial { Kind = PackIconMaterialKind.Translate },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Network, new PackIconModern { Kind = PackIconModernKind.Network },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Status, new PackIconMaterial { Kind = PackIconMaterialKind.Pulse },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.HotKeys, new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.KeyboardRegular },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Autostart, new PackIconMaterial { Kind = PackIconMaterialKind.Power },
            SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Update,
            new PackIconMaterial { Kind = PackIconMaterialKind.RocketLaunchOutline }, SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Profiles,
            new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.ServerSolid }, SettingsGroup.General),
        new SettingsViewInfo(SettingsName.Settings, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Cog },
            SettingsGroup.General),

        // Applications
        new SettingsViewInfo(SettingsName.Dashboard, ApplicationManager.GetIcon(ApplicationName.Dashboard),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.PingMonitor, ApplicationManager.GetIcon(ApplicationName.PingMonitor),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.AWSSessionManager,
            ApplicationManager.GetIcon(ApplicationName.AWSSessionManager), SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.WebConsole, ApplicationManager.GetIcon(ApplicationName.WebConsole),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.SNMP, ApplicationManager.GetIcon(ApplicationName.SNMP),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.SNTPLookup, ApplicationManager.GetIcon(ApplicationName.SNTPLookup),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN),
            SettingsGroup.Application),
        new SettingsViewInfo(SettingsName.BitCalculator, ApplicationManager.GetIcon(ApplicationName.BitCalculator),
            SettingsGroup.Application)
    };
}