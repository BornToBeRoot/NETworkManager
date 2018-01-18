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
                    new ApplicationViewInfo(Name.NetworkInterface, new PackIconModern() { Kind = PackIconModernKind.Network }),
                    new ApplicationViewInfo(Name.IPScanner, new PackIconMaterial() { Kind = PackIconMaterialKind.Sitemap }),
                    new ApplicationViewInfo(Name.PortScanner, new PackIconModern() { Kind = PackIconModernKind.NetworkPort}),
                    new ApplicationViewInfo(Name.Ping, new PackIconMaterial() { Kind = PackIconMaterialKind.LanConnect }),
                    new ApplicationViewInfo(Name.Traceroute,  new PackIconModern() { Kind = PackIconModernKind.TransitConnection }),
                    new ApplicationViewInfo(Name.DNSLookup, new PackIconMaterial() { Kind= PackIconMaterialKind.Dna }),
                    new ApplicationViewInfo(Name.RemoteDesktop, new PackIconOcticons() { Kind = PackIconOcticonsKind.DeviceDesktop}),                    
                    new ApplicationViewInfo(Name.SNMP, new PackIconMaterial() {Kind = PackIconMaterialKind.Switch }),
                    new ApplicationViewInfo(Name.WakeOnLAN, new PackIconMaterial() { Kind = PackIconMaterialKind.Power }),           
                    new ApplicationViewInfo(Name.HTTPHeaders, new PackIconMaterial() {Kind = PackIconMaterialKind.Web }),
                    new ApplicationViewInfo(Name.SubnetCalculator, new PackIconModern() { Kind = PackIconModernKind.Calculator }),
                    new ApplicationViewInfo(Name.Lookup, new PackIconMaterial() { Kind = PackIconMaterialKind.Magnify }),
                    new ApplicationViewInfo(Name.ARPTable, new PackIconMaterial() { Kind = PackIconMaterialKind.FormatListBulleted})
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
            PortScanner,
            Ping,
            Traceroute,
            DNSLookup,
            RemoteDesktop,            
            SNMP,
            WakeOnLAN,
            HTTPHeaders,
            SubnetCalculator,
            Lookup,
            ARPTable            
        }
    }
}
