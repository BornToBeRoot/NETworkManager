using NETworkManager.Models;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.Documentation
{
    /// <summary>
    /// This class is designed to interact with the documentation at https://borntoberoot.net/NETworkManager/.
    /// </summary>
    public static class DocumentationManager
    {
        /// <summary>
        /// Base path of the documentation.
        /// </summary>
        public const string DocumentationBaseUrl = @"https://borntoberoot.net/NETworkManager/";

        /// <summary>
        /// List with all known documentation entries.
        /// </summary>
        private static List<DocumentationInfo> List => new List<DocumentationInfo>
        {
            new DocumentationInfo(DocumentationIdentifier.ApplicationDashboard, @"Documentation/Application/Dashboard"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface, @"Documentation/Application/NetworkInterface"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWiFi, @"Documentation/Application/WiFi"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationIPScanner, @"Documentation/Application/IPScanner"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner, @"Documentation/Application/PortScanner"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPingMonitor, @"Documentation/Application/PingMonitor"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute, @"Documentation/Application/Traceroute"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup, @"Documentation/Application/DNSLookup"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop, @"Documentation/Application/RemoteDesktop"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPowerShell, @"Documentation/Application/PowerShell"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"Documentation/Application/PuTTY"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"Documentation/Application/AWSSessionManager"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTigerVNC, @"Documentation/Application/TigerVNC"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWebConsole, @"Documentation/Application/WebConsole"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp, @"Documentation/Application/SNMP"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDiscoveryProtocol, @"Documentation/Application/DiscoveryProtocol"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan, @"Documentation/Application/WakeOnLAN"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWhois, @"Documentation/Application/Whois"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator, @"Documentation/Application/SubnetCalculator"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationLookup, @"Documentation/Application/Lookup"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationConnections, @"Documentation/Application/Connection"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationListeners, @"Documentation/Application/Listeners"),
            new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable, @"Documentation/Application/ARPTable"),
            new DocumentationInfo(DocumentationIdentifier.CommandLineArguments, @"Documentation/CommandLine/CommandLineArguments"),
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

            return url;
        }

        /// <summary>
        /// Method for opening a documentation page with the default webbrowser based on the <see cref="DocumentationIdentifier"/> .
        /// </summary>
        /// <param name="documentationIdentifier"><see cref="DocumentationIdentifier"/> of the documentation page you want to open.</param>
        public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
        {
            ExternalProcessStarter.OpenUrl(CreateUrl(documentationIdentifier));
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
                case ApplicationName.PingMonitor:
                    return DocumentationIdentifier.ApplicationPingMonitor;
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
                case ApplicationName.WebConsole:
                    return DocumentationIdentifier.ApplicationWebConsole;
                case ApplicationName.SNMP:
                    return DocumentationIdentifier.ApplicationSnmp;
                case ApplicationName.DiscoveryProtocol:
                    return DocumentationIdentifier.ApplicationDiscoveryProtocol;
                case ApplicationName.WakeOnLAN:
                    return DocumentationIdentifier.ApplicationWakeOnLan;
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
