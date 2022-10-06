using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;
using System.Collections.Generic;
using System.Security;
using System.Xml.Serialization;

namespace NETworkManager.Profiles
{
    /// <summary>
    /// Class represents a group.
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// Name of the group.
        /// </summary>
        public string Name { get; set; }

        public bool IsDynamic { get; set; }

        [XmlIgnore]
        public new List<ProfileInfo> Profiles { get; set; }

        public bool RemoteDesktop_UseCredentials { get; set; }
        public string RemoteDesktop_Username { get; set; }

        [XmlIgnore]
        public SecureString RemoteDesktop_Password { get; set; }
        public bool RemoteDesktop_OverrideDisplay { get; set; }
        public bool RemoteDesktop_AdjustScreenAutomatically { get; set; }
        public bool RemoteDesktop_UseCurrentViewSize { get; set; }
        public bool RemoteDesktop_UseFixedScreenSize { get; set; } = true;
        public int RemoteDesktop_ScreenWidth { get; set; } = GlobalStaticConfiguration.RemoteDesktop_ScreenWidth;
        public int RemoteDesktop_ScreenHeight { get; set; } = GlobalStaticConfiguration.RemoteDesktop_ScreenHeight;
        public bool RemoteDesktop_UseCustomScreenSize { get; set; }
        public int RemoteDesktop_CustomScreenWidth { get; set; }
        public int RemoteDesktop_CustomScreenHeight { get; set; }
        public bool RemoteDesktop_OverrideColorDepth { get; set; }
        public int RemoteDesktop_ColorDepth { get; set; } = GlobalStaticConfiguration.RemoteDesktop_ColorDepth;
        public bool RemoteDesktop_OverridePort { get; set; }
        public int RemoteDesktop_Port { get; set; } = GlobalStaticConfiguration.RemoteDesktop_Port;
        public bool RemoteDesktop_OverrideCredSspSupport { get; set; }
        public bool RemoteDesktop_EnableCredSspSupport { get; set; }
        public bool RemoteDesktop_OverrideAuthenticationLevel { get; set; }
        public uint RemoteDesktop_AuthenticationLevel { get; set; } = GlobalStaticConfiguration.RemoteDesktop_AuthenticationLevel;
        public bool RemoteDesktop_OverrideAudioRedirectionMode { get; set; }
        public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode { get; set; } = GlobalStaticConfiguration.RemoteDesktop_AudioRedirectionMode;
        public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode { get; set; }
        public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode { get; set; } = GlobalStaticConfiguration.RemoteDesktop_AudioCaptureRedirectionMode;
        public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations { get; set; }
        public KeyboardHookMode RemoteDesktop_KeyboardHookMode { get; set; } = GlobalStaticConfiguration.RemoteDesktop_KeyboardHookMode;
        public bool RemoteDesktop_OverrideRedirectClipboard { get; set; }
        public bool RemoteDesktop_RedirectClipboard { get; set; } = true;
        public bool RemoteDesktop_OverrideRedirectDevices { get; set; }
        public bool RemoteDesktop_RedirectDevices { get; set; }
        public bool RemoteDesktop_OverrideRedirectDrives { get; set; }
        public bool RemoteDesktop_RedirectDrives { get; set; }
        public bool RemoteDesktop_OverrideRedirectPorts { get; set; }
        public bool RemoteDesktop_RedirectPorts { get; set; }
        public bool RemoteDesktop_OverrideRedirectSmartcards { get; set; }
        public bool RemoteDesktop_RedirectSmartCards { get; set; }
        public bool RemoteDesktop_OverrideRedirectPrinters { get; set; }
        public bool RemoteDesktop_RedirectPrinters { get; set; }
        public bool RemoteDesktop_OverridePersistentBitmapCaching { get; set; }
        public bool RemoteDesktop_PersistentBitmapCaching { get; set; }
        public bool RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped { get; set; }
        public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped { get; set; }
        public bool RemoteDesktop_OverrideNetworkConnectionType { get; set; }
        public NetworkConnectionType RemoteDesktop_NetworkConnectionType { get; set; }
        public bool RemoteDesktop_OverrideDesktopBackground { get; set; }
        public bool RemoteDesktop_DesktopBackground { get; set; }
        public bool RemoteDesktop_OverrideFontSmoothing { get; set; }
        public bool RemoteDesktop_FontSmoothing { get; set; }
        public bool RemoteDesktop_OverrideDesktopComposition { get; set; }
        public bool RemoteDesktop_DesktopComposition { get; set; }
        public bool RemoteDesktop_OverrideShowWindowContentsWhileDragging { get; set; }
        public bool RemoteDesktop_ShowWindowContentsWhileDragging { get; set; }
        public bool RemoteDesktop_OverrideMenuAndWindowAnimation { get; set; }
        public bool RemoteDesktop_MenuAndWindowAnimation { get; set; }
        public bool RemoteDesktop_OverrideVisualStyles { get; set; }
        public bool RemoteDesktop_VisualStyles { get; set; }

