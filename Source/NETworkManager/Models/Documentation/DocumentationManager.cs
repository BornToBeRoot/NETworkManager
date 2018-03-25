using NETworkManager.Utils;
using NETworkManager.ViewModels;
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
                    new DocumentationInfo(00001, @"/HowTo/Install_RDP_8dot1_on_Windows6dot1.md", GetLocalizationInfoList("en-US", "de-DE")),
                    new DocumentationInfo(00002, @"/HowTo/Create_custom_theme_and_accent.md", GetLocalizationInfoList("en-US", "de-DE"))
                };
            }
        }

        // Get localized documentation url (if available), else return the english webpage
        public static string GetLocalizedURLbyID(int id)
        {
            DocumentationInfo info = List.FirstOrDefault(x => x.Identifier == id);

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

        public static ICommand OpenDocumentationCommand
        {
            get { return new RelayCommand(p => OpenDocumentationAction(p)); }
        }

        private static void OpenDocumentationAction(object id)
        {
            Process.Start(DocumentationManager.GetLocalizedURLbyID(int.Parse((string)id)));
        }
    }
}
