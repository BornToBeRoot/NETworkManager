using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace NETworkManager.Models.Settings
{
    public static class LocalizationManager
    {
        public static List<LocalizationInfo> List
        {
            get
            {
                return new List<LocalizationInfo> {
                    new LocalizationInfo("English", "English", "/Resources/Localization/en-US.xaml", new Uri("/Resources/Localization/Flags/en-US.png", UriKind.Relative), "BornToBeRoot", "en-US"),
                    new LocalizationInfo("German", "Deutsch", "/Resources/Localization/de-DE.xaml", new Uri("/Resources/Localization/Flags/de-DE.png", UriKind.Relative), "BornToBeRoot", "de-DE"),
                    new LocalizationInfo("Russian", "Русский", "/Resources/Localization/ru-RU.xaml", new Uri("/Resources/Localization/Flags/ru-RU.png", UriKind.Relative), "LaXe", "ru-RU")
                };
            }
        }

        private static LocalizationInfo _current = new LocalizationInfo();
        public static LocalizationInfo Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public static CultureInfo Culture { get; set; }

        public static void Load()
        {
            // Get the language from the user settings
            string cultureCode = SettingsManager.Current.Localization_CultureCode;

            // If it's empty... detect the windows language
            if (string.IsNullOrEmpty(cultureCode))
                cultureCode = CultureInfo.CurrentCulture.Name;

            // Get the language from the list
            LocalizationInfo info = List.Where(x => x.Code == cultureCode).FirstOrDefault();

            // If it's not in the list, get the first one
            if (info == null)
                info = List.First();

            // Change the language if it's different than en-US
            if (info.Code != List.First().Code)
            {
                Change(info);
            }
            else
            {
                Current = info;
                Culture = new CultureInfo(info.Code);
            }
        }

        private static ResourceDictionary _resourceDictionaryLocalization;

        public static void Change(LocalizationInfo info)
        {
            // Set the current localization
            Current = info;

            // Remove dictionaries, which are no longer required
            if (_resourceDictionaryLocalization != null)
                Application.Current.Resources.MergedDictionaries.Remove(_resourceDictionaryLocalization);

            // Create/Initialize a new dictionary from the .xaml-file in the resource
            _resourceDictionaryLocalization = new ResourceDictionary { Source = new Uri(info.Path, UriKind.Relative) };

            // Add the new dictionary
            Application.Current.Resources.MergedDictionaries.Add(_resourceDictionaryLocalization);

            // Set the culture code
            Culture = new CultureInfo(info.Code);
        }

        public static string GetStringByKey(string key)
        {
            return (Application.Current.Resources[key] as string).Replace(@"\n", Environment.NewLine);
        }
    }
}
