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
            new SettingsViewInfo(Name.Dashboard, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Dashboard), Group.Applications),
            new SettingsViewInfo(Name.IPScanner, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.IPScanner), Group.Applications),
            new SettingsViewInfo(Name.PortScanner,ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PortScanner), Group.Applications),
            new SettingsViewInfo(Name.Ping, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Ping), Group.Applications),
            new SettingsViewInfo(Name.Traceroute, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Traceroute), Group.Applications),
            new SettingsViewInfo(Name.DNSLookup, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.DNSLookup), Group.Applications ),
            new SettingsViewInfo(Name.RemoteDesktop, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.RemoteDesktop), Group.Applications),
            new SettingsViewInfo(Name.PowerShell, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PowerShell), Group.Applications),
            new SettingsViewInfo(Name.PuTTY, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PuTTY), Group.Applications),
            new SettingsViewInfo(Name.TigerVNC, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.TigerVNC), Group.Applications),
            new SettingsViewInfo(Name.SNMP, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.SNMP), Group.Applications),
            new SettingsViewInfo(Name.WakeOnLAN, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.WakeOnLAN), Group.Applications),
            new SettingsViewInfo(Name.HTTPHeaders, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.HTTPHeaders), Group.Applications),
            new SettingsViewInfo(Name.Whois, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Whois), Group.Applications)
        };

        public static string TranslateName(Name name)
        {
            switch (name)
            {
                case Name.General:
                    return Localization.LanguageFiles.Strings.General;
                case Name.Window:
                    return Localization.LanguageFiles.Strings.Window;
                case Name.Appearance:
                    return Localization.LanguageFiles.Strings.Appearance;
                case Name.Language:
                    return Localization.LanguageFiles.Strings.Language;
                case Name.Status:
                    return Localization.LanguageFiles.Strings.Status;
                case Name.HotKeys:
                    return Localization.LanguageFiles.Strings.HotKeys;
                case Name.Autostart:
                    return Localization.LanguageFiles.Strings.Autostart;
                case Name.Update:
                    return Localization.LanguageFiles.Strings.Update;
                case Name.Settings:
                    return Localization.LanguageFiles.Strings.Settings;
                case Name.Profiles:
                    return Localization.LanguageFiles.Strings.Profiles;
                case Name.Dashboard:
                    return Localization.LanguageFiles.Strings.Dashboard;
                case Name.IPScanner:
                    return Localization.LanguageFiles.Strings.IPScanner;
                case Name.PortScanner:
                    return Localization.LanguageFiles.Strings.PortScanner;
                case Name.Ping:
                    return Localization.LanguageFiles.Strings.Ping;
                case Name.Traceroute:
                    return Localization.LanguageFiles.Strings.Traceroute;
                case Name.DNSLookup:
                    return Localization.LanguageFiles.Strings.DNSLookup;
                case Name.RemoteDesktop:
                    return Localization.LanguageFiles.Strings.RemoteDesktop;
                case Name.PowerShell:
                    return Localization.LanguageFiles.Strings.PowerShell;
                case Name.PuTTY:
                    return Localization.LanguageFiles.Strings.PuTTY;
                case Name.TigerVNC:
                    return Localization.LanguageFiles.Strings.TigerVNC;
                case Name.SNMP:
                    return Localization.LanguageFiles.Strings.SNMP;
                case Name.WakeOnLAN:
                    return Localization.LanguageFiles.Strings.WakeOnLAN;
                case Name.HTTPHeaders:
                    return Localization.LanguageFiles.Strings.HTTPHeaders;
                case Name.Whois:
                    return Localization.LanguageFiles.Strings.Whois;
                default:
                    return "Name translation not found";
            }
        }

        public static string TranslateGroup(Group group)
        {
            switch (group)
            {
                case Group.General:
                    return Localization.LanguageFiles.Strings.General;
                case Group.Applications:
                    return Localization.LanguageFiles.Strings.Applications;
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

