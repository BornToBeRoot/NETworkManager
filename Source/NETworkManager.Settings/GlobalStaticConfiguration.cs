using System;
using System.IO;
using System.Linq;
using DnsClient;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;

// ReSharper disable InconsistentNaming

namespace NETworkManager.Settings;

public static class GlobalStaticConfiguration
{
    #region Global settings

    // Type to search (average type speed --> 187 chars/min)
    public static TimeSpan SearchDispatcherTimerTimeSpan => new(0, 0, 0, 0, 750);

    // Status window delay in ms
    public static int StatusWindowDelayBeforeOpen => 5000;

    // Profile config
    public static bool Profile_ExpandProfileView => true;
    public static double Profile_WidthCollapsed => 40;
    public static double Profile_DefaultWidthExpanded => 250;
    public static double Profile_MaxWidthExpanded => 500;
    public static double Profile_FloatPointFix => 1.0;
    public static int Profile_EncryptionKeySize => 256;
    public static int Profile_EncryptionIterations => 1000000;

    // Filter for file dialog
    public static string ApplicationFileExtensionFilter => "Application (*.exe)|*.exe";
    public static string PuTTYPrivateKeyFileExtensionFilter => "PuTTY Private Key Files (*.ppk)|*.ppk";
    public static string ZipFileExtensionFilter => "ZIP Archive (*.zip)|*.zip";
    public static string XmlFileExtensionFilter => "XML-File (*.xml)|*.xml";

    #endregion

    #region Default settings

    // Settings: General
    public static ApplicationName General_DefaultApplicationViewName => ApplicationName.Dashboard;
    public static int General_BackgroundJobInterval => 5;
    public static int General_ThreadPoolAdditionalMinThreads => 512;
    public static int General_HistoryListEntries => 10;

    // Settings: Window
    public static bool SplashScreen_Enabled => true;

    // Settings: Appearance
    public static string Appearance_Theme => "Dark";
    public static string Appearance_Accent => "Lime";
    public static bool Appearance_UseCustomTheme => false;

    // Settings: Network
    public static bool Network_ResolveHostnamePreferIPv4 => true;

    // Settings: Status
    public static bool Status_ShowWindowOnNetworkChange => true;
    public static int Status_WindowCloseTime => 10;

    // HotKey
    public static int HotKey_ShowWindowKey => 79;
    public static int HotKey_ShowWindowModifier => 3;

    // Update
    public static bool Update_CheckForUpdatesAtStartup => true;

    public static bool Update_CheckForPreReleases => false;

    // Experimental
    public static bool Experimental_EnableExperimentalFeatures => false;

    // Application: Dashboard
    public static string Dashboard_PublicIPv4Address => "1.1.1.1";
    public static string Dashboard_PublicIPv6Address => "2606:4700:4700::1111";
    public static bool Dashboard_CheckPublicIPAddress => true;
    public static string Dashboard_PublicIPv4AddressAPI => "https://api.ipify.org";
    public static string Dashboard_PublicIPv6AddressAPI => "https://api6.ipify.org";
    public static bool Dashboard_CheckIPApiIPGeolocation => false;
    public static bool Dashboard_CheckIPApiDNSResolver => false;

    // Application: Network Interface
    public static ExportFileType NetworkInterface_ExportFileType => ExportFileType.Csv;

    // Application: WiFi
    public static bool WiFi_Show2dot4GHzNetworks => true;
    public static bool WiFi_Show5GHzNetworks => true;

