using MahApps.Metro.IconPacks;
using NETworkManager.Models;
using System.Collections.Generic;

namespace NETworkManager.Settings
{
    public static class SettingsViewManager
    {
        // List of all applications
        public static List<SettingsViewInfo> List => new List<SettingsViewInfo>
        {
            // General
            new SettingsViewInfo(SettingsViewName.General, new PackIconMaterial{ Kind = PackIconMaterialKind.Layers }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Window, new PackIconPicolIcons { Kind = PackIconPicolIconsKind.BrowserWindow }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Appearance, new PackIconMaterial { Kind = PackIconMaterialKind.Palette }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Language, new PackIconMaterial { Kind = PackIconMaterialKind.Translate }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Status, new PackIconModern { Kind = PackIconModernKind.Network }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.HotKeys, new PackIconOcticons { Kind = PackIconOcticonsKind.Keyboard }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Autostart, new PackIconMaterial { Kind = PackIconMaterialKind.Power }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Update, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Download }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Settings, new PackIconMaterialLight { Kind = PackIconMaterialLightKind.Cog }, SettingsViewGroup.General),
            new SettingsViewInfo(SettingsViewName.Profiles, new PackIconMaterial { Kind = PackIconMaterialKind.FormatListBulletedType }, SettingsViewGroup.General),
           
            // Applications
            new SettingsViewInfo(SettingsViewName.Dashboard, ApplicationManager.GetIcon(ApplicationName.Dashboard), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.Ping, ApplicationManager.GetIcon(ApplicationName.Ping), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup), SettingsViewGroup.Applications ),
            new SettingsViewInfo(SettingsViewName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.SNMP, ApplicationManager.GetIcon(ApplicationName.SNMP), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.HTTPHeaders, ApplicationManager.GetIcon(ApplicationName.HTTPHeaders), SettingsViewGroup.Applications),
            new SettingsViewInfo(SettingsViewName.Whois, ApplicationManager.GetIcon(ApplicationName.Whois), SettingsViewGroup.Applications)
        };

        /*
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
        */

            /*
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
        */
       

     
    }
}

