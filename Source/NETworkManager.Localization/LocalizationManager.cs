using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NETworkManager.Localization
{
    /// <summary>
    /// Class provides variables/methods to manage localization.
    /// </summary>
    public class LocalizationManager
    {
        /// <summary>
        /// Constant for the default culture code.
        /// </summary>
        private const string _defaultCultureCode = "en-US";

        /// <summary>
        /// Constant with the path to the flag images.
        /// </summary>
        private const string _baseFlagImageUri = @"pack://application:,,,/NETworkManager.Localization;component/Resources/Flags/";

        /// <summary>
        /// Variable for the instance of the class.
        /// </summary>
        private static LocalizationManager _instance = null;

        /// <summary>
        /// Returns the current instance of the class.
        /// The language can be set on creation (first call), by passing a culture code (like "en-US") as parameter.
        /// Use <see cref="Change(LocalizationInfo)" /> to change it later.
        /// </summary>
        /// <param name="cultureCode">Culture code (default is "en-US"). See also <see cref="LocalizationInfo.Code"/>.</param>
        /// <returns>Instance of the class.</returns>
        public static LocalizationManager GetInstance(string cultureCode = _defaultCultureCode)
        {
            if (_instance == null)
                _instance = new LocalizationManager(cultureCode);

            return _instance;
        }

        /// <summary>
        /// Method to build the uri for a flag image based on the culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code like "en-US".</param>
        /// <returns>Uri of the flag image.</returns>
        public static Uri GetImageUri(string cultureCode)
        {
            return new Uri(_baseFlagImageUri + cultureCode + ".png");
        }

        /// <summary>
        /// List with all <see cref="LocalizationInfo"/>s.
        /// </summary>
        public static List<LocalizationInfo> List => new List<LocalizationInfo>
        {
            new LocalizationInfo("English", "English", GetImageUri("en-US"), "BornToBeRoot", "en-US", 100, true),
            new LocalizationInfo("German", "Deutsch",  GetImageUri("de-DE"), "BornToBeRoot", "de-DE", 100, true),
            new LocalizationInfo("Russian", "Русский", GetImageUri("ru-RU"), "LaXe", "ru-RU", 81.18, true),
            new LocalizationInfo("Spanish", "Español", GetImageUri("es-ES"), "MS-PC", "es-ES", 99.88, false),
            new LocalizationInfo("Italian", "Italiano", GetImageUri("it-IT"), "simone.ferrari", "it-IT", 87.45, false),
            new LocalizationInfo("Dutch", "Nederlands", GetImageUri("nl-NL"), "Get_r3kt_by_Me", "nl-NL", 43.54, false),
            new LocalizationInfo("French", "Français", GetImageUri("fr-FR"), "f4alm", "fr-FR", 14.76, false),
            new LocalizationInfo("Chinese", "汉语", GetImageUri("zh-CN"), "dockernes, pedoc, Bonelol, ccstorm", "zh-CN", 77.98, false),
            new LocalizationInfo("Brazilian Portuguese", "português brasileiro", GetImageUri("pt-BR"), "ghroll", "pt-BR", 11.07, false),
        };

        /// <summary>
        /// Variable with the currently used <see cref="LocalizationInfo"/>.
        /// </summary>
        public LocalizationInfo Current { get; private set; } = new LocalizationInfo();

        /// <summary>
        /// Variable with the currently used <see cref="CultureInfo"/>.
        /// </summary>
        public CultureInfo Culture { get; private set; }

        /// <summary>
        /// Create an instance and load the language based on the culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code (default is "en-US"). See also <see cref="LocalizationInfo.Code"/>.</param>
        private LocalizationManager(string cultureCode = _defaultCultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
                cultureCode = CultureInfo.CurrentCulture.Name;

            var info = GetLocalizationInfoBasedOnCode(cultureCode) ?? List.First();

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

        /// <summary>
        /// Method to get the <see cref="LocalizationInfo"/> based on the culture code.
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns>Return the <see cref="LocalizationInfo"/> or <see cref="null"/> if not found.</returns>
        public static LocalizationInfo GetLocalizationInfoBasedOnCode(string cultureCode)
        {
            return List.FirstOrDefault(x => x.Code == cultureCode) ?? null;
        }

        /// <summary>
        /// Method to change the langauge.
        /// </summary>
        /// <param name="info"><see cref="LocalizationInfo"/></param>
        public void Change(LocalizationInfo info)
        {
            Current = info;

            Culture = new CultureInfo(info.Code);
        }
    }
}
