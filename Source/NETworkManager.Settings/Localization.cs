using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace NETworkManager.Settings
{
    public static class Localization
    {
        /// <summary>
        /// List of all available localizations
        /// Localizations are stored as .xaml-file in the resources
        /// </summary>
        public static List<LocalizationInfo> List
        {
            get
            {
                return new List<LocalizationInfo> {
                    new LocalizationInfo("English", "/Resources/Localization/Resources.en-US.xaml", new Uri("/Resources/Localization/Flags/en-US.png", UriKind.Relative), "BornToBeRoot", "en-US"),
                    new LocalizationInfo("Deutsch", "/Resources/Localization/Resources.de-DE.xaml", new Uri("/Resources/Localization/Flags/de-DE.png", UriKind.Relative), "BornToBeRoot", "de-DE")
                };
            }
        }

        /// <summary>
        /// Get or set the current localization
        /// </summary>
        private static LocalizationInfo _current = new LocalizationInfo();
        public static LocalizationInfo Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public static CultureInfo Culture { get; set; }

        /// <summary>
        /// Load the localization from the user settings
        /// </summary>
        public static void Load()
        {
            string cultureCode = Settings.Properties.Settings.Default.Localization_CultureCode;

            if (string.IsNullOrEmpty(cultureCode))
                cultureCode = CultureInfo.CurrentCulture.Name;

            LocalizationInfo info = List.Where(x => x.Code == cultureCode).FirstOrDefault();

            if (info == null)
                info = List.First();

            if (info.Code != Properties.Resources.Localization_DefaultCultureCode)
                Change(info);
            else
            {
                Current = info;
                Culture = new CultureInfo(info.Code);
            }
        }


        private static ResourceDictionary _resourceDictionaryLocalization;

        /// <summary>
        /// Change the localization
        /// </summary>
        /// <param name="info">LocalizationInfo</param>
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
    }
}
