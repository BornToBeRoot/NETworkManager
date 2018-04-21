using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.Models.Documentation
{
    public static class DocumentationManager
    {
        public const string DocumentationBaseURL = @"https://github.com/BornToBeRoot/NETworkManager/tree/master/Documentation/";

        public static string DocumentationURL
        {
            get { return string.Format("{0}{1}/README.md", DocumentationBaseURL, Settings.LocalizationManager.Current.Code); }
        }

        public static List<DocumentationInfo> List
        {
            get
            {
                return new List<DocumentationInfo>
                {
                    new DocumentationInfo(DocumentationIdentifier.Application_NetworkInterface, @"/Application/NetworkInterface.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_IPScanner, @"/Application/IPScanner.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_PortScanner, @"/Application/PortScanner.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_Ping, @"/Application/Ping.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_Traceroute, @"/Application/Traceroute.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_DNSLookup, @"/Application/DNSLookup.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_RemoteDesktop, @"/Application/RemoteDesktop.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_PuTTY, @"/Application/PuTTY.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_SNMP, @"/Application/SNMP.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_WakeOnLAN, @"/Application/WakeOnLAN.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_HTTPHeaders, @"/Application/HTTPHeaders.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_SubnetCalculator, @"/Application/SubnetCalculator.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_Lookup, @"/Application/Lookup.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.Application_ARPTable, @"/Application/ARPTable.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.HowTo_InstallRDP8dot1onWindows6dot1, @"/HowTo/Install_RDP_8dot1_on_Windows6dot1.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(DocumentationIdentifier.HowTo_CreateCustomThemeAndAccent, @"/HowTo/Create_custom_theme_and_accent.md", GetLocalizationInfoList("en-US", "de-DE"))
                };
            }
        }

        public enum DocumentationIdentifier
        {
            Default,
            Application_NetworkInterface,
            Application_IPScanner,
            Application_PortScanner,
            Application_Ping,
            Application_Traceroute,
            Application_DNSLookup,
            Application_RemoteDesktop,
            Application_PuTTY,
            Application_SNMP,
            Application_WakeOnLAN,
            Application_HTTPHeaders,
            Application_SubnetCalculator,
            Application_Lookup,
            Application_ARPTable,
            HowTo_InstallRDP8dot1onWindows6dot1,
            HowTo_CreateCustomThemeAndAccent,
        }

        // Get localized documentation url (if available), else return the english webpage
        public static string GetLocalizedURLbyID(DocumentationIdentifier documentationIdentifier)
        {
            DocumentationInfo info = List.FirstOrDefault(x => x.Identifier == documentationIdentifier);

            if (info.Localizations.FirstOrDefault(x => x.Code == Settings.LocalizationManager.Current.Code) != null)
                return string.Format("{0}{1}{2}", DocumentationBaseURL, Settings.LocalizationManager.Current.Code, info.Path);
            else
                return string.Format("{0}en-US{1}", DocumentationBaseURL, info.Path);
        }

        // Generate a list with culture codes
        private static List<Settings.LocalizationInfo> GetLocalizationInfoList(params string[] codes)
        {
            List<Settings.LocalizationInfo> list = new List<Settings.LocalizationInfo>();

            foreach (string code in codes)
                list.Add(new Settings.LocalizationInfo(code));

            return list;
        }

        public static void OpenDocumentation(DocumentationIdentifier documentationIdentifier)
        {            
            Process.Start(documentationIdentifier == DocumentationIdentifier.Default ? DocumentationURL : GetLocalizedURLbyID(documentationIdentifier));
        }

        #region ICommands & Actions
        public static ICommand OpenDocumentationCommand
        {
            get { return new RelayCommand(p => OpenDocumentationAction(p)); }
        }

        private static void OpenDocumentationAction(object documentationIdentifier)
        {
            OpenDocumentation((DocumentationIdentifier)Enum.Parse(typeof(DocumentationIdentifier), documentationIdentifier as string));
        }
        #endregion
    }
}
