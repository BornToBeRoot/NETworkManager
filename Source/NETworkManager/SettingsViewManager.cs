using MahApps.Metro.IconPacks;
using NETworkManager.Models.Settings;
using System.Collections.Generic;

namespace NETworkManager
{
    public static class SettingsViewManager
    {
        // List of all applications
        public static List<SettingsViewInfo> List => new List<SettingsViewInfo>
        {
            // General
            new SettingsViewInfo(Name.General, new PackIconModern() { Kind = PackIconModernKind.Box }, Group.General),
            new SettingsViewInfo(Name.Window, new PackIconMaterial() {Kind = PackIconMaterialKind.Application }, Group.General),
            new SettingsViewInfo(Name.Appearance, new PackIconMaterial() { Kind = PackIconMaterialKind.AutoFix }, Group.General),
            new SettingsViewInfo(Name.Language, new PackIconMaterial() { Kind = PackIconMaterialKind.Flag}, Group.General),
            new SettingsViewInfo(Name.HotKeys, new PackIconOcticons() { Kind = PackIconOcticonsKind.Keyboard }, Group.General),
            new SettingsViewInfo(Name.Autostart, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }, Group.General),
            new SettingsViewInfo(Name.Update, new PackIconMaterial() {Kind = PackIconMaterialKind.Download }, Group.General),
            new SettingsViewInfo(Name.ImportExport, new PackIconMaterial() { Kind = PackIconMaterialKind.Import}, Group.General),
            new SettingsViewInfo(Name.Settings, new PackIconMaterialLight() { Kind = PackIconMaterialLightKind.Cog }, Group.General),

            // Applications
            new SettingsViewInfo(Name.IPScanner, new PackIconMaterial() {Kind = PackIconMaterialKind.Sitemap }, Group.Applications),
            new SettingsViewInfo(Name.PortScanner, new PackIconModern() {Kind = PackIconModernKind.NetworkPort }, Group.Applications),
            new SettingsViewInfo(Name.Ping, new PackIconMaterial() { Kind = PackIconMaterialKind.LanConnect }, Group.Applications),
            new SettingsViewInfo(Name.Traceroute, new PackIconModern() {Kind = PackIconModernKind.TransitConnection}, Group.Applications),
            new SettingsViewInfo(Name.DNSLookup, new PackIconMaterial() { Kind= PackIconMaterialKind.SearchWeb }, Group.Applications ),
            new SettingsViewInfo(Name.RemoteDesktop, new PackIconMaterial() { Kind = PackIconMaterialKind.RemoteDesktop}, Group.Applications),
            new SettingsViewInfo(Name.PuTTY, new PackIconOcticons() {Kind = PackIconOcticonsKind.Terminal}, Group.Applications),
            new SettingsViewInfo(Name.SNMP,new PackIconMaterial() {Kind = PackIconMaterialKind.Switch }, Group.Applications),
            new SettingsViewInfo(Name.WakeOnLAN, new PackIconMaterial() {Kind = PackIconMaterialKind.Power} , Group.Applications),
            new SettingsViewInfo(Name.HTTPHeaders, new PackIconMaterial() {Kind = PackIconMaterialKind.Web}, Group.Applications)
        };

        public static string TranslateName(Name name, Group group)
        {
            switch (group)
            {
                case Group.General:
                    return LocalizationManager.GetStringByKey("String_SettingsName_" + name);
                case Group.Applications:
                    return LocalizationManager.GetStringByKey("String_ApplicationName_" + name);
                default:
                    return "Not found!";
            }
        }

        public static string TranslateGroup(Group group)
        {
            return LocalizationManager.GetStringByKey("String_SettingsGroup_" + group);
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
            HTTPHeaders
        }

        public enum Group
        {
            General,
            Applications
        }
    }
}
