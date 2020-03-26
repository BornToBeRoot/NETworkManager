using MahApps.Metro.IconPacks;
using System.Collections.Generic;

namespace NETworkManager
{
    public static class SettingsViewManager
    {
        // List of all applications
        public static List<SettingsViewInfo> List => new List<SettingsViewInfo>
        {
            // General
            new SettingsViewInfo(Name.General, new PackIconMaterial{ Kind = PackIconMaterialKind.Layers }, Group.General),
            new SettingsViewInfo(Name.Window, new PackIconPicolIcons { Kind = PackIconPicolIconsKind.BrowserWindow }, Group.General),
            new SettingsViewInfo(Name.Appearance, new PackIconMaterial { Kind = PackIconMaterialKind.Palette }, Group.General),
            new SettingsViewInfo(Name.Language, new PackIconMaterial { Kind = PackIconMaterialKind.Translate }, Group.General),
            new SettingsViewInfo(Name.Status, new PackIconModern { Kind = PackIconModernKind.Network }, Group.General),
            new SettingsViewInfo(Name.HotKeys, new PackIconOcticons { Kind = PackIconOcticonsKind.Keyboard }, Group.General),
            new SettingsViewInfo(Name.Autostart, new PackIconMaterial { Kind = PackIconMaterialKind.Power }, Group.General),
            new SettingsViewInfo(Name.Update, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Download }, Group.General),
            new SettingsViewInfo(Name.Settings, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Cog }, Group.General),
            new SettingsViewInfo(Name.Profiles, new PackIconMaterial { Kind = PackIconMaterialKind.FormatListBulletedType }, Group.General),

            // Applications
            new SettingsViewInfo(Name.Dashboard, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Dashboard), Group.Applications),
            new SettingsViewInfo(Name.IPScanner, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.IPScanner), Group.Applications),
            new SettingsViewInfo(Name.PortScanner,Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PortScanner), Group.Applications),
            new SettingsViewInfo(Name.Ping, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Ping), Group.Applications),
            new SettingsViewInfo(Name.Traceroute, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Traceroute), Group.Applications),
            new SettingsViewInfo(Name.DNSLookup, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.DNSLookup), Group.Applications ),
            new SettingsViewInfo(Name.RemoteDesktop, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.RemoteDesktop), Group.Applications),
            new SettingsViewInfo(Name.PowerShell, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PowerShell), Group.Applications),
            new SettingsViewInfo(Name.PuTTY, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PuTTY), Group.Applications),
            new SettingsViewInfo(Name.TigerVNC, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.TigerVNC), Group.Applications),
            new SettingsViewInfo(Name.SNMP, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.SNMP), Group.Applications),
            new SettingsViewInfo(Name.WakeOnLAN, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.WakeOnLAN), Group.Applications),
            new SettingsViewInfo(Name.HTTPHeaders, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.HTTPHeaders), Group.Applications),
            new SettingsViewInfo(Name.Whois, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Whois), Group.Applications)
        };

        public static string TranslateName(Name name)
        {
            switch (name)
            {
                case Name.General:
                    return Localization.Resources.Strings.General;
                case Name.Window:
                    return Localization.Resources.Strings.Window;
                case Name.Appearance:
                    return Localization.Resources.Strings.Appearance;
                case Name.Language:
                    return Localization.Resources.Strings.Language;
                case Name.Status:
                    return Localization.Resources.Strings.Status;
                case Name.HotKeys:
                    return Localization.Resources.Strings.HotKeys;
                case Name.Autostart:
                    return Localization.Resources.Strings.Autostart;
                case Name.Update:
                    return Localization.Resources.Strings.Update;
                case Name.Settings:
                    return Localization.Resources.Strings.Settings;
                case Name.Profiles:
                    return Localization.Resources.Strings.Profiles;
                case Name.Dashboard:
                    return Localization.Resources.Strings.Dashboard;
                case Name.IPScanner:
                    return Localization.Resources.Strings.IPScanner;
                case Name.PortScanner:
                    return Localization.Resources.Strings.PortScanner;
                case Name.Ping:
                    return Localization.Resources.Strings.Ping;
                case Name.Traceroute:
                    return Localization.Resources.Strings.Traceroute;
                case Name.DNSLookup:
                    return Localization.Resources.Strings.DNSLookup;
                case Name.RemoteDesktop:
                    return Localization.Resources.Strings.RemoteDesktop;
                case Name.PowerShell:
                    return Localization.Resources.Strings.PowerShell;
                case Name.PuTTY:
                    return Localization.Resources.Strings.PuTTY;
                case Name.TigerVNC:
                    return Localization.Resources.Strings.TigerVNC;
                case Name.SNMP:
                    return Localization.Resources.Strings.SNMP;
                case Name.WakeOnLAN:
                    return Localization.Resources.Strings.WakeOnLAN;
                case Name.HTTPHeaders:
                    return Localization.Resources.Strings.HTTPHeaders;
                case Name.Whois:
                    return Localization.Resources.Strings.Whois;
                default:
                    return "Name translation not found";
            }
        }

        public static string TranslateGroup(Group group)
        {
            switch (group)
            {
                case Group.General:
                    return Localization.Resources.Strings.General;
                case Group.Applications:
                    return Localization.Resources.Strings.Applications;
                default:
                    return "Group translation not found!";
            }
        }

        public enum Name
        {
            General,
            Window,
            Appearance,
            Language,
            Status,
            HotKeys,
            Autostart,
            Update,
            Settings,
            Profiles,
            Dashboard,
            IPScanner,
            PortScanner,
            Ping,
            Traceroute,
            DNSLookup,
            RemoteDesktop,
            PowerShell,
            PuTTY,
            TigerVNC,
            SNMP,
            WakeOnLAN,
            HTTPHeaders,
            Whois
        }

        public enum Group
        {
            General,
            Applications
        }
    }
}

