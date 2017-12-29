using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.Views
{
    public static class SettingsViewManager
    {
        // List of all applications
        public static List<SettingsViewInfo> List
        {
            get
            {
                return new List<SettingsViewInfo>
                {
                    new SettingsViewInfo(Name.General, new PackIconModern() { Kind = PackIconModernKind.Box }, Group.General),
                    new SettingsViewInfo(Name.Window, new PackIconMaterial() {Kind = PackIconMaterialKind.Application }, Group.General),
                    new SettingsViewInfo(Name.Appearance, new PackIconMaterial() { Kind = PackIconMaterialKind.AutoFix }, Group.General),
                    new SettingsViewInfo(Name.Language, new PackIconMaterial() { Kind = PackIconMaterialKind.Flag}, Group.General),
                    new SettingsViewInfo(Name.HotKeys, new PackIconOcticons() { Kind = PackIconOcticonsKind.Keyboard }, Group.General),
                    new SettingsViewInfo(Name.Autostart, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }, Group.General),
                    new SettingsViewInfo(Name.Settings, new PackIconOcticons() { Kind = PackIconOcticonsKind.Settings }, Group.General),
                    new SettingsViewInfo(Name.ImportExport, new PackIconMaterial() { Kind = PackIconMaterialKind.Import}, Group.General),
                    new SettingsViewInfo(Name.IPScanner, new PackIconMaterial() {Kind = PackIconMaterialKind.Sitemap }, Group.Applications),
                    new SettingsViewInfo(Name.PortScanner, new PackIconModern() {Kind = PackIconModernKind.NetworkPort }, Group.Applications),
                    new SettingsViewInfo(Name.Ping, new PackIconMaterial() { Kind = PackIconMaterialKind.LanConnect }, Group.Applications),
                    new SettingsViewInfo(Name.Traceroute, new PackIconModern() {Kind = PackIconModernKind.TransitConnection}, Group.Applications),
                    new SettingsViewInfo(Name.DNSLookup, new PackIconMaterial() { Kind= PackIconMaterialKind.Dna }, Group.Applications ),
                    new SettingsViewInfo(Name.RemoteDesktop, new PackIconOcticons() { Kind = PackIconOcticonsKind.DeviceDesktop}, Group.Applications),
                    new SettingsViewInfo(Name.WakeOnLAN, new PackIconMaterial() {Kind = PackIconMaterialKind.Power} , Group.Applications)
                };
            }
        }

        public enum Name
        {
            Window,
            General,
            Appearance,
            Language,
            HotKeys,
            Autostart,
            Settings,
            ImportExport,
            IPScanner,
            PortScanner,
            Ping,
            Traceroute,
            DNSLookup,
            RemoteDesktop,
            WakeOnLAN
        }

        public static string TranslateName(Name name, Group group)
        {
            switch(group)
            {
                case Group.General:
                    return Application.Current.Resources["String_SettingsName_" + name] as string;
                case Group.Applications:
                    return Application.Current.Resources["String_ApplicationName_" + name] as string;
                default:
                    return "Not found!";
            }
        }

        public enum Group
        {
            General,
            Applications
        }

        public static string TranslateGroup(Group group)
        {
            return Application.Current.Resources["String_SettingsGroup_" + group] as string;
        }
    }
}
