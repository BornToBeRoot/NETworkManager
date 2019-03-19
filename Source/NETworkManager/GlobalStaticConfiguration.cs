using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Heijden.DNS;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Utilities;
using NETworkManager.Utilities.Enum;

// ReSharper disable InconsistentNaming

namespace NETworkManager
{
    public class GlobalStaticConfiguration
    {
        // Type to search (verage type speed --> 187 chars/min)
        public static TimeSpan SearchDispatcherTimerTimeSpan => new TimeSpan(0, 0, 0, 0, 750);
        public static TimeSpan CredentialsUILockTime => new TimeSpan(0, 0, 120);

        // Filter
        public static string ApplicationFileExtensionFilter => "Application (*.exe)|*.exe";

        // Settings
        public static ApplicationViewManager.Name General_DefaultApplicationViewName => ApplicationViewManager.Name.Dashboard;
        public static int General_BackgroundJobInterval => 15;
        public static int General_HistoryListEntries => 5;
        public static double Appearance_Opacity => 0.85;

        // Fixes
        public static double FloatPointFix => 1.0;

        // HotKey
        public static int HotKey_ShowWindowKey => 79;
        public static int HotKey_ShowWindowModifier => 3;

        // Profile
        public static double Profile_WidthCollapsed => 40;
        public static double Profile_DefaultWidthExpanded => 250;
        public static double Profile_MaxWidthExpanded => 350;

        // Application: Dashboard
        public static string Dashboard_PublicIPAddressAPI => "https://api.ipify.org";
        public static string Dashboard_PublicIPAddress => "1.1.1.1";

        // Application: IP Scanner
        public static int IPScanner_Threads => 256;
        public static int IPScanner_ICMPAttempts => 2;
        public static int IPScanner_ICMPBuffer => 32;
        public static int IPScanner_DNSPort => 53;
        public static TransportType IPScanner_DNSTransportType => TransportType.Udp;
        public static int IPScanner_DNSAttempts => 2;
        public static int IPScanner_DNSTimeout => 2000;
        public static int IPScanner_ICMPTimeout => 4000;
        public static ExportManager.ExportFileType IPScanner_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Port Scanner 
        public static int PortScanner_HostThreads => 5;
        public static int PortScanner_PortThreds => 100;
        public static int PortScanner_Timeout => 4000;
        public static ExportManager.ExportFileType PortScanner_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Ping
        public static int Ping_Buffer => 32;
        public static int Ping_Timeout => 4000;
        public static int Ping_TTL => 64;
        public static int Ping_WaitTime => 1000;
        public static int Ping_ExceptionCancelCount => 3;
        public static ExportManager.ExportFileType Ping_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Traceroute
        public static int Traceroute_MaximumHops => 30;
        public static int Traceroute_Timeout => 4000;
        public static int Traceroute_Buffer => 32;
        public static ExportManager.ExportFileType Traceroute_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: DNS Lookup
        public static QClass DNSLookup_Class => QClass.IN;
        public static QType DNSLookup_Type => QType.ANY;
        public static TransportType DNSLookup_TransportType => TransportType.Udp;
        public static int DNSLookup_Attempts => 3;
        public static int DNSLookup_Timeout => 2000;
        public static ExportManager.ExportFileType DNSLookup_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: RemoteDesktop
        public static List<string> RemoteDesktop_ScreenResolutions => new List<string>
        {
            "640x480",
            "800x600",
            "1024x768",
            "1280x720",
            "1280x768",
            "1280x800",
            "1280x1024",
            "1366x768",
            "1440x900",
            "1400x1050",
            "1680x1050",
            "1920x1080"
        };
        public static int RemoteDesktop_ScreenWidth => 1280;
        public static int RemoteDesktop_ScreenHeight => 768;
        public static List<int> RemoteDesktop_ColorDepths => new List<int>
        {
            15,
            16,
            24,
            32
        };
        public static int RemoteDesktop_ColorDepth = 32;
        public static int RemoteDesktop_Port => 3389;
        public static List<Tuple<int, string>> RemoteDesktop_KeyboardHookModes => new List<Tuple<int, string>>
        {
            Tuple.Create(0, Resources.Localization.Strings.OnThisComputer),
            Tuple.Create(1, Resources.Localization.Strings.OnTheRemoteComputer)/*,
            Tuple.Create(2, Resources.Localization.Strings.OnlyWhenUsingTheFullScreen),*/
        };
        public static uint RemoteDesktop_AuthenticationLevel => 2;
        public static int RemoteDesktop_KeyboardHookMode => 1;

