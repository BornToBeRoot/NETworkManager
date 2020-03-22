using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;
using NETworkManager.Models;

namespace NETworkManager
{
    public static class ApplicationViewManager
    {
        // List of all applications
        public static List<ApplicationViewInfo> GetList()
        {
            var list = new List<ApplicationViewInfo>();

            foreach (var name in Application.GetNames().Where(x => x != Application.Name.None))
                list.Add(new ApplicationViewInfo(name));

            return list;
        }

        public static string GetTranslatedNameByName(Application.Name name)
        {
            switch (name)
            {
                case Application.Name.Dashboard:
                    return Localization.LanguageFiles.Strings.Dashboard;
                case Application.Name.NetworkInterface:
                    return Localization.LanguageFiles.Strings.NetworkInterface;
                case Application.Name.WiFi:
                    return Localization.LanguageFiles.Strings.WiFi;
                case Application.Name.IPScanner:
                    return Localization.LanguageFiles.Strings.IPScanner;
                case Application.Name.PortScanner:
                    return Localization.LanguageFiles.Strings.PortScanner;
                case Application.Name.Ping:
                    return Localization.LanguageFiles.Strings.Ping;
                case Application.Name.PingMonitor:
                    return Localization.LanguageFiles.Strings.PingMonitor;
                case Application.Name.Traceroute:
                    return Localization.LanguageFiles.Strings.Traceroute;
                case Application.Name.DNSLookup:
                    return Localization.LanguageFiles.Strings.DNSLookup;
                case Application.Name.RemoteDesktop:
                    return Localization.LanguageFiles.Strings.RemoteDesktop;
                case Application.Name.PowerShell:
                    return Localization.LanguageFiles.Strings.PowerShell;
                case Application.Name.PuTTY:
                    return Localization.LanguageFiles.Strings.PuTTY;
                case Application.Name.TigerVNC:
                    return Localization.LanguageFiles.Strings.TigerVNC;
                case Application.Name.WebConsole:
                    return Localization.LanguageFiles.Strings.WebConsole;
                case Application.Name.SNMP:
                    return Localization.LanguageFiles.Strings.SNMP;
                case Application.Name.DiscoveryProtocol:
                    return Localization.LanguageFiles.Strings.DiscoveryProtocol;
                case Application.Name.WakeOnLAN:
                    return Localization.LanguageFiles.Strings.WakeOnLAN;
                case Application.Name.HTTPHeaders:
                    return Localization.LanguageFiles.Strings.HTTPHeaders;
                case Application.Name.Whois:
                    return Localization.LanguageFiles.Strings.Whois;
                case Application.Name.SubnetCalculator:
                    return Localization.LanguageFiles.Strings.SubnetCalculator;
                case Application.Name.Lookup:
                    return Localization.LanguageFiles.Strings.Lookup;
                case Application.Name.Connections:
                    return Localization.LanguageFiles.Strings.Connections;
                case Application.Name.Listeners:
                    return Localization.LanguageFiles.Strings.Listeners;
                case Application.Name.ARPTable:
                    return Localization.LanguageFiles.Strings.ARPTable;
                default:
                    return "Name not found!";
            }
        }

        public static Canvas GetIconByName(Application.Name name)
        {
            var canvas = new Canvas();

            switch (name)
            {
                case Application.Name.Dashboard:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.ViewDashboardVariant });
                    break;
                case Application.Name.NetworkInterface:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Network });
                    break;
                case Application.Name.WiFi:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.AccessPointNetwork });
                    break;
                case Application.Name.IPScanner:
                    canvas.Children.Add(new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.NetworkWiredSolid });
                    break;
                case Application.Name.PortScanner:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.NetworkPort });
                    break;
                case Application.Name.Ping:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.LanConnect });
                    break;
                case Application.Name.PingMonitor:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.RadarScreen });
                    break;
                case Application.Name.Traceroute:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.TransitConnection });
                    break;
                case Application.Name.DNSLookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.SearchWeb });
                    break;
                case Application.Name.RemoteDesktop:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.RemoteDesktop });
                    break;
                case Application.Name.PowerShell:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Powershell });
                    break;
                case Application.Name.PuTTY:
                    canvas.Children.Add(new PackIconFontAwesome { Kind = PackIconFontAwesomeKind.TerminalSolid });
                    break;
                case Application.Name.TigerVNC:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.EyeOutline });
                    break;
                case Application.Name.WebConsole:
                    canvas.Children.Add(new PackIconPicolIcons { Kind = PackIconPicolIconsKind.Website });
                    break;
                case Application.Name.SNMP:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Switch });
                    break;
                case Application.Name.DiscoveryProtocol:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.SwapHorizontal });
                    break;
                case Application.Name.WakeOnLAN:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Power });
                    break;
                case Application.Name.HTTPHeaders:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Web });
                    break;
                case Application.Name.Whois:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.CloudSearchOutline });
                    break;
                case Application.Name.SubnetCalculator:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Calculator });
                    break;
                case Application.Name.Lookup:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.DatabaseSearch });
                    break;
                case Application.Name.Connections:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.Connect });
                    break;
                case Application.Name.Listeners:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.Wan });
                    break;
                case Application.Name.ARPTable:
                    canvas.Children.Add(new PackIconMaterial { Kind = PackIconMaterialKind.TableOfContents });
                    break;
                default:
                    canvas.Children.Add(new PackIconModern { Kind = PackIconModernKind.SmileyFrown });
                    break;
            }

            return canvas;
        }
    }
}
