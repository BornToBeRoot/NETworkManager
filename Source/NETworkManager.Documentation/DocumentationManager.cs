using NETworkManager.Localization;
using NETworkManager.Models.Application;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.Documentation
{
    public static class DocumentationManager
    {
        public const string DocumentationBaseUrl = @"https://github.com/BornToBeRoot/NETworkManager/tree/master/Documentation/";

        public static string DocumentationUrl => $"{DocumentationBaseUrl}{LocalizationManager.GetInstance().Current.Code}/README.md";

        public static List<DocumentationInfo> List => new List<DocumentationInfo>
        {
            new DocumentationInfo(DocumentationIdentifier.ApplicationDashboard, @"/Application/Dashboard.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface, @"/Application/NetworkInterface.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWiFi, @"/Application/WiFi.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationIPScanner, @"/Application/IPScanner.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner, @"/Application/PortScanner.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPing, @"/Application/Ping.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPingMonitor, @"/Application/PingMonitor.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute, @"/Application/Traceroute.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup, @"/Application/DNSLookup.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop, @"/Application/RemoteDesktop.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPowerShell, @"/Application/PowerShell.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"/Application/PuTTY.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTigerVNC, @"/Application/TigerVNC.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWebConsole, @"/Application/WebConsole.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp, @"/Application/SNMP.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDiscoveryProtocol, @"/Application/DiscoveryProtocol.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan, @"/Application/WakeOnLAN.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationHttpHeaders, @"/Application/HTTPHeaders.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWhois, @"/Application/Whois.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator, @"/Application/SubnetCalculator.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationLookup, @"/Application/Lookup.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationConnections, @"/Application/Connections.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationListeners, @"/Application/Listeners.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable, @"/Application/ARPTable.md", GetLocalizationInfoList("en-US", "de-DE")),
        };
               
        // Get localized documentation url (if available), else return the english page
        public static string GetLocalizedUrlByIdentifier(DocumentationIdentifier documentationIdentifier)
        {
            var info = List.FirstOrDefault(x => x.Identifier == documentationIdentifier);

            // Return start page
            if (info == null)
                return DocumentationBaseUrl;

            // Try get localized help, fallback is english
            return info.Localizations.FirstOrDefault(x => x.Code == LocalizationManager.GetInstance().Current.Code) != null ? $"{DocumentationBaseUrl}{LocalizationManager.GetInstance().Current.Code}{info.Path}" : $"{DocumentationBaseUrl}en-US{info.Path}";
        }

        // Generate a list with culture codes
        private static List<LocalizationInfo> GetLocalizationInfoList(params string[] codes)
        {
            return codes.Select(code => new LocalizationInfo(code)).ToList();
        }

        public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
        {            
            Process.Start(documentationIdentifier == DocumentationIdentifier.Default ? DocumentationUrl : GetLocalizedUrlByIdentifier(documentationIdentifier));
        }

        #region ICommands & Actions
        public static ICommand OpenDocumentationCommand => new RelayCommand(OpenDocumentationAction);

        private static void OpenDocumentationAction(object documentationIdentifier)
        {
            if (documentationIdentifier != null)
                OpenDocumentation((DocumentationIdentifier) documentationIdentifier);
        }

        public static DocumentationIdentifier GetIdentifierByAppliactionName(ApplicationName name)
        {
            switch (name)
            {
                case ApplicationName.Dashboard:
                    return DocumentationIdentifier.ApplicationDashboard;                    
                case ApplicationName.NetworkInterface:
                    return DocumentationIdentifier.ApplicationNetworkInterface;                    
                case ApplicationName.WiFi:
                    return DocumentationIdentifier.ApplicationWiFi;                    
                case ApplicationName.IPScanner:
                    return DocumentationIdentifier.ApplicationIPScanner;                    
                case ApplicationName.PortScanner:
                    return DocumentationIdentifier.ApplicationPortScanner;                    
                case ApplicationName.Ping:
                    return DocumentationIdentifier.ApplicationPing;                    
                case ApplicationName.Traceroute:
                    return DocumentationIdentifier.ApplicationTraceroute;                    
                case ApplicationName.DNSLookup:
                    return DocumentationIdentifier.ApplicationDnsLookup;                    
                case ApplicationName.RemoteDesktop:
                    return DocumentationIdentifier.ApplicationRemoteDesktop;                    
                case ApplicationName.PowerShell:
                    return DocumentationIdentifier.ApplicationPowerShell;                    
                case ApplicationName.PuTTY:
                    return DocumentationIdentifier.ApplicationPutty;                    
                case ApplicationName.TigerVNC:
                    return DocumentationIdentifier.ApplicationTigerVNC;                    
                case ApplicationName.SNMP:
                    return DocumentationIdentifier.ApplicationSnmp;                    
                case ApplicationName.WakeOnLAN:
                    return DocumentationIdentifier.ApplicationWakeOnLan;                    
                case ApplicationName.HTTPHeaders:
                    return DocumentationIdentifier.ApplicationHttpHeaders;                    
                case ApplicationName.Whois:
                    return DocumentationIdentifier.ApplicationWhois;                    
                case ApplicationName.SubnetCalculator:
                    return DocumentationIdentifier.ApplicationSubnetCalculator;                    
                case ApplicationName.Lookup:
                    return DocumentationIdentifier.ApplicationLookup;                    
                case ApplicationName.Connections:
                    return DocumentationIdentifier.ApplicationConnections;                    
                case ApplicationName.Listeners:
                    return DocumentationIdentifier.ApplicationListeners;                    
                case ApplicationName.ARPTable:
                    return DocumentationIdentifier.ApplicationArpTable;                    
                case ApplicationName.None:
                    return DocumentationIdentifier.Default;                    
                default:
                    return DocumentationIdentifier.Default;                    
            }
        }
        #endregion
    }
}