        public bool PowerShell_OverrideCommand { get; set; }
        public string PowerShell_Command { get; set; }
        public bool PowerShell_OverrideAdditionalCommandLine { get; set; }
        public string PowerShell_AdditionalCommandLine { get; set; }
        public bool PowerShell_OverrideExecutionPolicy { get; set; }
        public PowerShell.ExecutionPolicy PowerShell_ExecutionPolicy { get; set; }

        public bool PuTTY_OverrideUsername { get; set; }
        public string PuTTY_Username { get; set; }
        public bool PuTTY_OverridePrivateKeyFile { get; set; }
        public string PuTTY_PrivateKeyFile { get; set; }
        public bool PuTTY_OverrideProfile { get; set; }
        public string PuTTY_Profile { get; set; }
        public bool PuTTY_OverrideEnableLog { get; set; }
        public bool PuTTY_EnableLog { get; set; }
        public bool PuTTY_OverrideLogMode { get; set; }
        public LogMode PuTTY_LogMode { get; set; } = GlobalStaticConfiguration.PuTTY_LogMode;
        public bool PuTTY_OverrideLogPath { get; set; }
        public string PuTTY_LogPath { get; set; } = GlobalStaticConfiguration.PuTTY_LogPath;
        public bool PuTTY_OverrideLogFileName { get; set; }
        public string PuTTY_LogFileName { get; set; } = GlobalStaticConfiguration.PuTTY_LogFileName;
        public bool PuTTY_OverrideAdditionalCommandLine { get; set; }
        public string PuTTY_AdditionalCommandLine { get; set; }

        public bool AWSSessionManager_OverrideProfile { get; set; }
        public string AWSSessionManager_Profile { get; set; }
        public bool AWSSessionManager_OverrideRegion { get; set; }
        public string AWSSessionManager_Region { get; set; }

        public bool TigerVNC_OverridePort { get; set; }
        public int TigerVNC_Port { get; set; }


