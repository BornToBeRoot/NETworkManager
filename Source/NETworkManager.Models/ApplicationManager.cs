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
    public static class ApplicationManager
    {
        /// <summary>
        /// Method to return all available applications in.
        /// </summary>
        /// <returns>All names as array.</returns>
        public static ApplicationName[] GetNames() => (ApplicationName[])Enum.GetValues(typeof(ApplicationName));

        /// <summary>
        /// Method returns a list with all <see cref="ApplicationInfo"/>.
        /// </summary>
        /// <returns>IEnumerable with <see cref="ApplicationInfo"/></returns>
        public static IEnumerable<ApplicationInfo> GetList()
        {
            var list = new List<ApplicationInfo>();

            foreach (var name in GetNames().Where(x => x != ApplicationName.None))
                list.Add(new ApplicationInfo(name));

            return list;
        }

        /// <summary>
        /// Method will return the icon based on <see cref="ApplicationName"/>.
        /// </summary>
        /// <param name="name"><see cref="ApplicationName"/></param>
        /// <returns>Application icon as <see cref="Canvas"/>.</returns>
        public static Canvas GetIcon(ApplicationName name)
        {
            var canvas = new Canvas();

            switch (name)
            {
                case ApplicationName.Dashboard:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.ViewDashboardVariant });
                    break;
                case ApplicationName.NetworkInterface:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Network });
                    break;
                case ApplicationName.WiFi:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.AccessPointNetwork });
                    break;
                case ApplicationName.IPScanner:
                    canvas.Children.Add(new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.NetworkWiredSolid });
                    break;
                case ApplicationName.PortScanner:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.NetworkPort });
                    break;
                case ApplicationName.PingMonitor:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.RadarScreen });
                    break;
                case ApplicationName.Traceroute:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.TransitConnection });
                    break;
                case ApplicationName.DNSLookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.SearchWeb });
                    break;
                case ApplicationName.RemoteDesktop:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.RemoteDesktop });
                    break;
                case ApplicationName.PowerShell:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Powershell });
                    break;
                case ApplicationName.PuTTY:
                    canvas.Children.Add(new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.TerminalSolid });
                    break;
                case ApplicationName.AWSSessionManager:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Aws });
                    break;
                case ApplicationName.TigerVNC:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.EyeOutline });
                    break;
                case ApplicationName.WebConsole:
                    canvas.Children.Add(new PackIconPicolIcons { Kind = PackIconPicolIconsKind.Website });
                    break;
                case ApplicationName.SNMP:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Switch });
                    break;
                case ApplicationName.DiscoveryProtocol:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.SwapHorizontal });
                    break;
                case ApplicationName.WakeOnLAN:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Power });
                    break;
                case ApplicationName.Whois:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.CloudSearchOutline });
                    break;
                case ApplicationName.SubnetCalculator:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Calculator });
                    break;
                case ApplicationName.BitCalculator:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.TypeBit });
                    break;
                case ApplicationName.Lookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.DatabaseSearch });
                    break;
                case ApplicationName.Connections:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Connect });
                    break;
                case ApplicationName.Listeners:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Wan });
                    break;
                case ApplicationName.ARPTable:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.TableOfContents });
                    break;
                case ApplicationName.None:
                default:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.SmileyFrown });
                    break;
            }

            return canvas;
        }
    }
}
