using MahApps.Metro.IconPacks;
using System.Collections.Generic;
using System.Windows;

namespace NETworkManager.Views
{
    public static class ApplicationView
    {
        // List of all applications
        public static List<ApplicationViewInfo> List
        {
            get
            {
                return new List<ApplicationViewInfo>
                {
                    new ApplicationViewInfo(Name.NetworkInterface, new PackIconModern() { Kind = PackIconModernKind.Network }),
                    new ApplicationViewInfo(Name.IPScanner, new PackIconModern() { Kind = PackIconModernKind.Diagram }),
                    new ApplicationViewInfo(Name.PortScanner, new PackIconModern() { Kind = PackIconModernKind.NetworkPort}, true),
                    new ApplicationViewInfo(Name.SubnetCalculator, new PackIconModern() { Kind = PackIconModernKind.Calculator }),
                    new ApplicationViewInfo(Name.WakeOnLAN, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }),
                    new ApplicationViewInfo(Name.Ping, new PackIconModern() { Kind = PackIconModernKind.Console }),
                    new ApplicationViewInfo(Name.Traceroute,  new PackIconMaterial() { Kind = PackIconMaterialKind.Routes }),
                    new ApplicationViewInfo(Name.DNSLookup, new PackIconMaterial() { Kind= PackIconMaterialKind.Dna }, true),
                    new ApplicationViewInfo(Name.OUILookup, new PackIconMaterial() {Kind = PackIconMaterialKind.Magnify })
                };
            }
        }

        public static string TranslateName(Name name)
        {
            return Application.Current.Resources["String_ApplicationName_" + name] as string;
        }

        public enum Name
        {
            NetworkInterface,
            IPScanner,
            WakeOnLAN,
            Traceroute,
            SubnetCalculator,
            Ping,
            DNSLookup,
            PortScanner,
            OUILookup
        }
    }
}
