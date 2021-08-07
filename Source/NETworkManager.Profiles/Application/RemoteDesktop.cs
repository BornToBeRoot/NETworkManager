using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;
using System.Windows.Forms;

namespace NETworkManager.Profiles.Application
{
    public static class RemoteDesktop
    {
        public static RemoteDesktopSessionInfo CreateSessionInfo()
        {
            return new RemoteDesktopSessionInfo
            {
                // Hostname
                Hostname = string.Empty,

                // Network
                Port = SettingsManager.Current.RemoteDesktop_Port,

                // Display
                AdjustScreenAutomatically = SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically,
                UseCurrentViewSize = SettingsManager.Current.RemoteDesktop_UseCurrentViewSize,
                DesktopWidth = SettingsManager.Current.RemoteDesktop_UseCustomScreenSize ? SettingsManager.Current.RemoteDesktop_CustomScreenWidth : SettingsManager.Current.RemoteDesktop_ScreenWidth,
                DesktopHeight = SettingsManager.Current.RemoteDesktop_UseCustomScreenSize ? SettingsManager.Current.RemoteDesktop_CustomScreenHeight : SettingsManager.Current.RemoteDesktop_ScreenHeight,
                ColorDepth = SettingsManager.Current.RemoteDesktop_ColorDepth,

                // Authentication
                EnableCredSspSupport = SettingsManager.Current.RemoteDesktop_EnableCredSspSupport,
                AuthenticationLevel = SettingsManager.Current.RemoteDesktop_AuthenticationLevel,

                // Remote audio
                AudioRedirectionMode = SettingsManager.Current.RemoteDesktop_AudioRedirectionMode,
                AudioCaptureRedirectionMode = SettingsManager.Current.RemoteDesktop_AudioCaptureRedirectionMode,

                // Keyboard
                KeyboardHookMode = SettingsManager.Current.RemoteDesktop_KeyboardHookMode,

                // Local devices and resources
                RedirectClipboard = SettingsManager.Current.RemoteDesktop_RedirectClipboard,
                RedirectDevices = SettingsManager.Current.RemoteDesktop_RedirectDevices,
                RedirectDrives = SettingsManager.Current.RemoteDesktop_RedirectDrives,
                RedirectPorts = SettingsManager.Current.RemoteDesktop_RedirectPorts,
                RedirectSmartCards = SettingsManager.Current.RemoteDesktop_RedirectSmartCards,
                RedirectPrinters = SettingsManager.Current.RemoteDesktop_RedirectPrinters,

                // Experience
                PersistentBitmapCaching = SettingsManager.Current.RemoteDesktop_PersistentBitmapCaching,
                ReconnectIfTheConnectionIsDropped = SettingsManager.Current.RemoteDesktop_ReconnectIfTheConnectionIsDropped,

                // Performance
                NetworkConnectionType = SettingsManager.Current.RemoteDesktop_NetworkConnectionType,
                DesktopBackground = SettingsManager.Current.RemoteDesktop_DesktopBackground,
                FontSmoothing = SettingsManager.Current.RemoteDesktop_FontSmoothing,
                DesktopComposition = SettingsManager.Current.RemoteDesktop_DesktopComposition,
                ShowWindowContentsWhileDragging = SettingsManager.Current.RemoteDesktop_ShowWindowContentsWhileDragging,
                MenuAndWindowAnimation = SettingsManager.Current.RemoteDesktop_MenuAndWindowAnimation,
                VisualStyles = SettingsManager.Current.RemoteDesktop_VisualStyles
            };
        }

        public static RemoteDesktopSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new RemoteDesktopSessionInfo();
            // Override hostname
            info.Hostname = profileInfo.Host;

            // Network
            if (profileInfo.RemoteDesktop_OverridePort)
                info.Port = profileInfo.RemoteDesktop_Port;

            // Display
            if (profileInfo.RemoteDesktop_OverrideDisplay)
            {
                info.AdjustScreenAutomatically = profileInfo.RemoteDesktop_AdjustScreenAutomatically;
                info.UseCurrentViewSize = profileInfo.RemoteDesktop_UseCurrentViewSize;
                info.DesktopWidth = profileInfo.RemoteDesktop_UseCustomScreenSize ? profileInfo.RemoteDesktop_CustomScreenWidth : profileInfo.RemoteDesktop_ScreenWidth;
                info.DesktopHeight = profileInfo.RemoteDesktop_UseCustomScreenSize ? profileInfo.RemoteDesktop_CustomScreenHeight : profileInfo.RemoteDesktop_ScreenHeight;
                info.ColorDepth = profileInfo.RemoteDesktop_ColorDepth;
            }

