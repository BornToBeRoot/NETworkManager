using NETworkManager.Models;

namespace NETworkManager
{
    public static class ApplicationViewManager
    {
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

       
    }
}