        // Application: PowerShell
        public static string PowerShell_ApplicationFileLocationPowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"System32\WindowsPowerShell\v1.0\powershell.exe");
        public static PowerShell.ExecutionPolicy PowerShell_ExecutionPolicy => PowerShell.ExecutionPolicy.RemoteSigned;

        // Application: PuTTY
        public static PuTTY.ConnectionMode PuTTY_DefaultConnectionMode => PuTTY.ConnectionMode.SSH;
        public static int PuTTY_SSHPort => 22;
        public static string PuTTY_SerialLine => "COM1";
        public static int PuTTY_TelnetPort => 23;
        public static int PuTTY_RloginPort => 513;
        public static int PuTTY_BaudRate => 9600;
        public static int PuTTY_Raw => 0;

        // Application: TigerVNC
        public static int TigerVNC_DefaultVNCPort => 5900;

        // Application: SNMP
        public static WalkMode SNMP_WalkMode => WalkMode.WithinSubtree;
        public static int SNMP_Timeout => 60000;
        public static SNMP.SNMPMode SNMP_Mode => SNMP.SNMPMode.Walk;
        public static SNMP.SNMPVersion SNMP_Version => SNMP.SNMPVersion.V2C;
        public static SNMP.SNMPV3Security SNMP_Security => SNMP.SNMPV3Security.AuthPriv;
        public static SNMP.SNMPV3AuthenticationProvider SNMP_AuthenticationProvider => SNMP.SNMPV3AuthenticationProvider.SHA1;
        public static SNMP.SNMPV3PrivacyProvider SNMP_PrivacyProvider => SNMP.SNMPV3PrivacyProvider.AES;
        public static ExportManager.ExportFileType SNMP_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Wake on LAN
        public static int WakeOnLAN_Port => 7;

        // Application: HTTP Header
        public static int HTTPHeaders_Timeout => 10000;
        public static ExportManager.ExportFileType HTTPHeaders_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Subnet Calculator
        public static ExportManager.ExportFileType SubnetCalculator_Subnetting_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Lookup
        public static ExportManager.ExportFileType Lookup_OUI_ExportFileType => ExportManager.ExportFileType.CSV;
        public static ExportManager.ExportFileType Lookup_Port_ExportFileType => ExportManager.ExportFileType.CSV;

        // Application: Whois
        public static ExportManager.ExportFileType Whois_ExportFileType => ExportManager.ExportFileType.TXT;

        // Application: Connections
        public static ExportManager.ExportFileType Connections_ExportFileType => ExportManager.ExportFileType.CSV;
        public static AutoRefreshTimeInfo Connections_AutoRefreshTime => AutoRefreshTime.Defaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);

        // Application: Listeners
        public static ExportManager.ExportFileType Listeners_ExportFileType => ExportManager.ExportFileType.CSV;
        public static AutoRefreshTimeInfo Listeners_AutoRefreshTime => AutoRefreshTime.Defaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);

        // Application: ARP Table
        public static ExportManager.ExportFileType ARPTable_ExportFileType => ExportManager.ExportFileType.CSV;
        public static AutoRefreshTimeInfo ARPTable_AutoRefreshTime => AutoRefreshTime.Defaults.First(x => x.Value == 30 && x.TimeUnit == TimeUnit.Second);
    }
}