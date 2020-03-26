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
            new ProfileViewInfo(Name.NetworkInterface, Models.Application.Application.GetIcon(Models.Application.Name.NetworkInterface)),
            new ProfileViewInfo(Name.IPScanner, Models.Application.Application.GetIcon(Models.Application.Name.IPScanner)),
            new ProfileViewInfo(Name.PortScanner, Models.Application.Application.GetIcon(Models.Application.Name.PortScanner)),
            new ProfileViewInfo(Name.Ping, Models.Application.Application.GetIcon(Models.Application.Name.Ping)),
            new ProfileViewInfo(Name.PingMonitor, Models.Application.Application.GetIcon(Models.Application.Name.PingMonitor)),
            new ProfileViewInfo(Name.Traceroute, Models.Application.Application.GetIcon(Models.Application.Name.Traceroute)),
            new ProfileViewInfo(Name.DNSLookup, Models.Application.Application.GetIcon(Models.Application.Name.DNSLookup)),
            new ProfileViewInfo(Name.RemoteDesktop, Models.Application.Application.GetIcon(Models.Application.Name.RemoteDesktop)),
            new ProfileViewInfo(Name.PowerShell, Models.Application.Application.GetIcon(Models.Application.Name.PowerShell)),
            new ProfileViewInfo(Name.PuTTY, Models.Application.Application.GetIcon(Models.Application.Name.PuTTY)),
            new ProfileViewInfo(Name.TigerVNC, Models.Application.Application.GetIcon(Models.Application.Name.TigerVNC)),
            new ProfileViewInfo(Name.WebConsole, Models.Application.Application.GetIcon(Models.Application.Name.WebConsole)),
            new ProfileViewInfo(Name.WakeOnLAN, Models.Application.Application.GetIcon(Models.Application.Name.WakeOnLAN)),
            new ProfileViewInfo(Name.HTTPHeaders, Models.Application.Application.GetIcon(Models.Application.Name.HTTPHeaders)),
            new ProfileViewInfo(Name.Whois, Models.Application.Application.GetIcon(Models.Application.Name.Whois))
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