            // Authentication
            if (profileInfo.RemoteDesktop_OverrideCredSspSupport)
                info.EnableCredSspSupport = profileInfo.RemoteDesktop_EnableCredSspSupport;

            if (profileInfo.RemoteDesktop_OverrideAuthenticationLevel)
                info.AuthenticationLevel = profileInfo.RemoteDesktop_AuthenticationLevel;

            // Remote audio
            if (profileInfo.RemoteDesktop_OverrideAudioRedirectionMode)
                info.AudioRedirectionMode = profileInfo.RemoteDesktop_AudioRedirectionMode;

            if (profileInfo.RemoteDesktop_OverrideAudioCaptureRedirectionMode)
                info.AudioCaptureRedirectionMode = profileInfo.RemoteDesktop_AudioCaptureRedirectionMode;

            // Keyboard
            if (profileInfo.RemoteDesktop_OverrideApplyWindowsKeyCombinations)
                info.KeyboardHookMode = profileInfo.RemoteDesktop_KeyboardHookMode;

            // Local devices and resources
            if (profileInfo.RemoteDesktop_OverrideRedirectClipboard)
                info.RedirectClipboard = profileInfo.RemoteDesktop_RedirectClipboard;

            if (profileInfo.RemoteDesktop_OverrideRedirectDevices)
                info.RedirectDevices = profileInfo.RemoteDesktop_RedirectDevices;

            if (profileInfo.RemoteDesktop_OverrideRedirectDrives)
                info.RedirectDrives = profileInfo.RemoteDesktop_RedirectDrives;

            if (profileInfo.RemoteDesktop_OverrideRedirectPorts)
                info.RedirectPorts = profileInfo.RemoteDesktop_RedirectPorts;

            if (profileInfo.RemoteDesktop_OverrideRedirectSmartcards)
                info.RedirectSmartCards = profileInfo.RemoteDesktop_RedirectSmartCards;

            if (profileInfo.RemoteDesktop_OverrideRedirectPrinters)
                info.RedirectPrinters = profileInfo.RemoteDesktop_RedirectPrinters;

            // Experience
            if (profileInfo.RemoteDesktop_OverridePersistentBitmapCaching)
                info.PersistentBitmapCaching = profileInfo.RemoteDesktop_PersistentBitmapCaching;

            if (profileInfo.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped)
                info.ReconnectIfTheConnectionIsDropped = profileInfo.RemoteDesktop_ReconnectIfTheConnectionIsDropped;

            // Performance
            if (profileInfo.RemoteDesktop_OverrideNetworkConnectionType)
                info.NetworkConnectionType = profileInfo.RemoteDesktop_NetworkConnectionType;

            if (profileInfo.RemoteDesktop_OverrideDesktopBackground)
                info.DesktopBackground = profileInfo.RemoteDesktop_DesktopBackground;

            if (profileInfo.RemoteDesktop_OverrideFontSmoothing)
                info.FontSmoothing = profileInfo.RemoteDesktop_FontSmoothing;

            if (profileInfo.RemoteDesktop_OverrideDesktopComposition)
                info.DesktopComposition = profileInfo.RemoteDesktop_DesktopComposition;

            if (profileInfo.RemoteDesktop_OverrideShowWindowContentsWhileDragging)
                info.ShowWindowContentsWhileDragging = profileInfo.RemoteDesktop_ShowWindowContentsWhileDragging;

            if (profileInfo.RemoteDesktop_OverrideMenuAndWindowAnimation)
                info.MenuAndWindowAnimation = profileInfo.RemoteDesktop_MenuAndWindowAnimation;

            if (profileInfo.RemoteDesktop_OverrideVisualStyles)
                info.VisualStyles = profileInfo.RemoteDesktop_VisualStyles;


            // Set credentials
            if (profileInfo.RemoteDesktop_UseCredentials)
            {
                info.CustomCredentials = true;

                info.Username = profileInfo.RemoteDesktop_Username;
                info.Password = profileInfo.RemoteDesktop_Password;
            }

            return info;
        }
    }
}
