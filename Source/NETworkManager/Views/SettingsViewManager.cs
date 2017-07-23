using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Linq;
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
                    new SettingsViewInfo(Name.Developer,  new PackIconOcticons() { Kind = PackIconOcticonsKind.Beaker }, Group.General),
                    new SettingsViewInfo(Name.IPScanner, new PackIconMaterial() {Kind = PackIconMaterialKind.Sitemap }, Group.Applications),
                    new SettingsViewInfo(Name.PortScanner, new PackIconModern() {Kind = PackIconModernKind.NetworkPort }, Group.Applications),
                    new SettingsViewInfo(Name.Ping, new PackIconMaterial() { Kind = PackIconMaterialKind.LanConnect }, Group.Applications),
                    new SettingsViewInfo(Name.Traceroute, new PackIconModern() {Kind = PackIconModernKind.TransitConnection}, Group.Applications)
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
            Developer,
            IPScanner,
            PortScanner,
            Ping,
            Traceroute
        }

        public static string TranslateName(Name name)
        {
            return Application.Current.Resources["String_SettingsName_" + name] as string;
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
