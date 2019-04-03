using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using NETworkManager.Models.Network;
using NETworkManager.Enum;

namespace NETworkManager.Models.Settings
{
    public static class LocalizationManager
    {
        public static List<LocalizationInfo> List => new List<LocalizationInfo>
        {
            new LocalizationInfo("English", "English", new Uri("/Resources/Localization/Flags/en-US.png", UriKind.Relative), "BornToBeRoot", "en-US",100,true),
            new LocalizationInfo("German", "Deutsch", new Uri("/Resources/Localization/Flags/de-DE.png", UriKind.Relative), "BornToBeRoot", "de-DE",100, true),
            new LocalizationInfo("Russian", "Русский", new Uri("/Resources/Localization/Flags/ru-RU.png", UriKind.Relative), "LaXe", "ru-RU", 100, true),
            new LocalizationInfo("Spanish", "Español", new Uri("/Resources/Localization/Flags/es-ES.png", UriKind.Relative), "MS-PC", "es-ES", 100, false), 
            /*,
            new LocalizationInfo("French", "", new Uri("/Resources/Localization/Flags/fr-FR.png", UriKind.Relative), "", "fr-FR", 0, false),
            new LocalizationInfo("Dutch", "", new Uri("/Resources/Localization/Flags/nl-NL.png", UriKind.Relative), "", "nl-NL", 0, false),
            new LocalizationInfo("Latvian", "", new Uri("/Resources/Localization/Flags/lv-LV.png", UriKind.Relative), "", "lv-LV", 0, false),
            new LocalizationInfo("Portuguese", "", new Uri("/Resources/Localization/Flags/pt-BR.png", UriKind.Relative), "", "pt-BR", 0, false),
            */
        };

        public static LocalizationInfo Current { get; set; } = new LocalizationInfo();

        public static CultureInfo Culture { get; set; }

        public static void Load()
        {
            // Get the language from the user settings
            var cultureCode = SettingsManager.Current.Localization_CultureCode;

            // If it's empty... detect the windows language
            if (string.IsNullOrEmpty(cultureCode))
                cultureCode = CultureInfo.CurrentCulture.Name;

            // Get the language from the list
            var info = List.FirstOrDefault(x => x.Code == cultureCode) ?? List.First();

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

        public static void Change(LocalizationInfo info)
        {
            // Set the current localization
            Current = info;

            // Set the culture code
            Culture = new CultureInfo(info.Code);
        }

        public static string TranslateIPStatus(object value)
        {
            if (!(value is IPStatus ipStatus))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString("IPStatus_" + ipStatus, Culture);

            return string.IsNullOrEmpty(status) ? ipStatus.ToString() : status;
        }

        public static string TranslatePortStatus(object value)
        {
            if (!(value is PortInfo.PortStatus portStatus))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString("PortState_" + portStatus, Culture);

            return string.IsNullOrEmpty(status) ? portStatus.ToString() : status;
        }

        public static string TranslateTcpState(object value)
        {
            if (!(value is TcpState tcpState))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString("TcpState_" + tcpState, Culture);

            return string.IsNullOrEmpty(status) ? tcpState.ToString() : status;
        }

        public static string TranslateConnectionState(object value)
        {
            if (!(value is ConnectionState connectionState))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString("ConnectionState_" + connectionState, Culture);

            return string.IsNullOrEmpty(status) ? connectionState.ToString() : status;
        }
    }
}
