using NETworkManager.Models;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.Documentation
{
    public static class DocumentationManager
    {
        public const string DocumentationBaseUrl = @"https://borntoberoot.github.io/NETworkManager/Documentation/";

        public static List<DocumentationInfo> List => new List<DocumentationInfo>
        {
            new DocumentationInfo(DocumentationIdentifier.ApplicationDashboard, @"Application/Dashboard.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface, @"Application/NetworkInterface.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWiFi, @"Application/WiFi.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationIPScanner, @"Application/IPScanner.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner, @"Application/PortScanner.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPing, @"Application/Ping.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPingMonitor, @"Application/PingMonitor.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute, @"Application/Traceroute.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup, @"Application/DNSLookup.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop, @"Application/RemoteDesktop.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPowerShell, @"Application/PowerShell.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"Application/PuTTY.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTigerVNC, @"Application/TigerVNC.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWebConsole, @"Application/WebConsole.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp, @"Application/SNMP.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDiscoveryProtocol, @"Application/DiscoveryProtocol.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan, @"Application/WakeOnLAN.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationHttpHeaders, @"Application/HTTPHeaders.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWhois, @"Application/Whois.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator, @"Application/SubnetCalculator.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationLookup, @"Application/Lookup.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationConnections, @"Application/Connections.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationListeners, @"Application/Listeners.html"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable, @"Application/ARPTable.html"),
        };

        // Get localized documentation url (if available), else return the english page
        public static string CreateUrl(DocumentationIdentifier documentationIdentifier)
        {
            var info = List.FirstOrDefault(x => x.Identifier == documentationIdentifier);

            var url = DocumentationBaseUrl;

            if (info != null)
                url += info.Path;

            return url;
        }

        public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
        {
            Process.Start(CreateUrl(documentationIdentifier));
        }

        #region ICommands & Actions
        public static ICommand OpenDocumentationCommand => new RelayCommand(OpenDocumentationAction);

        private static void OpenDocumentationAction(object documentationIdentifier)
        {
            if (documentationIdentifier != null)
                OpenDocumentation((DocumentationIdentifier)documentationIdentifier);
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
