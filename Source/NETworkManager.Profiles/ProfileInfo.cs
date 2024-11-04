﻿using System.Security;
using System.Xml.Serialization;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;

// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace NETworkManager.Profiles;

/// <summary>
///     Class represents a profile.
/// </summary>
public class ProfileInfo
{
    /// <summary>
    ///     Initializes a new instance of the<see cref="ProfileInfo" /> class.
    /// </summary>
    public ProfileInfo()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the<see cref="ProfileInfo" /> class with properties.
    /// </summary>
    public ProfileInfo(ProfileInfo profile)
    {
        Name = profile.Name;
        Host = profile.Host;
        Description = profile.Description;
        Group = profile.Group;
        Tags = profile.Tags;

        IsDynamic = profile.IsDynamic;

        // Network Interface
        NetworkInterface_Enabled = profile.NetworkInterface_Enabled;
        NetworkInterface_EnableStaticIPAddress = profile.NetworkInterface_EnableStaticIPAddress;
        NetworkInterface_IPAddress = profile.NetworkInterface_IPAddress;
        NetworkInterface_Subnetmask = profile.NetworkInterface_Subnetmask;
        NetworkInterface_SubnetmaskOrCidr = profile.NetworkInterface_SubnetmaskOrCidr;
        NetworkInterface_Gateway = profile.NetworkInterface_Gateway;
        NetworkInterface_EnableStaticDNS = profile.NetworkInterface_EnableStaticDNS;
        NetworkInterface_PrimaryDNSServer = profile.NetworkInterface_PrimaryDNSServer;
        NetworkInterface_SecondaryDNSServer = profile.NetworkInterface_SecondaryDNSServer;

        // IP Scanner
        IPScanner_Enabled = profile.IPScanner_Enabled;
        IPScanner_InheritHost = profile.IPScanner_InheritHost;
        IPScanner_HostOrIPRange = profile.IPScanner_HostOrIPRange;

        // Port Scanner
        PortScanner_Enabled = profile.PortScanner_Enabled;
        PortScanner_InheritHost = profile.PortScanner_InheritHost;
        PortScanner_Host = profile.PortScanner_Host;
        PortScanner_Ports = profile.PortScanner_Ports;

        // Ping Monitor
        PingMonitor_Enabled = profile.PingMonitor_Enabled;
        PingMonitor_InheritHost = profile.PingMonitor_InheritHost;
        PingMonitor_Host = profile.PingMonitor_Host;

        // Traceroute
        Traceroute_Enabled = profile.Traceroute_Enabled;
        Traceroute_InheritHost = profile.Traceroute_InheritHost;
        Traceroute_Host = profile.Traceroute_Host;

        // DNS Lookup
        DNSLookup_Enabled = profile.DNSLookup_Enabled;
        DNSLookup_InheritHost = profile.DNSLookup_InheritHost;
        DNSLookup_Host = profile.DNSLookup_Host;

        // Remote Desktop
        RemoteDesktop_Enabled = profile.RemoteDesktop_Enabled;
        RemoteDesktop_InheritHost = profile.RemoteDesktop_InheritHost;
        RemoteDesktop_Host = profile.RemoteDesktop_Host;
        RemoteDesktop_UseCredentials = profile.RemoteDesktop_UseCredentials;
        RemoteDesktop_Username = profile.RemoteDesktop_Username;
        RemoteDesktop_Domain = profile.RemoteDesktop_Domain;
        RemoteDesktop_Password = profile.RemoteDesktop_Password;
        RemoteDesktop_OverrideDisplay = profile.RemoteDesktop_OverrideDisplay;
        RemoteDesktop_AdjustScreenAutomatically = profile.RemoteDesktop_AdjustScreenAutomatically;
        RemoteDesktop_UseCurrentViewSize = profile.RemoteDesktop_UseCurrentViewSize;
        RemoteDesktop_UseFixedScreenSize = profile.RemoteDesktop_UseFixedScreenSize;
        RemoteDesktop_ScreenWidth = profile.RemoteDesktop_ScreenWidth;
        RemoteDesktop_ScreenHeight = profile.RemoteDesktop_ScreenHeight;
        RemoteDesktop_UseCustomScreenSize = profile.RemoteDesktop_UseCustomScreenSize;
        RemoteDesktop_CustomScreenWidth = profile.RemoteDesktop_CustomScreenWidth;
        RemoteDesktop_CustomScreenHeight = profile.RemoteDesktop_CustomScreenHeight;
        RemoteDesktop_OverrideColorDepth = profile.RemoteDesktop_OverrideColorDepth;
        RemoteDesktop_ColorDepth = profile.RemoteDesktop_ColorDepth;
        RemoteDesktop_OverridePort = profile.RemoteDesktop_OverridePort;
        RemoteDesktop_Port = profile.RemoteDesktop_Port;
        RemoteDesktop_OverrideCredSspSupport = profile.RemoteDesktop_OverrideCredSspSupport;
        RemoteDesktop_EnableCredSspSupport = profile.RemoteDesktop_EnableCredSspSupport;
        RemoteDesktop_OverrideAuthenticationLevel = profile.RemoteDesktop_OverrideAuthenticationLevel;
        RemoteDesktop_AuthenticationLevel = profile.RemoteDesktop_AuthenticationLevel;
        RemoteDesktop_OverrideGatewayServer = profile.RemoteDesktop_OverrideGatewayServer;
        RemoteDesktop_EnableGatewayServer = profile.RemoteDesktop_EnableGatewayServer;
        RemoteDesktop_GatewayServerHostname = profile.RemoteDesktop_GatewayServerHostname;
        RemoteDesktop_GatewayServerBypassLocalAddresses = profile.RemoteDesktop_GatewayServerBypassLocalAddresses;
        RemoteDesktop_GatewayServerLogonMethod = profile.RemoteDesktop_GatewayServerLogonMethod;
        RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer =
            profile.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;
        RemoteDesktop_UseGatewayServerCredentials = profile.RemoteDesktop_UseGatewayServerCredentials;
        RemoteDesktop_GatewayServerUsername = profile.RemoteDesktop_GatewayServerUsername;
        RemoteDesktop_GatewayServerDomain = profile.RemoteDesktop_GatewayServerDomain;
        RemoteDesktop_GatewayServerPassword = profile.RemoteDesktop_GatewayServerPassword;
        RemoteDesktop_OverrideAudioRedirectionMode = profile.RemoteDesktop_OverrideAudioRedirectionMode;
        RemoteDesktop_AudioRedirectionMode = profile.RemoteDesktop_AudioRedirectionMode;
        RemoteDesktop_OverrideAudioCaptureRedirectionMode = profile.RemoteDesktop_OverrideAudioCaptureRedirectionMode;
        RemoteDesktop_AudioCaptureRedirectionMode = profile.RemoteDesktop_AudioCaptureRedirectionMode;
        RemoteDesktop_OverrideApplyWindowsKeyCombinations = profile.RemoteDesktop_OverrideApplyWindowsKeyCombinations;
        RemoteDesktop_KeyboardHookMode = profile.RemoteDesktop_KeyboardHookMode;
        RemoteDesktop_OverrideRedirectClipboard = profile.RemoteDesktop_OverrideRedirectClipboard;
        RemoteDesktop_RedirectClipboard = profile.RemoteDesktop_RedirectClipboard;
        RemoteDesktop_OverrideRedirectDevices = profile.RemoteDesktop_OverrideRedirectDevices;
        RemoteDesktop_RedirectDevices = profile.RemoteDesktop_RedirectDevices;
        RemoteDesktop_OverrideRedirectDrives = profile.RemoteDesktop_OverrideRedirectDrives;
        RemoteDesktop_RedirectDrives = profile.RemoteDesktop_RedirectDrives;
        RemoteDesktop_OverrideRedirectPorts = profile.RemoteDesktop_OverrideRedirectPorts;
        RemoteDesktop_RedirectPorts = profile.RemoteDesktop_RedirectPorts;
        RemoteDesktop_OverrideRedirectSmartcards = profile.RemoteDesktop_OverrideRedirectSmartcards;
        RemoteDesktop_RedirectSmartCards = profile.RemoteDesktop_RedirectSmartCards;
        RemoteDesktop_OverrideRedirectPrinters = profile.RemoteDesktop_OverrideRedirectPrinters;
        RemoteDesktop_RedirectPrinters = profile.RemoteDesktop_RedirectPrinters;
        RemoteDesktop_OverridePersistentBitmapCaching = profile.RemoteDesktop_OverridePersistentBitmapCaching;
        RemoteDesktop_PersistentBitmapCaching = profile.RemoteDesktop_PersistentBitmapCaching;
        RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped =
            profile.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
        RemoteDesktop_ReconnectIfTheConnectionIsDropped = profile.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
        RemoteDesktop_OverrideNetworkConnectionType = profile.RemoteDesktop_OverrideNetworkConnectionType;
        RemoteDesktop_NetworkConnectionType = profile.RemoteDesktop_NetworkConnectionType;
        RemoteDesktop_OverrideDesktopBackground = profile.RemoteDesktop_OverrideDesktopBackground;
        RemoteDesktop_DesktopBackground = profile.RemoteDesktop_DesktopBackground;
        RemoteDesktop_OverrideFontSmoothing = profile.RemoteDesktop_OverrideFontSmoothing;
        RemoteDesktop_FontSmoothing = profile.RemoteDesktop_FontSmoothing;
        RemoteDesktop_OverrideDesktopComposition = profile.RemoteDesktop_OverrideDesktopComposition;
        RemoteDesktop_DesktopComposition = profile.RemoteDesktop_DesktopComposition;
        RemoteDesktop_OverrideShowWindowContentsWhileDragging =
            profile.RemoteDesktop_OverrideShowWindowContentsWhileDragging;
        RemoteDesktop_ShowWindowContentsWhileDragging = profile.RemoteDesktop_ShowWindowContentsWhileDragging;
        RemoteDesktop_OverrideMenuAndWindowAnimation = profile.RemoteDesktop_OverrideMenuAndWindowAnimation;
        RemoteDesktop_MenuAndWindowAnimation = profile.RemoteDesktop_MenuAndWindowAnimation;
        RemoteDesktop_OverrideVisualStyles = profile.RemoteDesktop_OverrideVisualStyles;
        RemoteDesktop_VisualStyles = profile.RemoteDesktop_VisualStyles;

        // PowerShell
        PowerShell_Enabled = profile.PowerShell_Enabled;
        PowerShell_EnableRemoteConsole = profile.PowerShell_EnableRemoteConsole;
        PowerShell_InheritHost = profile.PowerShell_InheritHost;
        PowerShell_Host = profile.PowerShell_Host;
        PowerShell_OverrideCommand = profile.PowerShell_OverrideCommand;
        PowerShell_Command = profile.PowerShell_Command;
        PowerShell_OverrideAdditionalCommandLine = profile.PowerShell_OverrideAdditionalCommandLine;
        PowerShell_AdditionalCommandLine = profile.PowerShell_AdditionalCommandLine;
        PowerShell_OverrideExecutionPolicy = profile.PowerShell_OverrideExecutionPolicy;
        PowerShell_ExecutionPolicy = profile.PowerShell_ExecutionPolicy;

        // PuTTY
        PuTTY_Enabled = profile.PuTTY_Enabled;
        PuTTY_ConnectionMode = profile.PuTTY_ConnectionMode;
        PuTTY_InheritHost = profile.PuTTY_InheritHost;
        PuTTY_HostOrSerialLine = profile.PuTTY_HostOrSerialLine;
        PuTTY_OverridePortOrBaud = profile.PuTTY_OverridePortOrBaud;
        PuTTY_PortOrBaud = profile.PuTTY_PortOrBaud;
        PuTTY_OverrideUsername = profile.PuTTY_OverrideUsername;
        PuTTY_Username = profile.PuTTY_Username;
        PuTTY_OverridePrivateKeyFile = profile.PuTTY_OverridePrivateKeyFile;
        PuTTY_PrivateKeyFile = profile.PuTTY_PrivateKeyFile;
        PuTTY_OverrideProfile = profile.PuTTY_OverrideProfile;
        PuTTY_Profile = profile.PuTTY_Profile;
        PuTTY_OverrideHostkey = profile.PuTTY_OverrideHostkey;
        PuTTY_Hostkey = profile.PuTTY_Hostkey;
        PuTTY_OverrideEnableLog = profile.PuTTY_OverrideEnableLog;
        PuTTY_EnableLog = profile.PuTTY_EnableLog;
        PuTTY_OverrideLogMode = profile.PuTTY_OverrideLogMode;
        PuTTY_LogMode = profile.PuTTY_LogMode;
        PuTTY_OverrideLogPath = profile.PuTTY_OverrideLogPath;
        PuTTY_LogPath = profile.PuTTY_LogPath;
        PuTTY_OverrideLogFileName = profile.PuTTY_OverrideLogFileName;
        PuTTY_LogFileName = profile.PuTTY_LogFileName;
        PuTTY_OverrideAdditionalCommandLine = profile.PuTTY_OverrideAdditionalCommandLine;
        PuTTY_AdditionalCommandLine = profile.PuTTY_AdditionalCommandLine;

        // AWS Session Manager
        AWSSessionManager_Enabled = profile.AWSSessionManager_Enabled;
        AWSSessionManager_InstanceID = profile.AWSSessionManager_InstanceID;
        AWSSessionManager_OverrideProfile = profile.AWSSessionManager_OverrideProfile;
        AWSSessionManager_Profile = profile.AWSSessionManager_Profile;
        AWSSessionManager_OverrideRegion = profile.AWSSessionManager_OverrideRegion;
        AWSSessionManager_Region = profile.AWSSessionManager_Region;

        // TigerVNC
        TigerVNC_Enabled = profile.TigerVNC_Enabled;
        TigerVNC_InheritHost = profile.TigerVNC_InheritHost;
        TigerVNC_Host = profile.TigerVNC_Host;
        TigerVNC_OverridePort = profile.TigerVNC_OverridePort;
        TigerVNC_Port = profile.TigerVNC_Port;

        // WebConsole
        WebConsole_Enabled = profile.WebConsole_Enabled;
        WebConsole_Url = profile.WebConsole_Url;

        // SNMP
        SNMP_Enabled = profile.SNMP_Enabled;
        SNMP_InheritHost = profile.SNMP_InheritHost;
        SNMP_Host = profile.SNMP_Host;
        SNMP_OverrideOIDAndMode = profile.SNMP_OverrideOIDAndMode;
        SNMP_OID = profile.SNMP_OID;
        SNMP_Mode = profile.SNMP_Mode;
        SNMP_OverrideVersionAndAuth = profile.SNMP_OverrideVersionAndAuth;
        SNMP_Version = profile.SNMP_Version;
        SNMP_Community = profile.SNMP_Community;
        SNMP_Security = profile.SNMP_Security;
        SNMP_Username = profile.SNMP_Username;
        SNMP_AuthenticationProvider = profile.SNMP_AuthenticationProvider;
        SNMP_Auth = profile.SNMP_Auth;
        SNMP_PrivacyProvider = profile.SNMP_PrivacyProvider;
        SNMP_Priv = profile.SNMP_Priv;

        // Wake on LAN
        WakeOnLAN_Enabled = profile.WakeOnLAN_Enabled;
        WakeOnLAN_MACAddress = profile.WakeOnLAN_MACAddress;
        WakeOnLAN_Broadcast = profile.WakeOnLAN_Broadcast;

        // Whois
        Whois_Enabled = profile.Whois_Enabled;
        Whois_InheritHost = profile.Whois_InheritHost;
        Whois_Domain = profile.Whois_Domain;

        // IP Geolocation
        IPGeolocation_Enabled = profile.IPGeolocation_Enabled;
        IPGeolocation_InheritHost = profile.IPGeolocation_InheritHost;
        IPGeolocation_Host = profile.IPGeolocation_Host;
    }