    public static AutoRefreshTimeInfo WiFi_AutoRefreshTime =>
        AutoRefreshTime.GetDefaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);

    public static ExportFileType WiFi_ExportFileType => ExportFileType.Csv;

    // Application: IP Scanner    
    public static int IPScanner_ICMPAttempts => 2;
    public static int IPScanner_ICMPTimeout => 4000;
    public static int IPScanner_ICMPBuffer => 32;
    public static bool IPScanner_ResolveHostname => true;
    public static bool IPScanner_PortScanEnabled => true;
    public static string IPScanner_PortScanPorts => "22; 53; 80; 139; 389; 636; 443; 445; 3389";
    public static int IPScanner_PortScanTimeout => 4000;
    public static int IPScanner_MaxHostThreads => 256;
    public static int IPScanner_MaxPortThreads => 5;
    public static bool IPScanner_NetBIOSEnabled => true;
    public static int IPScanner_NetBIOSTimeout => 4000;
    public static ExportFileType IPScanner_ExportFileType => ExportFileType.Csv;

    // Application: Port Scanner 
    public static int PortScanner_MaxHostThreads => 5;
    public static int PortScanner_MaxPortThreads => 256;
    public static int PortScanner_Timeout => 4000;
    public static ExportFileType PortScanner_ExportFileType => ExportFileType.Csv;

    // Application: Ping Monitor
    public static int PingMonitor_Buffer => 32;
    public static bool PingMonitor_DontFragment => true;
    public static int PingMonitor_Timeout => 4000;
    public static int PingMonitor_TTL => 64;
    public static int PingMonitor_WaitTime => 1000;
    public static bool PingMonitor_ExpandHostView => false;
    public static ExportFileType PingMonitor_ExportFileType => ExportFileType.Csv;

    // Application: Traceroute
    public static int Traceroute_MaximumHops => 30;
    public static int Traceroute_Timeout => 4000;
    public static int Traceroute_Buffer => 32;
    public static bool Traceroute_ResolveHostname => true;
    public static bool Traceroute_CheckIPApiIPGeolocation => false;
    public static ExportFileType Traceroute_ExportFileType => ExportFileType.Csv;

    // Application: DNS Lookup
    public static QueryClass DNSLookup_QueryClass => QueryClass.IN;
    public static QueryType DNSLookup_QueryType => QueryType.ANY;
    public static bool DNSLookup_UseTCPOnly => false;
    public static int DNSLookup_Retries => 3;
    public static int DNSLookup_Timeout => 2;
    public static ExportFileType DNSLookup_ExportFileType => ExportFileType.Csv;

    // Application: RemoteDesktop
    public static bool RemoteDesktop_UseCurrentViewSize => true;
    public static int RemoteDesktop_ScreenWidth => 1280;
    public static int RemoteDesktop_ScreenHeight => 768;

    public static int RemoteDesktop_ColorDepth => 32;
    public static int RemoteDesktop_Port => 3389;
    public static bool RemoteDesktop_GatewayServerBypassLocalAddresses => true;

    public static GatewayUserSelectedCredsSource RemoteDesktop_GatewayServerLogonMethod =>
        GatewayUserSelectedCredsSource.Any;

    public static bool RemoteDesktop_GatewayServerShareCredentialsWithRemoteComputer => true;
    public static AudioRedirectionMode RemoteDesktop_AudioRedirectionMode => AudioRedirectionMode.PlayOnThisComputer;

    public static AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode =>
        AudioCaptureRedirectionMode.DoNotRecord;

    public static KeyboardHookMode RemoteDesktop_KeyboardHookMode => KeyboardHookMode.OnTheRemoteComputer;
    public static bool RemoteDesktop_RedirectClipboard => true;
    public static bool RemoteDesktop_EnableCredSspSupport => true;
    public static uint RemoteDesktop_AuthenticationLevel => 2;

    public static NetworkConnectionType RemoteDesktop_NetworkConnectionType =>
        NetworkConnectionType.DetectAutomatically;

    // Application: PowerShell
    public static string PowerShell_Command => "Set-Location ~";
    public static ExecutionPolicy PowerShell_ExecutionPolicy => ExecutionPolicy.RemoteSigned;

    // Application: PuTTY
    public static ConnectionMode PuTTY_DefaultConnectionMode => ConnectionMode.SSH;
    public static string PuTTY_DefaultProfile => "NETworkManager";
    public static LogMode PuTTY_LogMode => LogMode.SessionLog;

    public static string PuTTY_LogPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AssemblyManager.Current.Name, "PuTTY_Log");

    public static string PuTTY_LogFileName => "&H_&Y-&M-&D_&T.log";
    public static int PuTTY_SSHPort => 22;
    public static string PuTTY_SerialLine => "COM1";
    public static int PuTTY_TelnetPort => 23;
    public static int PuTTY_BaudRate => 9600;
    public static int PuTTY_RloginPort => 513;
    public static int PuTTY_RawPort => 23;

    // Application: AWSSessionManager
    public static bool AWSSessionManager_EnableSyncInstanceIDsFromAWS => false;
    public static bool AWSSessionManager_SyncOnlyRunningInstancesFromAWS => true;

    // Application: TigerVNC
    public static int TigerVNC_DefaultVNCPort => 5900;

    // Application: WebConsole
    public static bool WebConsole_ShowAddressBar => true;

    public static string WebConsole_Cache =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AssemblyManager.Current.Name, "WebConsole_Cache");

    // Application: SNMP
    public static WalkMode SNMP_WalkMode => WalkMode.WithinSubtree;
    public static int SNMP_Timeout => 60000;
    public static SNMPMode SNMP_Mode => SNMPMode.Walk;
    public static SNMPVersion SNMP_Version => SNMPVersion.V2C;
    public static SNMPV3Security SNMP_Security => SNMPV3Security.AuthPriv;
    public static SNMPV3AuthenticationProvider SNMP_AuthenticationProvider => SNMPV3AuthenticationProvider.SHA1;
    public static SNMPV3PrivacyProvider SNMP_PrivacyProvider => SNMPV3PrivacyProvider.AES;
    public static ExportFileType SNMP_ExportFileType => ExportFileType.Csv;

    // Application: SNTP Lookup
    public static int SNTPLookup_Timeout => 4000;
    public static ExportFileType SNTPLookup_ExportFileType => ExportFileType.Csv;

    // Application: Discovery Protocol
    public static DiscoveryProtocol DiscoveryProtocol_Protocol => DiscoveryProtocol.LldpCdp;
    public static int DiscoveryProtocol_Duration => 60;
    public static ExportFileType DiscoveryProtocol_ExportFileType => ExportFileType.Csv;

    // Application: Wake on LAN
    public static int WakeOnLAN_Port => 9;

    // Application: Subnet Calculator
    public static ExportFileType SubnetCalculator_Subnetting_ExportFileType => ExportFileType.Csv;

    // Application: Bit Calculator
    public static BitCaluclatorUnit BitCalculator_Unit => BitCaluclatorUnit.Bytes;
    public static BitCaluclatorNotation BitCalculator_Notation => BitCaluclatorNotation.Binary;
    public static ExportFileType BitCalculator_ExportFileType => ExportFileType.Csv;

    // Application: Lookup
    public static ExportFileType Lookup_OUI_ExportFileType => ExportFileType.Csv;
    public static ExportFileType Lookup_Port_ExportFileType => ExportFileType.Csv;

    // Application: Whois
    public static ExportFileType Whois_ExportFileType => ExportFileType.Txt;

    // Application: IP Geolocation
    public static ExportFileType IPGeolocation_ExportFileType => ExportFileType.Csv;

    // Application: Connections
    public static ExportFileType Connections_ExportFileType => ExportFileType.Csv;

    public static AutoRefreshTimeInfo Connections_AutoRefreshTime =>
        AutoRefreshTime.GetDefaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);

    // Application: Listeners
    public static ExportFileType Listeners_ExportFileType => ExportFileType.Csv;

    public static AutoRefreshTimeInfo Listeners_AutoRefreshTime =>
        AutoRefreshTime.GetDefaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);

    // Application: ARP Table
    public static ExportFileType ARPTable_ExportFileType => ExportFileType.Csv;

    public static AutoRefreshTimeInfo ARPTable_AutoRefreshTime =>
        AutoRefreshTime.GetDefaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);

    #endregion
}