using MahApps.Metro.IconPacks;
using System.Collections.Generic;

namespace NETworkManager
{
    public static class ProfileViewManager
    {
        // List of all applications
        public static List<ProfileViewInfo> List => new List<ProfileViewInfo>
        {
            // General
            new ProfileViewInfo(Name.General, new PackIconModern{ Kind = PackIconModernKind.Box }),

            // Applications
            new ProfileViewInfo(Name.NetworkInterface, ApplicationViewManager.GetIconByName(Models.Application.Name.NetworkInterface)),
            new ProfileViewInfo(Name.IPScanner, ApplicationViewManager.GetIconByName(Models.Application.Name.IPScanner)),
            new ProfileViewInfo(Name.PortScanner,ApplicationViewManager.GetIconByName(Models.Application.Name.PortScanner)),
            new ProfileViewInfo(Name.Ping, ApplicationViewManager.GetIconByName(Models.Application.Name.Ping)),
            new ProfileViewInfo(Name.PingMonitor, ApplicationViewManager.GetIconByName(Models.Application.Name.PingMonitor)),
            new ProfileViewInfo(Name.Traceroute, ApplicationViewManager.GetIconByName(Models.Application.Name.Traceroute)),
            new ProfileViewInfo(Name.DNSLookup, ApplicationViewManager.GetIconByName(Models.Application.Name.DNSLookup)),
            new ProfileViewInfo(Name.RemoteDesktop, ApplicationViewManager.GetIconByName(Models.Application.Name.RemoteDesktop)),
            new ProfileViewInfo(Name.PowerShell, ApplicationViewManager.GetIconByName(Models.Application.Name.PowerShell)),
            new ProfileViewInfo(Name.PuTTY, ApplicationViewManager.GetIconByName(Models.Application.Name.PuTTY)),
            new ProfileViewInfo(Name.TigerVNC, ApplicationViewManager.GetIconByName(Models.Application.Name.TigerVNC)),
            new ProfileViewInfo(Name.WebConsole, ApplicationViewManager.GetIconByName(Models.Application.Name.WebConsole)),
            new ProfileViewInfo(Name.WakeOnLAN, ApplicationViewManager.GetIconByName(Models.Application.Name.WakeOnLAN)),
            new ProfileViewInfo(Name.HTTPHeaders, ApplicationViewManager.GetIconByName(Models.Application.Name.HTTPHeaders)),
            new ProfileViewInfo(Name.Whois, ApplicationViewManager.GetIconByName(Models.Application.Name.Whois))
        };

        public static string TranslateName(Name name)
        {
            switch (name)
            {
                case Name.General:
                    return Localization.LanguageFiles.Strings.General;
                case Name.NetworkInterface:
                    return Localization.LanguageFiles.Strings.NetworkInterface;
                case Name.IPScanner:
                    return Localization.LanguageFiles.Strings.IPScanner;
                case Name.PortScanner:
                    return Localization.LanguageFiles.Strings.PortScanner;
                case Name.Ping:
                    return Localization.LanguageFiles.Strings.Ping;
                case Name.PingMonitor:
                    return Localization.LanguageFiles.Strings.PingMonitor;
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
                case Name.WebConsole:
                    return Localization.LanguageFiles.Strings.WebConsole;
                case Name.WakeOnLAN:
                    return Localization.LanguageFiles.Strings.WakeOnLAN;
                case Name.HTTPHeaders:
                    return Localization.LanguageFiles.Strings.HTTPHeaders;
                case Name.Whois:
                    return Localization.LanguageFiles.Strings.Whois;
                default:
                    return "Translation of name not found";
            }
        }

        public enum Name
        {
            General,
            NetworkInterface,
            IPScanner,
            PortScanner,
            Ping,
            PingMonitor,
            Traceroute,
            DNSLookup,
            RemoteDesktop,
            PowerShell,
            PuTTY,
            TigerVNC,
            WebConsole,
            WakeOnLAN,
            HTTPHeaders,
            Whois
        }
    }
}
