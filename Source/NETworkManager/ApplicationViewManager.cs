using MahApps.Metro.IconPacks;
using NETworkManager.Models.Settings;
using System.Collections.Generic;

namespace NETworkManager
{
    public static class ApplicationViewManager
    {
        // List of all applications
        public static List<ApplicationViewInfo> List => new List<ApplicationViewInfo>
        {
            new ApplicationViewInfo(Name.NetworkInterface, new PackIconModern { Kind = PackIconModernKind.Network }),
            new ApplicationViewInfo(Name.IPScanner, new PackIconMaterial { Kind = PackIconMaterialKind.Sitemap }),
            new ApplicationViewInfo(Name.PortScanner, new PackIconModern { Kind = PackIconModernKind.NetworkPort}),
            new ApplicationViewInfo(Name.Ping, new PackIconMaterial { Kind = PackIconMaterialKind.LanConnect }),
            new ApplicationViewInfo(Name.Traceroute,  new PackIconModern { Kind = PackIconModernKind.TransitConnection }),
            new ApplicationViewInfo(Name.DNSLookup, new PackIconMaterial { Kind= PackIconMaterialKind.SearchWeb }),
            new ApplicationViewInfo(Name.RemoteDesktop, new PackIconMaterial{ Kind = PackIconMaterialKind.RemoteDesktop }),
            new ApplicationViewInfo(Name.PuTTY, new PackIconOcticons {Kind = PackIconOcticonsKind.Terminal }),
            new ApplicationViewInfo(Name.SNMP, new PackIconMaterial {Kind = PackIconMaterialKind.Switch }),
            new ApplicationViewInfo(Name.WakeOnLAN, new PackIconMaterial { Kind = PackIconMaterialKind.Power }),
            new ApplicationViewInfo(Name.HTTPHeaders, new PackIconMaterial { Kind = PackIconMaterialKind.Web }),
            new ApplicationViewInfo(Name.SubnetCalculator, new PackIconModern { Kind = PackIconModernKind.Calculator }),
            new ApplicationViewInfo(Name.Lookup, new PackIconMaterial { Kind = PackIconMaterialKind.Magnify }),
            new ApplicationViewInfo(Name.Connections, new PackIconModern {Kind = PackIconModernKind.Connect }),
            new ApplicationViewInfo(Name.Listeners, new PackIconMaterial {Kind = PackIconMaterialKind.Wan}),
            new ApplicationViewInfo(Name.ARPTable, new PackIconMaterial { Kind = PackIconMaterialKind.TableOfContents })
        };

        public static string GetTranslatedNameByName(Name name)
        {
            return LocalizationManager.GetStringByKey("String_ApplicationName_" + name);
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
            PuTTY,
            SNMP,
            WakeOnLAN,
            HTTPHeaders,
            SubnetCalculator,
            Lookup,
            Connections,
            Listeners,
            ARPTable
        }
    }
}
