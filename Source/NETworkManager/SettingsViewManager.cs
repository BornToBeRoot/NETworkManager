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
            new SettingsViewInfo(Name.General, new PackIconModern{ Kind = PackIconModernKind.Box }, Group.General),
            new SettingsViewInfo(Name.Window, new PackIconMaterial { Kind = PackIconMaterialKind.Application }, Group.General),
            new SettingsViewInfo(Name.Appearance, new PackIconMaterial { Kind = PackIconMaterialKind.AutoFix }, Group.General),
            new SettingsViewInfo(Name.Language, new PackIconMaterial { Kind = PackIconMaterialKind.Flag }, Group.General),
            new SettingsViewInfo(Name.HotKeys, new PackIconOcticons { Kind = PackIconOcticonsKind.Keyboard }, Group.General),
            new SettingsViewInfo(Name.Autostart, new PackIconMaterial { Kind = PackIconMaterialKind.Power }, Group.General),
            new SettingsViewInfo(Name.Update, new PackIconMaterial { Kind = PackIconMaterialKind.Download }, Group.General),
            new SettingsViewInfo(Name.ImportExport, new PackIconMaterial { Kind = PackIconMaterialKind.Import }, Group.General),
            new SettingsViewInfo(Name.Settings, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Cog }, Group.General),

            // Applications
            new SettingsViewInfo(Name.IPScanner, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.IPScanner), Group.Applications),
            new SettingsViewInfo(Name.PortScanner,ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PortScanner), Group.Applications),
            new SettingsViewInfo(Name.Ping, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Ping), Group.Applications),
            new SettingsViewInfo(Name.Traceroute, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Traceroute), Group.Applications),
            new SettingsViewInfo(Name.DNSLookup, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.DNSLookup), Group.Applications ),
            new SettingsViewInfo(Name.RemoteDesktop, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.RemoteDesktop), Group.Applications),
            new SettingsViewInfo(Name.PowerShell, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PowerShell), Group.Applications),
            new SettingsViewInfo(Name.PuTTY, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PuTTY), Group.Applications),
            new SettingsViewInfo(Name.TightVNC, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.TightVNC), Group.Applications),
            new SettingsViewInfo(Name.SNMP, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.SNMP), Group.Applications),
            new SettingsViewInfo(Name.WakeOnLAN, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.WakeOnLAN), Group.Applications),
            new SettingsViewInfo(Name.HTTPHeaders, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.HTTPHeaders), Group.Applications),
            new SettingsViewInfo(Name.Whois, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Whois), Group.Applications)
        };

        public static string TranslateName(Name name, Group group)
        {
            switch (name)
            {
                case Name.General:
                    return Resources.Localization.Strings.General;
                case Name.Window:
                    return Resources.Localization.Strings.Window;
                case Name.Appearance:
                    return Resources.Localization.Strings.Appearance;
                case Name.Language:
                    return Resources.Localization.Strings.Language;
                case Name.HotKeys:
                    return Resources.Localization.Strings.HotKeys;
                case Name.Autostart:
                    return Resources.Localization.Strings.Autostart;
                case Name.Update:
                    return Resources.Localization.Strings.Update;
                case Name.ImportExport:
                    return Resources.Localization.Strings.ImportExport;
                case Name.Settings:
                    return Resources.Localization.Strings.Settings;
                case Name.IPScanner:
                    return Resources.Localization.Strings.IPScanner;
                case Name.PortScanner:
                    return Resources.Localization.Strings.PortScanner;
                case Name.Ping:
                    return Resources.Localization.Strings.Ping;
                case Name.Traceroute:
                    return Resources.Localization.Strings.Traceroute;
                case Name.DNSLookup:
                    return Resources.Localization.Strings.DNSLookup;
                case Name.RemoteDesktop:
                    return Resources.Localization.Strings.RemoteDesktop;
                case Name.PowerShell:
                    return Resources.Localization.Strings.PowerShell;
                case Name.PuTTY:
                    return Resources.Localization.Strings.PuTTY;
                case Name.TightVNC:
                    return Resources.Localization.Strings.TightVNC;
                case Name.SNMP:
                    return Resources.Localization.Strings.SNMP;
                case Name.WakeOnLAN:
                    return Resources.Localization.Strings.WakeOnLAN;
                case Name.HTTPHeaders:
                    return Resources.Localization.Strings.HTTPHeaders;
                case Name.Whois:
                    return Resources.Localization.Strings.Whois;
                default:
                    return "Translation of name not found";
            }
        }

        public static string TranslateGroup(Group group)
        {
            switch (group)
            {
                case Group.General:
                    return Resources.Localization.Strings.General;
                case Group.Applications:
                    return Resources.Localization.Strings.Applications;
                default:
                    return "Translation of group not found!";
            }
        }

        public enum Name
        {
            General,
            Window,
            Appearance,
            Language,
            HotKeys,
            Autostart,
            Update,
            ImportExport,
            Settings,
            IPScanner,
            PortScanner,
            Ping,
            Traceroute,
            DNSLookup,
            RemoteDesktop,
            PowerShell,
            PuTTY,
            TightVNC,
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
