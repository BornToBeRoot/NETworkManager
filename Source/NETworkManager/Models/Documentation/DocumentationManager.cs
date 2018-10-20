using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.Models.Documentation
{
    public static class DocumentationManager
    {
        public const string DocumentationBaseUrl = @"https://github.com/BornToBeRoot/NETworkManager/tree/master/Documentation/";

        public static string DocumentationUrl => $"{DocumentationBaseUrl}{Settings.LocalizationManager.Current.Code}/README.md";

        public static List<DocumentationInfo> List => new List<DocumentationInfo>
        {
            new DocumentationInfo(DocumentationIdentifier.ApplicationNetworkInterface, @"/Application/NetworkInterface.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationIpScanner, @"/Application/IPScanner.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPortScanner, @"/Application/PortScanner.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPing, @"/Application/Ping.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTraceroute, @"/Application/Traceroute.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationDnsLookup, @"/Application/DNSLookup.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationRemoteDesktop, @"/Application/RemoteDesktop.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationPutty, @"/Application/PuTTY.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationTightVNC, @"/Application/TightVNC.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSnmp, @"/Application/SNMP.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWakeOnLan, @"/Application/WakeOnLAN.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationHttpHeaders, @"/Application/HTTPHeaders.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationWhois, @"/Application/Whois.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationSubnetCalculator, @"/Application/SubnetCalculator.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationLookup, @"/Application/Lookup.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationConnections, @"/Application/Connections.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationListeners, @"/Application/Listeners.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.ApplicationArpTable, @"/Application/ARPTable.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.HowToInstallRdp8Dot1OnWindows6Dot1, @"/HowTo/Install_RDP_8dot1_on_Windows6dot1.md", GetLocalizationInfoList("en-US", "de-DE")),
            new DocumentationInfo(DocumentationIdentifier.HowToCreateCustomThemeAndAccent, @"/HowTo/Create_custom_theme_and_accent.md", GetLocalizationInfoList("en-US", "de-DE"))
        };

       
        // Get localized documentation url (if available), else return the english webpage
        public static string GetLocalizedUrlById(DocumentationIdentifier documentationIdentifier)
        {
            var info = List.FirstOrDefault(x => x.Identifier == documentationIdentifier);

            // Return start page
            if (info == null)
                return DocumentationBaseUrl;

            // Try get localized help, fallback is english
            return info.Localizations.FirstOrDefault(x => x.Code == Settings.LocalizationManager.Current.Code) != null ? $"{DocumentationBaseUrl}{Settings.LocalizationManager.Current.Code}{info.Path}" : $"{DocumentationBaseUrl}en-US{info.Path}";
        }

        // Generate a list with culture codes
        private static List<Settings.LocalizationInfo> GetLocalizationInfoList(params string[] codes)
        {
            return codes.Select(code => new Settings.LocalizationInfo(code)).ToList();
        }

        public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
        {            
            Process.Start(documentationIdentifier == DocumentationIdentifier.Default ? DocumentationUrl : GetLocalizedUrlById(documentationIdentifier));
        }

        #region ICommands & Actions
        public static ICommand OpenDocumentationCommand => new RelayCommand(OpenDocumentationAction);

        private static void OpenDocumentationAction(object documentationIdentifier)
        {
            if (documentationIdentifier != null)
                OpenDocumentation((DocumentationIdentifier) documentationIdentifier);
        }
        #endregion
    }
}