        /// <summary>
        /// Initializes a new instance of the<see cref="GroupInfo"/> class.
        /// </summary>
        public GroupInfo()
        {
            Profiles = new List<ProfileInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="GroupInfo"/> class with name.
        /// </summary>
        public GroupInfo(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="GroupInfo"/> class with properties.
        /// </summary>
        public GroupInfo(GroupInfo group) : this(group.Name)
        {
            Profiles = group.Profiles;

            // Remote Desktop
            RemoteDesktop_UseCredentials = group.RemoteDesktop_UseCredentials;
            RemoteDesktop_Username = group.RemoteDesktop_Username;
            RemoteDesktop_Password = group.RemoteDesktop_Password;
            RemoteDesktop_OverrideDisplay = group.RemoteDesktop_OverrideDisplay;
            RemoteDesktop_AdjustScreenAutomatically = group.RemoteDesktop_AdjustScreenAutomatically;
            RemoteDesktop_UseCurrentViewSize = group.RemoteDesktop_UseCurrentViewSize;
            RemoteDesktop_UseFixedScreenSize = group.RemoteDesktop_UseFixedScreenSize;
            RemoteDesktop_ScreenWidth = group.RemoteDesktop_ScreenWidth;
            RemoteDesktop_ScreenHeight = group.RemoteDesktop_ScreenHeight;
            RemoteDesktop_UseCustomScreenSize = group.RemoteDesktop_UseCustomScreenSize;
            RemoteDesktop_CustomScreenWidth = group.RemoteDesktop_CustomScreenWidth;
            RemoteDesktop_CustomScreenHeight = group.RemoteDesktop_CustomScreenHeight;
            RemoteDesktop_OverrideColorDepth = group.RemoteDesktop_OverrideColorDepth;
            RemoteDesktop_ColorDepth = group.RemoteDesktop_ColorDepth;
            RemoteDesktop_OverridePort = group.RemoteDesktop_OverridePort;
            RemoteDesktop_Port = group.RemoteDesktop_Port;
            RemoteDesktop_OverrideCredSspSupport = group.RemoteDesktop_OverrideCredSspSupport;
            RemoteDesktop_EnableCredSspSupport = group.RemoteDesktop_EnableCredSspSupport;
            RemoteDesktop_OverrideAuthenticationLevel = group.RemoteDesktop_OverrideAuthenticationLevel;
            RemoteDesktop_AuthenticationLevel = group.RemoteDesktop_AuthenticationLevel;
            RemoteDesktop_OverrideAudioRedirectionMode = group.RemoteDesktop_OverrideAudioRedirectionMode;
            RemoteDesktop_AudioRedirectionMode = group.RemoteDesktop_AudioRedirectionMode;
            RemoteDesktop_OverrideAudioCaptureRedirectionMode = group.RemoteDesktop_OverrideAudioCaptureRedirectionMode;
            RemoteDesktop_AudioCaptureRedirectionMode = group.RemoteDesktop_AudioCaptureRedirectionMode;
            RemoteDesktop_OverrideApplyWindowsKeyCombinations = group.RemoteDesktop_OverrideApplyWindowsKeyCombinations;
            RemoteDesktop_KeyboardHookMode = group.RemoteDesktop_KeyboardHookMode;
            RemoteDesktop_OverrideRedirectClipboard = group.RemoteDesktop_OverrideRedirectClipboard;
            RemoteDesktop_RedirectClipboard = group.RemoteDesktop_RedirectClipboard;
            RemoteDesktop_OverrideRedirectDevices = group.RemoteDesktop_OverrideRedirectDevices;
            RemoteDesktop_RedirectDevices = group.RemoteDesktop_RedirectDevices;
            RemoteDesktop_OverrideRedirectDrives = group.RemoteDesktop_OverrideRedirectDrives;
            RemoteDesktop_RedirectDrives = group.RemoteDesktop_RedirectDrives;
            RemoteDesktop_OverrideRedirectPorts = group.RemoteDesktop_OverrideRedirectPorts;
            RemoteDesktop_RedirectPorts = group.RemoteDesktop_RedirectPorts;
            RemoteDesktop_OverrideRedirectSmartcards = group.RemoteDesktop_OverrideRedirectSmartcards;
            RemoteDesktop_RedirectSmartCards = group.RemoteDesktop_RedirectSmartCards;
            RemoteDesktop_OverrideRedirectPrinters = group.RemoteDesktop_OverrideRedirectPrinters;
            RemoteDesktop_RedirectPrinters = group.RemoteDesktop_RedirectPrinters;
            RemoteDesktop_OverridePersistentBitmapCaching = group.RemoteDesktop_OverridePersistentBitmapCaching;
            RemoteDesktop_PersistentBitmapCaching = group.RemoteDesktop_PersistentBitmapCaching;
            RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped = group.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
            RemoteDesktop_ReconnectIfTheConnectionIsDropped = group.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
            RemoteDesktop_OverrideNetworkConnectionType = group.RemoteDesktop_OverrideNetworkConnectionType;
            RemoteDesktop_NetworkConnectionType = group.RemoteDesktop_NetworkConnectionType;
            RemoteDesktop_OverrideDesktopBackground = group.RemoteDesktop_OverrideDesktopBackground;
            RemoteDesktop_DesktopBackground = group.RemoteDesktop_DesktopBackground;
            RemoteDesktop_OverrideFontSmoothing = group.RemoteDesktop_OverrideFontSmoothing;
            RemoteDesktop_FontSmoothing = group.RemoteDesktop_FontSmoothing;
            RemoteDesktop_OverrideDesktopComposition = group.RemoteDesktop_OverrideDesktopComposition;
            RemoteDesktop_DesktopComposition = group.RemoteDesktop_DesktopComposition;
            RemoteDesktop_OverrideShowWindowContentsWhileDragging = group.RemoteDesktop_OverrideShowWindowContentsWhileDragging;
            RemoteDesktop_ShowWindowContentsWhileDragging = group.RemoteDesktop_ShowWindowContentsWhileDragging;
            RemoteDesktop_OverrideMenuAndWindowAnimation = group.RemoteDesktop_OverrideMenuAndWindowAnimation;
            RemoteDesktop_MenuAndWindowAnimation = group.RemoteDesktop_MenuAndWindowAnimation;
            RemoteDesktop_OverrideVisualStyles = group.RemoteDesktop_OverrideVisualStyles;
            RemoteDesktop_VisualStyles = group.RemoteDesktop_VisualStyles;

            // PowerShell
            PowerShell_OverrideCommand = group.PowerShell_OverrideCommand;
            PowerShell_Command = group.PowerShell_Command;
            PowerShell_OverrideAdditionalCommandLine = group.PowerShell_OverrideAdditionalCommandLine;
            PowerShell_AdditionalCommandLine = group.PowerShell_AdditionalCommandLine;
            PowerShell_OverrideExecutionPolicy = group.PowerShell_OverrideExecutionPolicy;
            PowerShell_ExecutionPolicy = group.PowerShell_ExecutionPolicy;

            // PuTTY
            PuTTY_OverrideUsername = group.PuTTY_OverrideUsername;
            PuTTY_Username = group.PuTTY_Username;
            PuTTY_OverridePrivateKeyFile = group.PuTTY_OverridePrivateKeyFile;
            PuTTY_PrivateKeyFile = group.PuTTY_PrivateKeyFile;
            PuTTY_OverrideProfile = group.PuTTY_OverrideProfile;
            PuTTY_Profile = group.PuTTY_Profile;
            PuTTY_OverrideEnableLog = group.PuTTY_OverrideEnableLog;
            PuTTY_EnableLog = group.PuTTY_EnableLog;
            PuTTY_OverrideLogMode = group.PuTTY_OverrideLogMode;
            PuTTY_LogMode = group.PuTTY_LogMode;
            PuTTY_OverrideLogPath = group.PuTTY_OverrideLogPath;
            PuTTY_LogPath = group.PuTTY_LogPath;
            PuTTY_OverrideLogFileName = group.PuTTY_OverrideLogFileName;
            PuTTY_LogFileName = group.PuTTY_LogFileName;
            PuTTY_OverrideAdditionalCommandLine = group.PuTTY_OverrideAdditionalCommandLine;
            PuTTY_AdditionalCommandLine = group.PuTTY_AdditionalCommandLine;

            // AWS Session Manager
            AWSSessionManager_OverrideProfile = group.AWSSessionManager_OverrideProfile;
            AWSSessionManager_Profile = group.AWSSessionManager_Profile;
            AWSSessionManager_OverrideRegion = group.AWSSessionManager_OverrideRegion;
            AWSSessionManager_Region = group.AWSSessionManager_Region;

            // TigerVNC
            TigerVNC_OverridePort = group.TigerVNC_OverridePort;
            TigerVNC_Port = group.TigerVNC_Port;
        }
    }
}
