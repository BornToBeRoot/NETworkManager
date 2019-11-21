using System;
using System.Diagnostics.CodeAnalysis;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.Profile
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ProfileInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public bool NetworkInterface_Enabled { get; set; }
        public bool NetworkInterface_EnableStaticIPAddress { get; set; }
        public string NetworkInterface_IPAddress { get; set; }
        public string NetworkInterface_SubnetmaskOrCidr { get; set; }
        public string NetworkInterface_Gateway { get; set; }
        public bool NetworkInterface_EnableStaticDNS { get; set; }
        public string NetworkInterface_PrimaryDNSServer { get; set; }
        public string NetworkInterface_SecondaryDNSServer { get; set; }

        public bool IPScanner_Enabled { get; set; }
        public bool IPScanner_InheritHost { get; set; } = true;
        public string IPScanner_HostOrIPRange { get; set; }

        public bool PortScanner_Enabled { get; set; }
        public bool PortScanner_InheritHost { get; set; } = true;
        public string PortScanner_Host { get; set; }
        public string PortScanner_Ports { get; set; }

        public bool Ping_Enabled { get; set; }
        public bool Ping_InheritHost { get; set; } = true;
        public string Ping_Host { get; set; }

        public bool PingMonitor_Enabled { get; set; }
        public bool PingMonitor_InheritHost { get; set; } = true;
        public string PingMonitor_Host { get; set; }

        public bool Traceroute_Enabled { get; set; }
        public bool Traceroute_InheritHost { get; set; } = true;
        public string Traceroute_Host { get; set; }

        public bool DNSLookup_Enabled { get; set; }
        public bool DNSLookup_InheritHost { get; set; } = true;
        public string DNSLookup_Host { get; set; }

        public bool RemoteDesktop_Enabled { get; set; }
        public bool RemoteDesktop_InheritHost { get; set; } = true;
        public string RemoteDesktop_Host { get; set; }
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
        public RemoteDesktop.RemoteDesktop.AudioRedirectionMode RemoteDesktop_AudioRedirectionMode { get; set; } = GlobalStaticConfiguration.RemoteDesktop_AudioRedirectionMode;
        public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode { get; set; }
        public RemoteDesktop.RemoteDesktop.AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode { get; set; } = GlobalStaticConfiguration.RemoteDesktop_AudioCaptureRedirectionMode;
        public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations { get; set; }
        public RemoteDesktop.RemoteDesktop.KeyboardHookMode RemoteDesktop_KeyboardHookMode { get; set; } = GlobalStaticConfiguration.RemoteDesktop_KeyboardHookMode;
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
        public RemoteDesktop.RemoteDesktop.NetworkConnectionType RemoteDesktop_NetworkConnectionType { get; set; }
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
        
        public bool PowerShell_Enabled { get; set; }
        public bool PowerShell_EnableRemoteConsole { get; set; } = true;
        public bool PowerShell_InheritHost { get; set; } = true;
        public string PowerShell_Host { get; set; }
        public bool PowerShell_OverrideAdditionalCommandLine { get; set; }
        public string PowerShell_AdditionalCommandLine { get; set; }
        public bool PowerShell_OverrideExecutionPolicy { get; set; }
        public PowerShell.PowerShell.ExecutionPolicy PowerShell_ExecutionPolicy { get; set; }

        public bool PuTTY_Enabled { get; set; }
        public ConnectionMode PuTTY_ConnectionMode { get; set; }
        public bool PuTTY_InheritHost { get; set; } = true;
        public string PuTTY_HostOrSerialLine { get; set; }
        public bool PuTTY_OverridePortOrBaud { get; set; }
        public int PuTTY_PortOrBaud { get; set; }
        public bool PuTTY_OverrideUsername { get; set; }
        public string PuTTY_Username { get; set; }
        public bool PuTTY_OverrideProfile { get; set; }
        public string PuTTY_Profile { get; set; }
        public bool PuTTY_OverrideAdditionalCommandLine { get; set; }
        public string PuTTY_AdditionalCommandLine { get; set; }

        public bool TigerVNC_Enabled { get; set; }
        public bool TigerVNC_InheritHost { get; set; } = true;
        public string TigerVNC_Host { get; set; }
        public bool TigerVNC_OverridePort { get; set; }
        public int TigerVNC_Port { get; set; }

        public bool WakeOnLAN_Enabled { get; set; }
        public string WakeOnLAN_MACAddress { get; set; }
        public string WakeOnLAN_Broadcast { get; set; }
        public bool WakeOnLAN_OverridePort { get; set; }
        public int WakeOnLAN_Port { get; set; }

        public bool HTTPHeaders_Enabled { get; set; }
        public string HTTPHeaders_Website { get; set; }

        public bool Whois_Enabled { get; set; }
        public bool Whois_InheritHost { get; set; } = true;
        public string Whois_Domain { get; set; }

        public ProfileInfo()
        {

        }
    }
}
