using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;

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

        public static RemoteDesktopSessionInfo CreateSessionInfo(ProfileInfo profile)
        {
            var info = CreateSessionInfo();

            // Get group info
            GroupInfo group = ProfileManager.GetGroup(profile.Group);

            // Override hostname
            info.Hostname = profile.RemoteDesktop_Host;

            // Network
            if (profile.RemoteDesktop_OverridePort)
                info.Port = profile.RemoteDesktop_Port;
            else if (group.RemoteDesktop_OverridePort)
                info.Port = group.RemoteDesktop_Port;

            // Display
            if (profile.RemoteDesktop_OverrideDisplay)
            {
                info.AdjustScreenAutomatically = profile.RemoteDesktop_AdjustScreenAutomatically;
                info.UseCurrentViewSize = profile.RemoteDesktop_UseCurrentViewSize;
                info.DesktopWidth = profile.RemoteDesktop_UseCustomScreenSize ? profile.RemoteDesktop_CustomScreenWidth : profile.RemoteDesktop_ScreenWidth;
                info.DesktopHeight = profile.RemoteDesktop_UseCustomScreenSize ? profile.RemoteDesktop_CustomScreenHeight : profile.RemoteDesktop_ScreenHeight;
                info.ColorDepth = profile.RemoteDesktop_ColorDepth;
            }
            else if (group.RemoteDesktop_OverrideDisplay)
            {
                info.AdjustScreenAutomatically = group.RemoteDesktop_AdjustScreenAutomatically;
                info.UseCurrentViewSize = group.RemoteDesktop_UseCurrentViewSize;
                info.DesktopWidth = group.RemoteDesktop_UseCustomScreenSize ? group.RemoteDesktop_CustomScreenWidth : group.RemoteDesktop_ScreenWidth;
                info.DesktopHeight = group.RemoteDesktop_UseCustomScreenSize ? group.RemoteDesktop_CustomScreenHeight : group.RemoteDesktop_ScreenHeight;
                info.ColorDepth = group.RemoteDesktop_ColorDepth;
            }

            // Authentication
            if (profile.RemoteDesktop_OverrideCredSspSupport)
                info.EnableCredSspSupport = profile.RemoteDesktop_EnableCredSspSupport;
            else if (group.RemoteDesktop_OverrideCredSspSupport)
                info.EnableCredSspSupport = group.RemoteDesktop_EnableCredSspSupport;

            if (profile.RemoteDesktop_OverrideAuthenticationLevel)
                info.AuthenticationLevel = profile.RemoteDesktop_AuthenticationLevel;
            else if (group.RemoteDesktop_OverrideAuthenticationLevel)
                info.AuthenticationLevel = group.RemoteDesktop_AuthenticationLevel;

            // Remote audio
            if (profile.RemoteDesktop_OverrideAudioRedirectionMode)
                info.AudioRedirectionMode = profile.RemoteDesktop_AudioRedirectionMode;
            else if (group.RemoteDesktop_OverrideAudioRedirectionMode)
                info.AudioRedirectionMode = group.RemoteDesktop_AudioRedirectionMode;

            if (profile.RemoteDesktop_OverrideAudioCaptureRedirectionMode)
                info.AudioCaptureRedirectionMode = profile.RemoteDesktop_AudioCaptureRedirectionMode;
            else if (group.RemoteDesktop_OverrideAudioCaptureRedirectionMode)
                info.AudioCaptureRedirectionMode = group.RemoteDesktop_AudioCaptureRedirectionMode;

            // Keyboard
            if (profile.RemoteDesktop_OverrideApplyWindowsKeyCombinations)
                info.KeyboardHookMode = profile.RemoteDesktop_KeyboardHookMode;
            else if (group.RemoteDesktop_OverrideApplyWindowsKeyCombinations)
                info.KeyboardHookMode = group.RemoteDesktop_KeyboardHookMode;

            // Local devices and resources
            if (profile.RemoteDesktop_OverrideRedirectClipboard)
                info.RedirectClipboard = profile.RemoteDesktop_RedirectClipboard;
            else if (group.RemoteDesktop_OverrideRedirectClipboard)
                info.RedirectClipboard = group.RemoteDesktop_RedirectClipboard;

            if (profile.RemoteDesktop_OverrideRedirectDevices)
                info.RedirectDevices = profile.RemoteDesktop_RedirectDevices;
            else if (group.RemoteDesktop_OverrideRedirectDevices)
                info.RedirectDevices = group.RemoteDesktop_RedirectDevices;

            if (profile.RemoteDesktop_OverrideRedirectDrives)
                info.RedirectDrives = profile.RemoteDesktop_RedirectDrives;
            else if (group.RemoteDesktop_OverrideRedirectDrives)
                info.RedirectDrives = group.RemoteDesktop_RedirectDrives;

            if (profile.RemoteDesktop_OverrideRedirectPorts)
                info.RedirectPorts = profile.RemoteDesktop_RedirectPorts;
            else if (group.RemoteDesktop_OverrideRedirectPorts)
                info.RedirectPorts = group.RemoteDesktop_RedirectPorts;

            if (profile.RemoteDesktop_OverrideRedirectSmartcards)
                info.RedirectSmartCards = profile.RemoteDesktop_RedirectSmartCards;
            else if (group.RemoteDesktop_OverrideRedirectSmartcards)
                info.RedirectSmartCards = group.RemoteDesktop_RedirectSmartCards;

            if (profile.RemoteDesktop_OverrideRedirectPrinters)
                info.RedirectPrinters = profile.RemoteDesktop_RedirectPrinters;
            else if (group.RemoteDesktop_OverrideRedirectPrinters)
                info.RedirectPrinters = group.RemoteDesktop_RedirectPrinters;

            // Experience
            if (profile.RemoteDesktop_OverridePersistentBitmapCaching)
                info.PersistentBitmapCaching = profile.RemoteDesktop_PersistentBitmapCaching;
            else if (group.RemoteDesktop_OverridePersistentBitmapCaching)
                info.PersistentBitmapCaching = group.RemoteDesktop_PersistentBitmapCaching;

            if (profile.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped)
                info.ReconnectIfTheConnectionIsDropped = profile.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
            else if (group.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped)
                info.ReconnectIfTheConnectionIsDropped = group.RemoteDesktop_ReconnectIfTheConnectionIsDropped;

            // Performance
            if (profile.RemoteDesktop_OverrideNetworkConnectionType)
                info.NetworkConnectionType = profile.RemoteDesktop_NetworkConnectionType;
            else if (group.RemoteDesktop_OverrideNetworkConnectionType)
                info.NetworkConnectionType = group.RemoteDesktop_NetworkConnectionType;

            if (profile.RemoteDesktop_OverrideDesktopBackground)
                info.DesktopBackground = profile.RemoteDesktop_DesktopBackground;
            else if (group.RemoteDesktop_OverrideDesktopBackground)
                info.DesktopBackground = group.RemoteDesktop_DesktopBackground;

            if (profile.RemoteDesktop_OverrideFontSmoothing)
                info.FontSmoothing = profile.RemoteDesktop_FontSmoothing;
            else if (group.RemoteDesktop_OverrideFontSmoothing)
                info.FontSmoothing = group.RemoteDesktop_FontSmoothing;

            if (profile.RemoteDesktop_OverrideDesktopComposition)
                info.DesktopComposition = profile.RemoteDesktop_DesktopComposition;
            else if (group.RemoteDesktop_OverrideDesktopComposition)
                info.DesktopComposition = group.RemoteDesktop_DesktopComposition;

            if (profile.RemoteDesktop_OverrideShowWindowContentsWhileDragging)
                info.ShowWindowContentsWhileDragging = profile.RemoteDesktop_ShowWindowContentsWhileDragging;
            else if (group.RemoteDesktop_OverrideShowWindowContentsWhileDragging)
                info.ShowWindowContentsWhileDragging = group.RemoteDesktop_ShowWindowContentsWhileDragging;

            if (profile.RemoteDesktop_OverrideMenuAndWindowAnimation)
                info.MenuAndWindowAnimation = profile.RemoteDesktop_MenuAndWindowAnimation;
            else if (group.RemoteDesktop_OverrideMenuAndWindowAnimation)
                info.MenuAndWindowAnimation = group.RemoteDesktop_MenuAndWindowAnimation;

            if (profile.RemoteDesktop_OverrideVisualStyles)
                info.VisualStyles = profile.RemoteDesktop_VisualStyles;
            else if (group.RemoteDesktop_OverrideVisualStyles)
                info.VisualStyles = group.RemoteDesktop_VisualStyles;

            // Set credentials
            if (profile.RemoteDesktop_UseCredentials)
            {
                info.CustomCredentials = true;

                info.Username = profile.RemoteDesktop_Username;
                info.Password = profile.RemoteDesktop_Password;
            }
            else if (group.RemoteDesktop_UseCredentials)
            {
                info.CustomCredentials = true;

                info.Username = group.RemoteDesktop_Username;
                info.Password = group.RemoteDesktop_Password;
            }

            return info;
        }
    }
}
