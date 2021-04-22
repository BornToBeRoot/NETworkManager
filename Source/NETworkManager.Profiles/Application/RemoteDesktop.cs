﻿using NETworkManager.Profiles;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application
{
    public static class RemoteDesktop
    {
        public static RemoteDesktopSessionInfo CreateSessionInfo(ProfileInfo profileInfo = null)
        {
            if (profileInfo == null)
                return new RemoteDesktopSessionInfo();

            var info = new RemoteDesktopSessionInfo
            {
                // Hostname
                Hostname = profileInfo.Host,

                // Display
                AdjustScreenAutomatically = profileInfo.RemoteDesktop_OverrideDisplay ? profileInfo.RemoteDesktop_AdjustScreenAutomatically : SettingsManager.Current.RemoteDesktop_AdjustScreenAutomatically,
                UseCurrentViewSize = profileInfo.RemoteDesktop_OverrideDisplay ? profileInfo.RemoteDesktop_UseCurrentViewSize : SettingsManager.Current.RemoteDesktop_UseCurrentViewSize,
                DesktopWidth = profileInfo.RemoteDesktop_OverrideDisplay ? (profileInfo.RemoteDesktop_UseCustomScreenSize ? profileInfo.RemoteDesktop_CustomScreenWidth : profileInfo.RemoteDesktop_ScreenWidth) : (SettingsManager.Current.RemoteDesktop_UseCustomScreenSize ? SettingsManager.Current.RemoteDesktop_CustomScreenWidth : SettingsManager.Current.RemoteDesktop_ScreenWidth),
                DesktopHeight = profileInfo.RemoteDesktop_OverrideDisplay ? (profileInfo.RemoteDesktop_UseCustomScreenSize ? profileInfo.RemoteDesktop_CustomScreenHeight : profileInfo.RemoteDesktop_ScreenHeight) : (SettingsManager.Current.RemoteDesktop_UseCustomScreenSize ? SettingsManager.Current.RemoteDesktop_CustomScreenHeight : SettingsManager.Current.RemoteDesktop_ScreenHeight),
                ColorDepth = profileInfo.RemoteDesktop_OverrideColorDepth ? profileInfo.RemoteDesktop_ColorDepth : SettingsManager.Current.RemoteDesktop_ColorDepth,

                // Network
                Port = profileInfo.RemoteDesktop_OverridePort ? profileInfo.RemoteDesktop_Port : SettingsManager.Current.RemoteDesktop_Port,

                // Authentication
                EnableCredSspSupport = profileInfo.RemoteDesktop_OverrideCredSspSupport ? profileInfo.RemoteDesktop_EnableCredSspSupport : SettingsManager.Current.RemoteDesktop_EnableCredSspSupport,
                AuthenticationLevel = profileInfo.RemoteDesktop_OverrideAuthenticationLevel ? profileInfo.RemoteDesktop_AuthenticationLevel : SettingsManager.Current.RemoteDesktop_AuthenticationLevel,

                // Remote audio
                AudioRedirectionMode = profileInfo.RemoteDesktop_OverrideAudioRedirectionMode ? profileInfo.RemoteDesktop_AudioRedirectionMode : SettingsManager.Current.RemoteDesktop_AudioRedirectionMode,
                AudioCaptureRedirectionMode = profileInfo.RemoteDesktop_OverrideAudioCaptureRedirectionMode ? profileInfo.RemoteDesktop_AudioCaptureRedirectionMode : SettingsManager.Current.RemoteDesktop_AudioCaptureRedirectionMode,

                // Keyboard
                KeyboardHookMode = profileInfo.RemoteDesktop_OverrideApplyWindowsKeyCombinations ? profileInfo.RemoteDesktop_KeyboardHookMode : SettingsManager.Current.RemoteDesktop_KeyboardHookMode,

                // Local devices and resources
                RedirectClipboard = profileInfo.RemoteDesktop_OverrideRedirectClipboard ? profileInfo.RemoteDesktop_RedirectClipboard : SettingsManager.Current.RemoteDesktop_RedirectClipboard,
                RedirectDevices = profileInfo.RemoteDesktop_OverrideRedirectDevices ? profileInfo.RemoteDesktop_RedirectDevices : SettingsManager.Current.RemoteDesktop_RedirectDevices,
                RedirectDrives = profileInfo.RemoteDesktop_OverrideRedirectDrives ? profileInfo.RemoteDesktop_RedirectDrives : SettingsManager.Current.RemoteDesktop_RedirectDrives,
                RedirectPorts = profileInfo.RemoteDesktop_OverrideRedirectPorts ? profileInfo.RemoteDesktop_RedirectPorts : SettingsManager.Current.RemoteDesktop_RedirectPorts,
                RedirectSmartCards = profileInfo.RemoteDesktop_OverrideRedirectSmartcards ? profileInfo.RemoteDesktop_RedirectSmartCards : SettingsManager.Current.RemoteDesktop_RedirectSmartCards,
                RedirectPrinters = profileInfo.RemoteDesktop_OverrideRedirectPrinters ? profileInfo.RemoteDesktop_RedirectPrinters : SettingsManager.Current.RemoteDesktop_RedirectPrinters,

                // Experience
                PersistentBitmapCaching = profileInfo.RemoteDesktop_OverridePersistentBitmapCaching ? profileInfo.RemoteDesktop_PersistentBitmapCaching : SettingsManager.Current.RemoteDesktop_PersistentBitmapCaching,
                ReconnectIfTheConnectionIsDropped = profileInfo.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped ? profileInfo.RemoteDesktop_ReconnectIfTheConnectionIsDropped : SettingsManager.Current.RemoteDesktop_ReconnectIfTheConnectionIsDropped,

                // Performance
                NetworkConnectionType = profileInfo.RemoteDesktop_OverrideNetworkConnectionType ? profileInfo.RemoteDesktop_NetworkConnectionType : SettingsManager.Current.RemoteDesktop_NetworkConnectionType,
                DesktopBackground = profileInfo.RemoteDesktop_OverrideDesktopBackground ? profileInfo.RemoteDesktop_DesktopBackground : SettingsManager.Current.RemoteDesktop_DesktopBackground,
                FontSmoothing = profileInfo.RemoteDesktop_OverrideFontSmoothing ? profileInfo.RemoteDesktop_FontSmoothing : SettingsManager.Current.RemoteDesktop_FontSmoothing,
                DesktopComposition = profileInfo.RemoteDesktop_OverrideDesktopComposition ? profileInfo.RemoteDesktop_DesktopComposition : SettingsManager.Current.RemoteDesktop_DesktopComposition,
                ShowWindowContentsWhileDragging = profileInfo.RemoteDesktop_OverrideShowWindowContentsWhileDragging ? profileInfo.RemoteDesktop_ShowWindowContentsWhileDragging : SettingsManager.Current.RemoteDesktop_ShowWindowContentsWhileDragging,
                MenuAndWindowAnimation = profileInfo.RemoteDesktop_OverrideMenuAndWindowAnimation ? profileInfo.RemoteDesktop_MenuAndWindowAnimation : SettingsManager.Current.RemoteDesktop_MenuAndWindowAnimation,
                VisualStyles = profileInfo.RemoteDesktop_OverrideVisualStyles ? profileInfo.RemoteDesktop_VisualStyles : SettingsManager.Current.RemoteDesktop_VisualStyles
            };

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
