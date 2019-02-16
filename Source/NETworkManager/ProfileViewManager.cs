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
            new ProfileViewInfo(Name.NetworkInterface, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.NetworkInterface)),
            new ProfileViewInfo(Name.IPScanner, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.IPScanner)),
            new ProfileViewInfo(Name.PortScanner,ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PortScanner)),
            new ProfileViewInfo(Name.Ping, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Ping)),
            new ProfileViewInfo(Name.Traceroute, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Traceroute)),
            new ProfileViewInfo(Name.DNSLookup, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.DNSLookup)),
            new ProfileViewInfo(Name.RemoteDesktop, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.RemoteDesktop)),
            new ProfileViewInfo(Name.PowerShell, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PowerShell)),
            new ProfileViewInfo(Name.PuTTY, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.PuTTY)),
            new ProfileViewInfo(Name.TigerVNC, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.TigerVNC)),
            new ProfileViewInfo(Name.WakeOnLAN, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.WakeOnLAN)),
            new ProfileViewInfo(Name.HTTPHeaders, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.HTTPHeaders)),
            new ProfileViewInfo(Name.Whois, ApplicationViewManager.GetIconByName(ApplicationViewManager.Name.Whois))
        };

        public static string TranslateName(Name name)
        {
            switch (name)
            {
                case Name.General:
                    return Resources.Localization.Strings.General;
                case Name.NetworkInterface:
                    return Resources.Localization.Strings.NetworkInterface;
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
                case Name.TigerVNC:
                    return Resources.Localization.Strings.TigerVNC;
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

        public enum Name
        {
            General,
            NetworkInterface,
            IPScanner,
            PortScanner,
            Ping,
            Traceroute,
            DNSLookup,
            RemoteDesktop,
            PowerShell,
            PuTTY,
            TigerVNC,
            WakeOnLAN,
            HTTPHeaders,
            Whois
        }
    }
}
