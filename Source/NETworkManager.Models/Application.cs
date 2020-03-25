using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace NETworkManager.Models
{
    /// <summary>
    /// Provides methods to manage networkmanger applications.
    /// </summary>
    public static partial class Application
    {
        /// <summary>
        /// Method to return all available applications in.
        /// </summary>
        /// <returns>All names as array.</returns>
        public static Name[] GetNames() => (Name[])Enum.GetValues(typeof(Name));

        /// <summary>
        /// Method returns a list with all <see cref="ApplicationInfo"/>.
        /// </summary>
        /// <returns>IEnumerable with <see cref="ApplicationInfo"/></returns>
        public static IEnumerable<ApplicationInfo> GetList()
        {
            var list = new List<ApplicationInfo>();

            foreach (var name in GetNames().Where(x => x != Name.None))
                list.Add(new ApplicationInfo(name));
            
            return list;
        }

        /// <summary>
        /// Method will return the icon based on <see cref="Name"/>.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <returns>Application icon as <see cref="Canvas"/>.</returns>
        public static Canvas GetIcon(Name name)
        {
            var canvas = new Canvas();

            switch (name)
            {
                case Name.Dashboard:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.ViewDashboardVariant });
                    break;
                case Name.NetworkInterface:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Network });
                    break;
                case Name.WiFi:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.AccessPointNetwork });
                    break;
                case Name.IPScanner:
                    canvas.Children.Add(new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.NetworkWiredSolid });
                    break;
                case Name.PortScanner:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.NetworkPort });
                    break;
                case Name.Ping:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.LanConnect });
                    break;
                case Name.PingMonitor:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.RadarScreen });
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
                case Name.PowerShell:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Powershell });
                    break;
                case Name.PuTTY:
                    canvas.Children.Add(new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.TerminalSolid });
                    break;
                case Name.TigerVNC:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.EyeOutline });
                    break;
                case Name.WebConsole:
                    canvas.Children.Add(new PackIconPicolIcons { Kind = PackIconPicolIconsKind.Website });
                    break;
                case Name.SNMP:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Switch });
                    break;
                case Name.DiscoveryProtocol:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.SwapHorizontal });
                    break;
                case Name.WakeOnLAN:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Power });
                    break;
                case Name.HTTPHeaders:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Web });
                    break;
                case Name.Whois:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.CloudSearchOutline });
                    break;
                case Name.SubnetCalculator:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Calculator });
                    break;
                case Name.Lookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.DatabaseSearch });
                    break;
                case Name.Connections:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Connect });
                    break;
                case Name.Listeners:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Wan });
                    break;
                case Name.ARPTable:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.TableOfContents });
                    break;
                case Name.None:
                default:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.SmileyFrown });
                    break;                    
            }

            return canvas;
        }
    }
}
