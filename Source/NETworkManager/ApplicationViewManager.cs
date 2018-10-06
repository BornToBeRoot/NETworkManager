using System;
using System.Collections.Generic;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace NETworkManager
{
    public static class ApplicationViewManager
    {
        // List of all applications
        public static List<ApplicationViewInfo> GetList()
        {
            var list = new List<ApplicationViewInfo>();

            foreach (Name name in Enum.GetValues(typeof(Name)))
            {
                if (name != Name.None)
                    list.Add(new ApplicationViewInfo(name));
            }

            return list;
        }

        public static string GetTranslatedNameByName(Name name)
        {
            switch (name)
            {
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
                case Name.PuTTY:
                    return Resources.Localization.Strings.PuTTY;
                case Name.SNMP:
                    return Resources.Localization.Strings.SNMP;
                case Name.WakeOnLAN:
                    return Resources.Localization.Strings.WakeOnLAN;
                case Name.HTTPHeaders:
                    return Resources.Localization.Strings.HTTPHeaders;
                case Name.Whois:
                    return Resources.Localization.Strings.Whois;
                case Name.SubnetCalculator:
                    return Resources.Localization.Strings.SubnetCalculator;
                case Name.Lookup:
                    return Resources.Localization.Strings.Lookup;
                //case Name.Routing:
                //    return Resources.Localization.Strings.Routing;
                case Name.Connections:
                    return Resources.Localization.Strings.Connections;
                case Name.Listeners:
                    return Resources.Localization.Strings.Listeners;
                case Name.ARPTable:
                    return Resources.Localization.Strings.ARPTable;
                default:
                    return "Name not found!";
            }
        }

        public static Canvas GetIconByName(Name name)
        {
            var canvas = new Canvas();

            switch (name)
            {
                case Name.NetworkInterface:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Network });
                    break;
                case Name.IPScanner:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Sitemap });
                    break;
                case Name.PortScanner:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.NetworkPort });
                    break;
                case Name.Ping:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.LanConnect });
                    break;
                case Name.Traceroute:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.TransitConnection });
                    break;
                case Name.DNSLookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.SearchWeb });
                    break;
                case Name.RemoteDesktop:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.RemoteDesktop });
                    break;
                case Name.PuTTY:
                    canvas.Children.Add(new PackIconOcticons { Kind = PackIconOcticonsKind.Terminal });
                    break;
                case Name.SNMP:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Switch });
                    break;
                case Name.WakeOnLAN:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Power });
                    break;
                case Name.HTTPHeaders:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Web });
                    break;
                case Name.Whois:
                    canvas.Children.Add(new PackIconMaterial {Kind = PackIconMaterialKind.CloudSearchOutline});
                    break;
                case Name.SubnetCalculator:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Calculator });
                    break;
                case Name.Lookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Magnify });
                    break;
                //case Name.Routing:
                //    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Routes });
                //    break;
                case Name.Connections:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Connect });
                    break;
                case Name.Listeners:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Wan });
                    break;
                case Name.ARPTable:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.TableOfContents });
                    break;
                default:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.SmileyFrown });
                    break;
            }

            return canvas;
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
            Whois,
            SubnetCalculator,
            Lookup,
            //Routing,
            Connections,
            Listeners,
            ARPTable
        }
    }
}
