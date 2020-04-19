using NETworkManager.Models;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.Documentation
{
    /// <summary>
    /// This class is designed to interact with the documentation at https://borntoberoot.github.io/NETworkManager/.
    /// </summary>
    public static class DocumentationManager
    {
        /// <summary>
        /// Base path of the documentation.
        /// </summary>
        public const string DocumentationBaseUrl = @"https://borntoberoot.github.io/NETworkManager/Documentation/";

        /// <summary>
        /// Constant to identify the header, which should be displayed, of the website at <see cref="DocumentationBaseUrl"/>. 
        /// </summary>
        public const string DocumentationBaseUrlHeader = @"#documentation";

        /// <summary>
        /// List with all known documentation entries.
        /// </summary>
        private static List<DocumentationInfo> List => new List<DocumentationInfo>
        {
            new DocumentationInfo(DocumentationIdentifier.ApplicationDashboard, @"Application/Dashboard.html#dashboard"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface, @"Application/NetworkInterface.html#network-interface"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWiFi, @"Application/WLAN.html#wlan"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationIPScanner, @"Application/IPScanner.html#ip-scanner"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner, @"Application/PortScanner.html#port-scanner"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPing, @"Application/Ping.html#ping"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPingMonitor, @"Application/PingMonitor.html#ping-monitor"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute, @"Application/Traceroute.html#traceroute"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup, @"Application/DNSLookup.html#dns-lookup"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop, @"Application/RemoteDesktop.html#remote-desktop"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPowerShell, @"Application/PowerShell.html#powershell"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"Application/PuTTY.html#putty"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTigerVNC, @"Application/TigerVNC.html#tigervnc"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWebConsole, @"Application/WebConsole.html#web-console"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp, @"Application/SNMP.html#snmp"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDiscoveryProtocol, @"Application/DiscoveryProtocol.html#discovery-protocol"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan, @"Application/WakeOnLAN.html#wake-on-lan"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationHttpHeaders, @"Application/HTTPHeaders.html#http-headers"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWhois, @"Application/Whois.html#whois"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator, @"Application/SubnetCalculator.html#subnet-calculator"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationLookup, @"Application/Lookup.html#lookup"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationConnections, @"Application/Connections.html#connections"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationListeners, @"Application/Listeners.html#listeners"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable, @"Application/ARPTable.html#arp-table"),
        };

        /// <summary>
        /// Method to create the documentation url from <see cref="DocumentationIdentifier"/>.
        /// </summary>
        /// <param name="documentationIdentifier"><see cref="DocumentationIdentifier"/> of the documentation page you want to open.</param>
        /// <returns>URL of the documentation page.</returns>
        private static string CreateUrl(DocumentationIdentifier documentationIdentifier)
        {
            var info = List.FirstOrDefault(x => x.Identifier == documentationIdentifier);

            var url = DocumentationBaseUrl;

            if (info != null)
                url += info.Path;
            else
                url += DocumentationBaseUrlHeader;

            return url;
        }

        /// <summary>
        /// Method for opening a documentation page with the default webbrowser based on the <see cref="DocumentationIdentifier"/> .
        /// </summary>
        /// <param name="documentationIdentifier"><see cref="DocumentationIdentifier"/> of the documentation page you want to open.</param>
        public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
        {
            Process.Start(CreateUrl(documentationIdentifier));
        }

        /// <summary>
        /// Command to open a documentation page based on <see cref="DocumentationIdentifier"/>.
        /// </summary>
        public static ICommand OpenDocumentationCommand => new RelayCommand(OpenDocumentationAction);

        /// <summary>
        /// Method to open a documentation page based on <see cref="DocumentationIdentifier"/>.
        /// </summary>
        /// <param name="documentationIdentifier"></param>
        private static void OpenDocumentationAction(object documentationIdentifier)
        {
            if (documentationIdentifier != null)
                OpenDocumentation((DocumentationIdentifier)documentationIdentifier);
        }

        /// <summary>
        /// Method to get the <see cref="DocumentationIdentifier"/> from an <see cref="ApplicationName"/>.
        /// </summary>
        /// <param name="name"><see cref="ApplicationName"/> from which you want to get the <see cref="DocumentationIdentifier"/>.</param>
        /// <returns><see cref="DocumentationIdentifier"/> of the application.</returns>
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
    }
}
