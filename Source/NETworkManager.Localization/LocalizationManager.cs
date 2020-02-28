using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NETworkManager.Localization
{
    public class LocalizationManager
    {
        /// <summary>
        /// Constant for the default culture code
        /// </summary>
        private const string _defaultCultureCode = "en-US";
        
        /// <summary>
        /// Variable for the instance of the class.
        /// </summary>
        private static LocalizationManager _instance = null;

        /// <summary>
        /// Method to return the current instance of the class.
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
        /// List with all <see cref="LocalizationInfo"/>s.
        /// </summary>
        public static List<LocalizationInfo> List => new List<LocalizationInfo>
        {
            new LocalizationInfo("English", "English", new Uri("/Images/en-US.png", UriKind.Relative), "BornToBeRoot", "en-US", 100, true),
            new LocalizationInfo("German", "Deutsch", new Uri("/Images/de-DE.png", UriKind.Relative), "BornToBeRoot", "de-DE", 100, true),
            new LocalizationInfo("Russian", "Русский", new Uri("/Images/ru-RU.png", UriKind.Relative), "LaXe", "ru-RU", 81.18, true),
            new LocalizationInfo("Spanish", "Español", new Uri("/Images/es-ES.png", UriKind.Relative), "MS-PC", "es-ES", 99.88, false),
            new LocalizationInfo("Italian", "Italiano", new Uri("/Images/it-IT.png", UriKind.Relative), "simone.ferrari", "it-IT", 87.45, false),
            new LocalizationInfo("Dutch", "Nederlands", new Uri("/Images/nl-NL.png", UriKind.Relative), "Get_r3kt_by_Me", "nl-NL", 43.54, false),
            new LocalizationInfo("French", "Français", new Uri("/Images/fr-FR.png", UriKind.Relative), "f4alm", "fr-FR", 14.76, false),
            new LocalizationInfo("Chinese", "汉语", new Uri("/Images/zh-CN.png", UriKind.Relative), "dockernes, pedoc, Bonelol, ccstorm", "zh-CN", 77.98, false),
            new LocalizationInfo("Brazilian Portuguese", "português brasileiro", new Uri("/Images/pt-BR.png", UriKind.Relative), "ghroll", "pt-BR", 11.07, false),
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
