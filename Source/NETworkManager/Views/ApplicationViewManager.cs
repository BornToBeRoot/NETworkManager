using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.Views
{
    public static class ApplicationViewManager
    {
        // List of all applications
        public static List<ApplicationViewInfo> List
        {
            get
            {
                return new List<ApplicationViewInfo>
                {
                    new ApplicationViewInfo(Name.NetworkInterface, new PackIconModern() { Kind = PackIconModernKind.Network }, false),
                    new ApplicationViewInfo(Name.IPScanner, new PackIconMaterial() { Kind = PackIconMaterialKind.Sitemap }, true),
                    new ApplicationViewInfo(Name.PortScanner, new PackIconModern() { Kind = PackIconModernKind.NetworkPort}, true),
                    new ApplicationViewInfo(Name.SubnetCalculator, new PackIconMaterial() { Kind = PackIconMaterialKind.Calculator }, false),
                    new ApplicationViewInfo(Name.WakeOnLAN, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }, false),
                    new ApplicationViewInfo(Name.Ping, new PackIconMaterial() { Kind = PackIconMaterialKind.LanConnect }, true),
                    new ApplicationViewInfo(Name.Traceroute,  new PackIconModern() { Kind = PackIconModernKind.TransitConnection }, true),
                    new ApplicationViewInfo(Name.DNSLookup, new PackIconMaterial() { Kind= PackIconMaterialKind.Dna }, true, true),
                    new ApplicationViewInfo(Name.Lookup, new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.Book }, false)
                };
            }
        }

        public static string TranslateName(Name name)
        {
            return Application.Current.Resources["String_ApplicationName_" + name] as string;
        }

        public enum Name
        {
            None,
            NetworkInterface,
            IPScanner,
            WakeOnLAN,
            Traceroute,
            SubnetCalculator,
            Ping,
            DNSLookup,
            PortScanner,
            Lookup
        }
    }
}
