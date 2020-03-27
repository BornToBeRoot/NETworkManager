using MahApps.Metro.IconPacks;
using NETworkManager.Models;
using System.Collections.Generic;

namespace NETworkManager.Profiles
{
    public static class ProfileViewManager
    {
        // List of all applications
        public static List<ProfileViewInfo> List => new List<ProfileViewInfo>
        {
            // General
            new ProfileViewInfo(ProfileViewName.General, new PackIconModern{ Kind = PackIconModernKind.Box }),

            // Applications
            new ProfileViewInfo(ProfileViewName.NetworkInterface, ApplicationManager.GetIcon(ApplicationName.NetworkInterface)),
            new ProfileViewInfo(ProfileViewName.IPScanner, ApplicationManager.GetIcon(ApplicationName.IPScanner)),
            new ProfileViewInfo(ProfileViewName.PortScanner, ApplicationManager.GetIcon(ApplicationName.PortScanner)),
            new ProfileViewInfo(ProfileViewName.Ping, ApplicationManager.GetIcon(ApplicationName.Ping)),
            new ProfileViewInfo(ProfileViewName.PingMonitor, ApplicationManager.GetIcon(ApplicationName.PingMonitor)),
            new ProfileViewInfo(ProfileViewName.Traceroute, ApplicationManager.GetIcon(ApplicationName.Traceroute)),
            new ProfileViewInfo(ProfileViewName.DNSLookup, ApplicationManager.GetIcon(ApplicationName.DNSLookup)),
            new ProfileViewInfo(ProfileViewName.RemoteDesktop, ApplicationManager.GetIcon(ApplicationName.RemoteDesktop)),
            new ProfileViewInfo(ProfileViewName.PowerShell, ApplicationManager.GetIcon(ApplicationName.PowerShell)),
            new ProfileViewInfo(ProfileViewName.PuTTY, ApplicationManager.GetIcon(ApplicationName.PuTTY)),
            new ProfileViewInfo(ProfileViewName.TigerVNC, ApplicationManager.GetIcon(ApplicationName.TigerVNC)),
            new ProfileViewInfo(ProfileViewName.WebConsole, ApplicationManager.GetIcon(ApplicationName.WebConsole)),
            new ProfileViewInfo(ProfileViewName.WakeOnLAN, ApplicationManager.GetIcon(ApplicationName.WakeOnLAN)),
            new ProfileViewInfo(ProfileViewName.HTTPHeaders, ApplicationManager.GetIcon(ApplicationName.HTTPHeaders)),
            new ProfileViewInfo(ProfileViewName.Whois, ApplicationManager.GetIcon(ApplicationName.Whois))
        };

        /*
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
        */
    }
}
