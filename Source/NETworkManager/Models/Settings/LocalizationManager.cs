using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using NETworkManager.Models.Network;
using NETworkManager.Enum;
using static NETworkManager.Models.RemoteDesktop.RemoteDesktop;

namespace NETworkManager.Models.Settings
{
    public static class LocalizationManager
    {
        public const string TcpStateIdentifier = "TcpState_";
        public const string IPStatusIdentifier = "IPStatus_";
        public const string PortStateIdentifier = "PortState_";
        public const string ConnectionStateIdentifier = "ConnectionState_";
        public const string RemoteDesktopNetworkConnectionTypeIdentifier = "RemoteDesktopNetworkConnectionType_";
        public const string RemoteDesktopKeyboardHookModeIdentifier = "RemoteDesktopKeyboardHookMode_";
        public const string RemoteDesktopAudioRedirectionModeIdentifier = "RemoteDesktopAudioRedirectionMode_";
        public const string RemoteDesktopAudioCaptureRedirectionModeIdentifier = "RemoteDesktopAudioCaptureRedirectionMode_";

        public static List<LocalizationInfo> List => new List<LocalizationInfo>
        {
            new LocalizationInfo("English", "English", new Uri("/Resources/Localization/Flags/en-US.png", UriKind.Relative), "BornToBeRoot", "en-US", 100,true),
            new LocalizationInfo("German", "Deutsch", new Uri("/Resources/Localization/Flags/de-DE.png", UriKind.Relative), "BornToBeRoot", "de-DE", 100, true),
            new LocalizationInfo("Russian", "Русский", new Uri("/Resources/Localization/Flags/ru-RU.png", UriKind.Relative), "LaXe", "ru-RU", 87.78, true),
            new LocalizationInfo("Spanish", "Español", new Uri("/Resources/Localization/Flags/es-ES.png", UriKind.Relative), "MS-PC", "es-ES", 98.83, false),
            new LocalizationInfo("Italian", "Italiano", new Uri("/Resources/Localization/Flags/it-IT.png", UriKind.Relative), "simone.ferrari", "it-IT", 94.54, false),
            new LocalizationInfo("Dutch", "Nederlands", new Uri("/Resources/Localization/Flags/nl-NL.png", UriKind.Relative), "Get_r3kt_by_Me", "nl-NL", 46.42, false),
            new LocalizationInfo("French", "Français", new Uri("/Resources/Localization/Flags/fr-FR.png", UriKind.Relative), "f4alm", "fr-FR", 15.73, false),            
            new LocalizationInfo("Chinese", "汉语", new Uri("/Resources/Localization/Flags/zh-CN.png", UriKind.Relative), "dockernes, pedoc, Bonelol, ccstorm", "zh-CN", 81.40, false),
            
            /*
            new LocalizationInfo("Latvian", "", new Uri("/Resources/Localization/Flags/lv-LV.png", UriKind.Relative), "", "lv-LV", 0, false),
            new LocalizationInfo("Portuguese", "", new Uri("/Resources/Localization/Flags/pt-BR.png", UriKind.Relative), "", "pt-BR", 0, false),            
            */
        };

        public static LocalizationInfo Current { get; set; } = new LocalizationInfo();

        public static CultureInfo Culture { get; set; }

        static LocalizationManager()
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

            var status = Resources.Localization.Strings.ResourceManager.GetString(IPStatusIdentifier + ipStatus, Culture);

            return string.IsNullOrEmpty(status) ? ipStatus.ToString() : status;
        }

        public static string TranslatePortStatus(object value)
        {
            if (!(value is PortInfo.PortStatus portStatus))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(PortStateIdentifier + portStatus, Culture);

            return string.IsNullOrEmpty(status) ? portStatus.ToString() : status;
        }

        public static string TranslateTcpState(object value)
        {
            if (!(value is TcpState tcpState))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(TcpStateIdentifier + tcpState, Culture);

            return string.IsNullOrEmpty(status) ? tcpState.ToString() : status;
        }

        public static string TranslateConnectionState(object value)
        {
            if (!(value is ConnectionState connectionState))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(ConnectionStateIdentifier + connectionState, Culture);

            return string.IsNullOrEmpty(status) ? connectionState.ToString() : status;
        }

        public static string TranslateRemoteDesktopConnectionSpeed(object value)
        {
            if (!(value is NetworkConnectionType networkConnectionType))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(RemoteDesktopNetworkConnectionTypeIdentifier + networkConnectionType, Culture);

            return string.IsNullOrEmpty(status) ? networkConnectionType.ToString() : status;
        }

        public static string TranslateRemoteDesktopKeyboardHookMode(object value)
        {
            if (!(value is KeyboardHookMode keyboardHookMode))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(RemoteDesktopKeyboardHookModeIdentifier + keyboardHookMode, Culture);

            return string.IsNullOrEmpty(status) ? keyboardHookMode.ToString() : status;
        }

        public static string TranslateRemoteDesktopAudioRedirectionMode(object value)
        {
            if (!(value is AudioRedirectionMode audioRedirectionMode))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(RemoteDesktopAudioRedirectionModeIdentifier + audioRedirectionMode, Culture);

            return string.IsNullOrEmpty(status) ? audioRedirectionMode.ToString() : status;
        }

        public static string TranslateRemoteDesktopAudioCaptureRedirectionMode(object value)
        {
            if (!(value is AudioCaptureRedirectionMode audioCaptureRedirectionMode))
                return "-/-";

            var status = Resources.Localization.Strings.ResourceManager.GetString(RemoteDesktopAudioCaptureRedirectionModeIdentifier + audioCaptureRedirectionMode, Culture);

            return string.IsNullOrEmpty(status) ? audioCaptureRedirectionMode.ToString() : status;
        }
    }
}