    /// <summary>
    ///     Name of the profile.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Hostname or IP address of the host.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     Description of the profile.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    ///     Name of the group. Profiles are grouped based on the name.
    /// </summary>
    public string Group { get; set; }
    
    /// <summary>
    ///     Tags to classify the profiles and to filter by it.
    /// </summary>
    public string Tags { get; set; }

    /// <summary>
    ///     Dynamic profiles (e.g. AWS) are not save to file.
    /// </summary>
    [XmlIgnore]
    public bool IsDynamic { get; set; }

    public bool NetworkInterface_Enabled { get; set; }
    public bool NetworkInterface_EnableStaticIPAddress { get; set; }
    public string NetworkInterface_IPAddress { get; set; }

    //[Obsolete("Replaced by NetworkInterface_Subnetmask")]
    public string NetworkInterface_SubnetmaskOrCidr { get; set; }
    public string NetworkInterface_Subnetmask { get; set; }
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
    public bool RemoteDesktop_UseCredentials { get; set; }
    public string RemoteDesktop_Username { get; set; }
    public string RemoteDesktop_Domain { get; set; }

    [XmlIgnore] public SecureString RemoteDesktop_Password { get; set; }
    public bool RemoteDesktop_OverrideDisplay { get; set; }
    public bool RemoteDesktop_AdjustScreenAutomatically { get; set; }

