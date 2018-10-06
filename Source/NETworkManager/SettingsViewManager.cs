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
            new SettingsViewInfo(Name.Window, new PackIconMaterial {Kind = PackIconMaterialKind.Application }, Group.General),
            new SettingsViewInfo(Name.Appearance, new PackIconMaterial { Kind = PackIconMaterialKind.AutoFix }, Group.General),
            new SettingsViewInfo(Name.Language, new PackIconMaterial { Kind = PackIconMaterialKind.Flag}, Group.General),
            new SettingsViewInfo(Name.HotKeys, new PackIconOcticons { Kind = PackIconOcticonsKind.Keyboard }, Group.General),
            new SettingsViewInfo(Name.Autostart, new PackIconMaterial { Kind = PackIconMaterialKind.Power }, Group.General),
            new SettingsViewInfo(Name.Update, new PackIconMaterial {Kind = PackIconMaterialKind.Download }, Group.General),
            new SettingsViewInfo(Name.ImportExport, new PackIconMaterial { Kind = PackIconMaterialKind.Import}, Group.General),
            new SettingsViewInfo(Name.Settings, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Cog }, Group.General),

            // Applications
            new SettingsViewInfo(Name.IPScanner, new PackIconMaterial {Kind = PackIconMaterialKind.Sitemap }, Group.Applications),
            new SettingsViewInfo(Name.PortScanner, new PackIconModern{Kind = PackIconModernKind.NetworkPort }, Group.Applications),
            new SettingsViewInfo(Name.Ping, new PackIconMaterial{ Kind = PackIconMaterialKind.LanConnect }, Group.Applications),
            new SettingsViewInfo(Name.Traceroute, new PackIconModern {Kind = PackIconModernKind.TransitConnection}, Group.Applications),
            new SettingsViewInfo(Name.DNSLookup, new PackIconMaterial { Kind= PackIconMaterialKind.SearchWeb }, Group.Applications ),
            new SettingsViewInfo(Name.RemoteDesktop, new PackIconMaterial{ Kind = PackIconMaterialKind.RemoteDesktop}, Group.Applications),
            new SettingsViewInfo(Name.PuTTY, new PackIconOcticons {Kind = PackIconOcticonsKind.Terminal}, Group.Applications),
            new SettingsViewInfo(Name.SNMP,new PackIconMaterial {Kind = PackIconMaterialKind.Switch }, Group.Applications),
            new SettingsViewInfo(Name.WakeOnLAN, new PackIconMaterial {Kind = PackIconMaterialKind.Power} , Group.Applications),
            new SettingsViewInfo(Name.HTTPHeaders, new PackIconMaterial {Kind = PackIconMaterialKind.Web}, Group.Applications),
            new SettingsViewInfo(Name.Whois, new PackIconMaterial {Kind = PackIconMaterialKind.CloudSearchOutline}, Group.Applications)
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
                case Name.PuTTY:
                    return Resources.Localization.Strings.PuTTY;
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
            PuTTY,
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
