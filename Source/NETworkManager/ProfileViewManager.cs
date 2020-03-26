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
            new ProfileViewInfo(Name.NetworkInterface, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.NetworkInterface)),
            new ProfileViewInfo(Name.IPScanner, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.IPScanner)),
            new ProfileViewInfo(Name.PortScanner, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PortScanner)),
            new ProfileViewInfo(Name.Ping, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Ping)),
            new ProfileViewInfo(Name.PingMonitor, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PingMonitor)),
            new ProfileViewInfo(Name.Traceroute, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Traceroute)),
            new ProfileViewInfo(Name.DNSLookup, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.DNSLookup)),
            new ProfileViewInfo(Name.RemoteDesktop, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.RemoteDesktop)),
            new ProfileViewInfo(Name.PowerShell, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PowerShell)),
            new ProfileViewInfo(Name.PuTTY, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.PuTTY)),
            new ProfileViewInfo(Name.TigerVNC, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.TigerVNC)),
            new ProfileViewInfo(Name.WebConsole, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.WebConsole)),
            new ProfileViewInfo(Name.WakeOnLAN, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.WakeOnLAN)),
            new ProfileViewInfo(Name.HTTPHeaders, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.HTTPHeaders)),
            new ProfileViewInfo(Name.Whois, Models.Application.ApplicationManager.GetIcon(Models.Application.ApplicationName.Whois))
        };

        public static string TranslateName(Name name)
        {
            switch (name)
            {
                case Name.General:
                    return Localization.Resources.Strings.General;
                case Name.NetworkInterface:
                    return Localization.Resources.Strings.NetworkInterface;
                case Name.IPScanner:
                    return Localization.Resources.Strings.IPScanner;
                case Name.PortScanner:
                    return Localization.Resources.Strings.PortScanner;
                case Name.Ping:
                    return Localization.Resources.Strings.Ping;
                case Name.PingMonitor:
                    return Localization.Resources.Strings.PingMonitor;
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
                case Name.WebConsole:
                    return Localization.Resources.Strings.WebConsole;
                case Name.WakeOnLAN:
                    return Localization.Resources.Strings.WakeOnLAN;
                case Name.HTTPHeaders:
                    return Localization.Resources.Strings.HTTPHeaders;
                case Name.Whois:
                    return Localization.Resources.Strings.Whois;
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