    public bool RemoteDesktop_UseCurrentViewSize { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_UseCurrentViewSize;

    public bool RemoteDesktop_UseFixedScreenSize { get; set; }
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

    public uint RemoteDesktop_AuthenticationLevel { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_AuthenticationLevel;

    public bool RemoteDesktop_OverrideGatewayServer { get; set; }
    public bool RemoteDesktop_EnableGatewayServer { get; set; }
    public string RemoteDesktop_GatewayServerHostname { get; set; }

    public bool RemoteDesktop_GatewayServerBypassLocalAddresses { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_GatewayServerBypassLocalAddresses;

    public GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_GatewayServerLogonMethod;

    public bool RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer;

    public bool RemoteDesktop_UseGatewayServerCredentials { get; set; }
    public string RemoteDesktop_GatewayServerUsername { get; set; }
    public string RemoteDesktop_GatewayServerDomain { get; set; }

    [XmlIgnore] public SecureString RemoteDesktop_GatewayServerPassword { get; set; }
    public bool RemoteDesktop_OverrideAudioRedirectionMode { get; set; }

    public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_AudioRedirectionMode;

    public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode { get; set; }

    public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_AudioCaptureRedirectionMode;

    public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations { get; set; }

    public KeyboardHookMode RemoteDesktop_KeyboardHookMode { get; set; } =
        GlobalStaticConfiguration.RemoteDesktop_KeyboardHookMode;

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

    public bool PowerShell_Enabled { get; set; }
    public bool PowerShell_EnableRemoteConsole { get; set; } = true;
    public bool PowerShell_InheritHost { get; set; } = true;
    public string PowerShell_Host { get; set; }
    public bool PowerShell_OverrideCommand { get; set; }
    public string PowerShell_Command { get; set; }
    public bool PowerShell_OverrideAdditionalCommandLine { get; set; }
    public string PowerShell_AdditionalCommandLine { get; set; }
    public bool PowerShell_OverrideExecutionPolicy { get; set; }

    public ExecutionPolicy PowerShell_ExecutionPolicy { get; set; } =
        GlobalStaticConfiguration.PowerShell_ExecutionPolicy;

    public bool PuTTY_Enabled { get; set; }
    public ConnectionMode PuTTY_ConnectionMode { get; set; }
    public bool PuTTY_InheritHost { get; set; } = true;
    public string PuTTY_HostOrSerialLine { get; set; }
    public bool PuTTY_OverridePortOrBaud { get; set; }
    public int PuTTY_PortOrBaud { get; set; }
    public bool PuTTY_OverrideUsername { get; set; }
    public string PuTTY_Username { get; set; }
    public bool PuTTY_OverridePrivateKeyFile { get; set; }
    public string PuTTY_PrivateKeyFile { get; set; }
    public bool PuTTY_OverrideProfile { get; set; }
    public string PuTTY_Profile { get; set; }
    public bool PuTTY_OverrideHostkey { get; set; }
    public string PuTTY_Hostkey { get; set; }
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

    public bool AWSSessionManager_Enabled { get; set; }
    public string AWSSessionManager_InstanceID { get; set; }
    public bool AWSSessionManager_OverrideProfile { get; set; }
    public string AWSSessionManager_Profile { get; set; }
    public bool AWSSessionManager_OverrideRegion { get; set; }
    public string AWSSessionManager_Region { get; set; }

    public bool TigerVNC_Enabled { get; set; }
    public bool TigerVNC_InheritHost { get; set; } = true;
    public string TigerVNC_Host { get; set; }
    public bool TigerVNC_OverridePort { get; set; }
    public int TigerVNC_Port { get; set; } = GlobalStaticConfiguration.TigerVNC_DefaultVNCPort;

    public bool WebConsole_Enabled { get; set; }
    public string WebConsole_Url { get; set; }

    public bool SNMP_Enabled { get; set; }
    public bool SNMP_InheritHost { get; set; } = true;
    public string SNMP_Host { get; set; }
    public bool SNMP_OverrideOIDAndMode { get; set; }
    public string SNMP_OID { get; set; }
    public SNMPMode SNMP_Mode { get; set; } = GlobalStaticConfiguration.SNMP_Mode;
    public bool SNMP_OverrideVersionAndAuth { get; set; }
    public SNMPVersion SNMP_Version { get; set; } = GlobalStaticConfiguration.SNMP_Version;
    [XmlIgnore] public SecureString SNMP_Community { get; set; }
    public SNMPV3Security SNMP_Security { get; set; } = GlobalStaticConfiguration.SNMP_Security;
    public string SNMP_Username { get; set; }

    public SNMPV3AuthenticationProvider SNMP_AuthenticationProvider { get; set; } =
        GlobalStaticConfiguration.SNMP_AuthenticationProvider;

    [XmlIgnore] public SecureString SNMP_Auth { get; set; }
    public SNMPV3PrivacyProvider SNMP_PrivacyProvider { get; set; } = GlobalStaticConfiguration.SNMP_PrivacyProvider;
    [XmlIgnore] public SecureString SNMP_Priv { get; set; }

    public bool WakeOnLAN_Enabled { get; set; }
    public string WakeOnLAN_MACAddress { get; set; }
    public string WakeOnLAN_Broadcast { get; set; }

    public bool Whois_Enabled { get; set; }
    public bool Whois_InheritHost { get; set; } = true;
    public string Whois_Domain { get; set; }

    public bool IPGeolocation_Enabled { get; set; }
    public bool IPGeolocation_InheritHost { get; set; } = true;
    public string IPGeolocation_Host { get; set; }
}