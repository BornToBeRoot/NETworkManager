using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using DnsClient;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Controls;
using NETworkManager.Models;
using NETworkManager.Models.AWS;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Models.PuTTY;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;
using static NETworkManager.Models.Network.SNMP;

namespace NETworkManager.Settings
{
    public class SettingsInfo : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        [XmlIgnore] public bool SettingsChanged { get; set; }

        private bool _firstRun = true;
        public bool FirstRun
        {
            get => _firstRun;
            set
            {
                if (value == _firstRun)
                    return;

                _firstRun = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _version { get; set; }

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version)
                    return;

                _version = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        #region General 
        // General        
        private ApplicationName _general_DefaultApplicationViewName = GlobalStaticConfiguration.General_DefaultApplicationViewName;
        public ApplicationName General_DefaultApplicationViewName
        {
            get => _general_DefaultApplicationViewName;
            set
            {
                if (value == _general_DefaultApplicationViewName)
                    return;

                _general_DefaultApplicationViewName = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _general_BackgroundJobInterval = GlobalStaticConfiguration.General_BackgroundJobInterval;
        public int General_BackgroundJobInterval
        {
            get => _general_BackgroundJobInterval;
            set
            {
                if (value == _general_BackgroundJobInterval)
                    return;

                _general_BackgroundJobInterval = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _general_HistoryListEntries = GlobalStaticConfiguration.General_HistoryListEntries;
        public int General_HistoryListEntries
        {
            get => _general_HistoryListEntries;
            set
            {
                if (value == _general_HistoryListEntries)
                    return;

                _general_HistoryListEntries = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableSetCollection<ApplicationInfo> _general_ApplicationList = new();
        public ObservableSetCollection<ApplicationInfo> General_ApplicationList
        {
            get => _general_ApplicationList;
            set
            {
                if (value == _general_ApplicationList)
                    return;

                _general_ApplicationList = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Window
        private bool _window_ConfirmClose;
        public bool Window_ConfirmClose
        {
            get => _window_ConfirmClose;
            set
            {
                if (value == _window_ConfirmClose)
                    return;

                _window_ConfirmClose = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _window_MinimizeInsteadOfTerminating;
        public bool Window_MinimizeInsteadOfTerminating
        {
            get => _window_MinimizeInsteadOfTerminating;
            set
            {
                if (value == _window_MinimizeInsteadOfTerminating)
                    return;

                _window_MinimizeInsteadOfTerminating = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _window_MultipleInstances;
        public bool Window_MultipleInstances
        {
            get => _window_MultipleInstances;
            set
            {
                if (value == _window_MultipleInstances)
                    return;

                _window_MultipleInstances = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _window_MinimizeToTrayInsteadOfTaskbar;
        public bool Window_MinimizeToTrayInsteadOfTaskbar
        {
            get => _window_MinimizeToTrayInsteadOfTaskbar;
            set
            {
                if (value == _window_MinimizeToTrayInsteadOfTaskbar)
                    return;

                _window_MinimizeToTrayInsteadOfTaskbar = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // TrayIcon
        private bool _trayIcon_AlwaysShowIcon;
        public bool TrayIcon_AlwaysShowIcon
        {
            get => _trayIcon_AlwaysShowIcon;
            set
            {
                if (value == _trayIcon_AlwaysShowIcon)
                    return;

                _trayIcon_AlwaysShowIcon = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // SplashScreen
        private bool _splashScreen_Enabled = GlobalStaticConfiguration.SplashScreen_Enabled;
        public bool SplashScreen_Enabled
        {
            get => _splashScreen_Enabled;
            set
            {
                if (value == _splashScreen_Enabled)
                    return;

                _splashScreen_Enabled = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Appearance
        private string _appearance_Theme = GlobalStaticConfiguration.Appearance_Theme;
        public string Appearance_Theme
        {
            get => _appearance_Theme;
            set
            {
                if (value == _appearance_Theme)
                    return;

                _appearance_Theme = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _appearance_Accent = GlobalStaticConfiguration.Appearance_Accent;
        public string Appearance_Accent
        {
            get => _appearance_Accent;
            set
            {
                if (value == _appearance_Accent)
                    return;

                _appearance_Accent = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _appearance_UseCustomTheme = GlobalStaticConfiguration.Appearance_UseCustomTheme;
        public bool Appearance_UseCustomTheme
        {
            get => _appearance_UseCustomTheme;
            set
            {
                if (value == _appearance_UseCustomTheme)
                    return;

                _appearance_UseCustomTheme = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _appearance_CustomThemeName;
        public string Appearance_CustomThemeName
        {
            get => _appearance_CustomThemeName;
            set
            {
                if (value == _appearance_CustomThemeName)
                    return;

                _appearance_CustomThemeName = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _appearance_PowerShellModifyGlobalProfile;
        public bool Appearance_PowerShellModifyGlobalProfile
        {
            get => _appearance_PowerShellModifyGlobalProfile;
            set
            {
                if (value == _appearance_PowerShellModifyGlobalProfile)
                    return;

                _appearance_PowerShellModifyGlobalProfile = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Localization
        private string _localization_CultureCode;
        public string Localization_CultureCode
        {
            get => _localization_CultureCode;
            set
            {
                if (value == _localization_CultureCode)
                    return;

                _localization_CultureCode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Status
        private bool _status_ShowWindowOnNetworkChange = GlobalStaticConfiguration.Status_ShowWindowOnNetworkChange;
        public bool Status_ShowWindowOnNetworkChange
        {
            get => _status_ShowWindowOnNetworkChange;
            set
            {
                if (value == _status_ShowWindowOnNetworkChange)
                    return;

                _status_ShowWindowOnNetworkChange = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _status_WindowCloseTime = GlobalStaticConfiguration.Status_WindowCloseTime;
        public int Status_WindowCloseTime
        {
            get => _status_WindowCloseTime;
            set
            {
                if (value == _status_WindowCloseTime)
                    return;

                _status_WindowCloseTime = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _status_IPAddressToDetectLocalIPAddressBasedOnRouting = GlobalStaticConfiguration.Status_IPAddressToDetectLocalIPAddressBasedOnRouting;
        public string Status_IPAddressToDetectLocalIPAddressBasedOnRouting
        {
            get => _status_IPAddressToDetectLocalIPAddressBasedOnRouting;
            set
            {
                if (value == _status_IPAddressToDetectLocalIPAddressBasedOnRouting)
                    return;

                _status_IPAddressToDetectLocalIPAddressBasedOnRouting = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Autostart
        private bool _autostart_StartMinimizedInTray;
        public bool Autostart_StartMinimizedInTray
        {
            get => _autostart_StartMinimizedInTray;
            set
            {
                if (value == _autostart_StartMinimizedInTray)
                    return;

                _autostart_StartMinimizedInTray = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // HotKey
        private bool _hotKey_ShowWindowEnabled;
        public bool HotKey_ShowWindowEnabled
        {
            get => _hotKey_ShowWindowEnabled;
            set
            {
                if (value == _hotKey_ShowWindowEnabled)
                    return;

                _hotKey_ShowWindowEnabled = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _hotKey_ShowWindowKey = GlobalStaticConfiguration.HotKey_ShowWindowKey;
        public int HotKey_ShowWindowKey
        {
            get => _hotKey_ShowWindowKey;
            set
            {
                if (value == _hotKey_ShowWindowKey)
                    return;

                _hotKey_ShowWindowKey = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _hotKey_ShowWindowModifier = GlobalStaticConfiguration.HotKey_ShowWindowModifier;
        public int HotKey_ShowWindowModifier
        {
            get => _hotKey_ShowWindowModifier;
            set
            {
                if (value == _hotKey_ShowWindowModifier)
                    return;

                _hotKey_ShowWindowModifier = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Update
        private bool _update_CheckForUpdatesAtStartup = true;
        public bool Update_CheckForUpdatesAtStartup
        {
            get => _update_CheckForUpdatesAtStartup;
            set
            {
                if (value == _update_CheckForUpdatesAtStartup)
                    return;

                _update_CheckForUpdatesAtStartup = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _update_CheckForPreReleases;
        public bool Update_CheckForPreReleases
        {
            get => _update_CheckForPreReleases;
            set
            {
                if (value == _update_CheckForPreReleases)
                    return;

                _update_CheckForPreReleases = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        // Profiles
        private string _profiles_CustomProfilesLocation;
        public string Profiles_CustomProfilesLocation
        {
            get => _profiles_CustomProfilesLocation;
            set
            {
                if (value == _profiles_CustomProfilesLocation)
                    return;

                _profiles_CustomProfilesLocation = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _profiles_LastSelected;
        public string Profiles_LastSelected
        {
            get => _profiles_LastSelected;
            set
            {
                if (value == _profiles_LastSelected)
                    return;

                _profiles_LastSelected = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Others
        // Application view       
        private bool _expandApplicationView;
        public bool ExpandApplicationView
        {
            get => _expandApplicationView;
            set
            {
                if (value == _expandApplicationView)
                    return;

                _expandApplicationView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Dashboard
        private string _dashboard_PublicIPv4Address = GlobalStaticConfiguration.Dashboard_PublicIPv4Address;
        public string Dashboard_PublicIPv4Address
        {
            get => _dashboard_PublicIPv4Address;
            set
            {
                if (value == _dashboard_PublicIPv4Address)
                    return;

                _dashboard_PublicIPv4Address = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _dashboard_PublicIPv6Address = GlobalStaticConfiguration.Dashboard_PublicIPv6Address;
        public string Dashboard_PublicIPv6Address
        {
            get => _dashboard_PublicIPv6Address;
            set
            {
                if (value == _dashboard_PublicIPv6Address)
                    return;

                _dashboard_PublicIPv6Address = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dashboard_CheckPublicIPAddress = GlobalStaticConfiguration.Dashboard_CheckPublicIPAddress;
        public bool Dashboard_CheckPublicIPAddress
        {
            get => _dashboard_CheckPublicIPAddress;
            set
            {
                if (value == _dashboard_CheckPublicIPAddress)
                    return;

                _dashboard_CheckPublicIPAddress = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dashboard_UseCustomPublicIPv4AddressAPI;
        public bool Dashboard_UseCustomPublicIPv4AddressAPI
        {
            get => _dashboard_UseCustomPublicIPv4AddressAPI;
            set
            {
                if (value == _dashboard_UseCustomPublicIPv4AddressAPI)
                    return;

                _dashboard_UseCustomPublicIPv4AddressAPI = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _dashboard_CustomPublicIPv4AddressAPI;
        public string Dashboard_CustomPublicIPv4AddressAPI
        {
            get => _dashboard_CustomPublicIPv4AddressAPI;
            set
            {
                if (value == _dashboard_CustomPublicIPv4AddressAPI)
                    return;

                _dashboard_CustomPublicIPv4AddressAPI = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dashboard_UseCustomPublicIPv6AddressAPI;
        public bool Dashboard_UseCustomPublicIPv6AddressAPI
        {
            get => _dashboard_UseCustomPublicIPv6AddressAPI;
            set
            {
                if (value == _dashboard_UseCustomPublicIPv6AddressAPI)
                    return;

                _dashboard_UseCustomPublicIPv6AddressAPI = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _dashboard_CustomPublicIPv6AddressAPI;
        public string Dashboard_CustomPublicIPv6AddressAPI
        {
            get => _dashboard_CustomPublicIPv6AddressAPI;
            set
            {
                if (value == _dashboard_CustomPublicIPv6AddressAPI)
                    return;

                _dashboard_CustomPublicIPv6AddressAPI = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Network Interface       
        private string _networkInterface_InterfaceId;
        public string NetworkInterface_InterfaceId
        {
            get => _networkInterface_InterfaceId;
            set
            {
                if (value == _networkInterface_InterfaceId)
                    return;

                _networkInterface_InterfaceId = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _networkInterface_ExpandProfileView = true;
        public bool NetworkInterface_ExpandProfileView
        {
            get => _networkInterface_ExpandProfileView;
            set
            {
                if (value == _networkInterface_ExpandProfileView)
                    return;

                _networkInterface_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _networkInterface_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double NetworkInterface_ProfileWidth
        {
            get => _networkInterface_ProfileWidth;
            set
            {
                if (Math.Abs(value - _networkInterface_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _networkInterface_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region WiFi
        private string _wiFi_InterfaceId;
        public string WiFi_InterfaceId
        {
            get => _wiFi_InterfaceId;
            set
            {
                if (value == _wiFi_InterfaceId)
                    return;

                _wiFi_InterfaceId = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _wiFi_Show2dot4GHzNetworks = GlobalStaticConfiguration.WiFi_Show2dot4GHzNetworks;
        public bool WiFi_Show2dot4GHzNetworks
        {
            get => _wiFi_Show2dot4GHzNetworks;
            set
            {
                if (value == _wiFi_Show2dot4GHzNetworks)
                    return;

                _wiFi_Show2dot4GHzNetworks = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _wiFi_Show5GHzNetworks = GlobalStaticConfiguration.WiFi_Show5GHzNetworks;
        public bool WiFi_Show5GHzNetworks
        {
            get => _wiFi_Show5GHzNetworks;
            set
            {
                if (value == _wiFi_Show5GHzNetworks)
                    return;

                _wiFi_Show5GHzNetworks = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _wiFi_AutoRefresh;
        public bool WiFi_AutoRefresh
        {
            get => _wiFi_AutoRefresh;
            set
            {
                if (value == _wiFi_AutoRefresh)
                    return;

                _wiFi_AutoRefresh = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private AutoRefreshTimeInfo _wiFi_AutoRefreshTime = GlobalStaticConfiguration.WiFi_AutoRefreshTime;
        public AutoRefreshTimeInfo WiFi_AutoRefreshTime
        {
            get => _wiFi_AutoRefreshTime;
            set
            {
                if (value == _wiFi_AutoRefreshTime)
                    return;

                _wiFi_AutoRefreshTime = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _wiFi_ExportFilePath;
        public string WiFi_ExportFilePath
        {
            get => _wiFi_ExportFilePath;
            set
            {
                if (value == _wiFi_ExportFilePath)
                    return;

                _wiFi_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _wiFi_ExportFileType = GlobalStaticConfiguration.WiFi_ExportFileType;
        public ExportManager.ExportFileType WiFi_ExportFileType
        {
            get => _wiFi_ExportFileType;
            set
            {
                if (value == _wiFi_ExportFileType)
                    return;

                _wiFi_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region IPScanner        
        private bool _ipScanner_ShowScanResultForAllIPAddresses;
        public bool IPScanner_ShowScanResultForAllIPAddresses
        {
            get => _ipScanner_ShowScanResultForAllIPAddresses;
            set
            {
                if (value == _ipScanner_ShowScanResultForAllIPAddresses)
                    return;

                _ipScanner_ShowScanResultForAllIPAddresses = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_Threads = GlobalStaticConfiguration.IPScanner_Threads;
        public int IPScanner_Threads
        {
            get => _ipScanner_Threads;
            set
            {
                if (value == _ipScanner_Threads)
                    return;

                _ipScanner_Threads = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_ICMPAttempts = GlobalStaticConfiguration.IPScanner_ICMPAttempts;
        public int IPScanner_ICMPAttempts
        {
            get => _ipScanner_ICMPAttempts;
            set
            {
                if (value == _ipScanner_ICMPAttempts)
                    return;

                _ipScanner_ICMPAttempts = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_ICMPBuffer = GlobalStaticConfiguration.IPScanner_ICMPBuffer;
        public int IPScanner_ICMPBuffer
        {
            get => _ipScanner_ICMPBuffer;
            set
            {
                if (value == _ipScanner_ICMPBuffer)
                    return;

                _ipScanner_ICMPBuffer = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _ipScanner_HostsHistory = new();
        public ObservableCollection<string> IPScanner_HostsHistory
        {
            get => _ipScanner_HostsHistory;
            set
            {
                if (value == _ipScanner_HostsHistory)
                    return;

                _ipScanner_HostsHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ResolveHostname = true;
        public bool IPScanner_ResolveHostname
        {
            get => _ipScanner_ResolveHostname;
            set
            {
                if (value == _ipScanner_ResolveHostname)
                    return;

                _ipScanner_ResolveHostname = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_UseCustomDNSServer;
        public bool IPScanner_UseCustomDNSServer
        {
            get => _ipScanner_UseCustomDNSServer;
            set
            {
                if (value == _ipScanner_UseCustomDNSServer)
                    return;

                _ipScanner_UseCustomDNSServer = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _ipScanner_CustomDNSServer;
        public string IPScanner_CustomDNSServer
        {
            get => _ipScanner_CustomDNSServer;
            set
            {
                if (value == _ipScanner_CustomDNSServer)
                    return;

                _ipScanner_CustomDNSServer = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_CustomDNSPort = GlobalStaticConfiguration.IPScanner_DNSPort;
        public int IPScanner_CustomDNSPort
        {
            get => _ipScanner_CustomDNSPort;
            set
            {
                if (value == _ipScanner_CustomDNSPort)
                    return;

                _ipScanner_CustomDNSPort = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_DNSRecursion = true;
        public bool IPScanner_DNSRecursion
        {
            get => _ipScanner_DNSRecursion;
            set
            {
                if (value == _ipScanner_DNSRecursion)
                    return;

                _ipScanner_DNSRecursion = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_DNSUseCache;
        public bool IPScanner_DNSUseCache
        {
            get => _ipScanner_DNSUseCache;
            set
            {
                if (value == _ipScanner_DNSUseCache)
                    return;

                _ipScanner_DNSUseCache = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_DNSUseTCPOnly = GlobalStaticConfiguration.IPScanner_DNSUseTCPOnly;
        public bool IPScanner_DNSUseTCPOnly
        {
            get => _ipScanner_DNSUseTCPOnly;
            set
            {
                if (value == _ipScanner_DNSUseTCPOnly)
                    return;

                _ipScanner_DNSUseTCPOnly = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_DNSTimeout = GlobalStaticConfiguration.IPScanner_DNSTimeout;
        public int IPScanner_DNSTimeout
        {
            get => _ipScanner_DNSTimeout;
            set
            {
                if (value == _ipScanner_DNSTimeout)
                    return;

                _ipScanner_DNSTimeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_DNSRetries = GlobalStaticConfiguration.IPScanner_DNSRetries;
        public int IPScanner_DNSRetries
        {
            get => _ipScanner_DNSRetries;
            set
            {
                if (value == _ipScanner_DNSRetries)
                    return;

                _ipScanner_DNSRetries = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_DNSShowErrorMessage = GlobalStaticConfiguration.IPScanner_DNSShowErrorMessage;
        public bool IPScanner_DNSShowErrorMessage
        {
            get => _ipScanner_DNSShowErrorMessage;
            set
            {
                if (value == _ipScanner_DNSShowErrorMessage)
                    return;

                _ipScanner_DNSShowErrorMessage = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ResolveMACAddress;
        public bool IPScanner_ResolveMACAddress
        {
            get => _ipScanner_ResolveMACAddress;
            set
            {
                if (value == _ipScanner_ResolveMACAddress)
                    return;

                _ipScanner_ResolveMACAddress = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _ipScanner_ICMPTimeout = GlobalStaticConfiguration.IPScanner_ICMPTimeout;
        public int IPScanner_ICMPTimeout
        {
            get => _ipScanner_ICMPTimeout;
            set
            {
                if (value == _ipScanner_ICMPTimeout)
                    return;

                _ipScanner_ICMPTimeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<CustomCommandInfo> _ipScanner_CustomCommands = new();
        public ObservableCollection<CustomCommandInfo> IPScanner_CustomCommands
        {
            get => _ipScanner_CustomCommands;
            set
            {
                if (value == _ipScanner_CustomCommands)
                    return;

                _ipScanner_CustomCommands = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ExpandProfileView = true;
        public bool IPScanner_ExpandProfileView
        {
            get => _ipScanner_ExpandProfileView;
            set
            {
                if (value == _ipScanner_ExpandProfileView)
                    return;

                _ipScanner_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _ipScanner_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double IPScanner_ProfileWidth
        {
            get => _ipScanner_ProfileWidth;
            set
            {
                if (Math.Abs(value - _ipScanner_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _ipScanner_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _ipScanner_ExportFilePath;
        public string IPScanner_ExportFilePath
        {
            get => _ipScanner_ExportFilePath;
            set
            {
                if (value == _ipScanner_ExportFilePath)
                    return;

                _ipScanner_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _ipScanner_ExportFileType = GlobalStaticConfiguration.IPScanner_ExportFileType;
        public ExportManager.ExportFileType IPScanner_ExportFileType
        {
            get => _ipScanner_ExportFileType;
            set
            {
                if (value == _ipScanner_ExportFileType)
                    return;

                _ipScanner_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Port Scanner
        private ObservableCollection<string> _portScanner_HostsHistory = new();
        public ObservableCollection<string> PortScanner_HostsHistory
        {
            get => _portScanner_HostsHistory;
            set
            {
                if (value == _portScanner_HostsHistory)
                    return;

                _portScanner_HostsHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _portScanner_PortsHistory = new();
        public ObservableCollection<string> PortScanner_PortsHistory
        {
            get => _portScanner_PortsHistory;
            set
            {
                if (value == _portScanner_PortsHistory)
                    return;

                _portScanner_PortsHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<PortProfileInfo> _portScanner_PortProfiles = new();
        public ObservableCollection<PortProfileInfo> PortScanner_PortProfiles
        {
            get => _portScanner_PortProfiles;
            set
            {
                if (value == _portScanner_PortProfiles)
                    return;

                _portScanner_PortProfiles = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ResolveHostname = true;
        public bool PortScanner_ResolveHostname
        {
            get => _portScanner_ResolveHostname;
            set
            {
                if (value == _portScanner_ResolveHostname)
                    return;

                _portScanner_ResolveHostname = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _portScanner_HostThreads = GlobalStaticConfiguration.PortScanner_HostThreads;
        public int PortScanner_HostThreads
        {
            get => _portScanner_HostThreads;
            set
            {
                if (value == _portScanner_HostThreads)
                    return;

                _portScanner_HostThreads = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _portScanner_PortThreads = GlobalStaticConfiguration.PortScanner_PortThreds;
        public int PortScanner_PortThreads
        {
            get => _portScanner_PortThreads;
            set
            {
                if (value == _portScanner_PortThreads)
                    return;

                _portScanner_PortThreads = value;
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ShowClosed;
        public bool PortScanner_ShowClosed
        {
            get => _portScanner_ShowClosed;
            set
            {
                if (value == _portScanner_ShowClosed)
                    return;

                _portScanner_ShowClosed = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _portScanner_Timeout = GlobalStaticConfiguration.PortScanner_Timeout;
        public int PortScanner_Timeout
        {
            get => _portScanner_Timeout;
            set
            {
                if (value == _portScanner_Timeout)
                    return;

                _portScanner_Timeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ExpandProfileView = true;
        public bool PortScanner_ExpandProfileView
        {
            get => _portScanner_ExpandProfileView;
            set
            {
                if (value == _portScanner_ExpandProfileView)
                    return;

                _portScanner_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _portScanner_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double PortScanner_ProfileWidth
        {
            get => _portScanner_ProfileWidth;
            set
            {
                if (Math.Abs(value - _portScanner_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _portScanner_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _portScanner_ExportFilePath;
        public string PortScanner_ExportFilePath
        {
            get => _portScanner_ExportFilePath;
            set
            {
                if (value == _portScanner_ExportFilePath)
                    return;

                _portScanner_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _portScanner_ExportFileType = GlobalStaticConfiguration.PortScanner_ExportFileType;
        public ExportManager.ExportFileType PortScanner_ExportFileType
        {
            get => _portScanner_ExportFileType;
            set
            {
                if (value == _portScanner_ExportFileType)
                    return;

                _portScanner_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Ping Monitor
        private ObservableCollection<string> _pingMonitor_HostHistory = new();
        public ObservableCollection<string> PingMonitor_HostHistory
        {
            get => _pingMonitor_HostHistory;
            set
            {
                if (value == _pingMonitor_HostHistory)
                    return;

                _pingMonitor_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _pingMonitor_Buffer = GlobalStaticConfiguration.PingMonitor_Buffer;
        public int PingMonitor_Buffer
        {
            get => _pingMonitor_Buffer;
            set
            {
                if (value == _pingMonitor_Buffer)
                    return;

                _pingMonitor_Buffer = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _pingMonitor_DontFragement = true;
        public bool PingMonitor_DontFragment
        {
            get => _pingMonitor_DontFragement;
            set
            {
                if (value == _pingMonitor_DontFragement)
                    return;

                _pingMonitor_DontFragement = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _pingMonitor_Timeout = GlobalStaticConfiguration.PingMonitor_Timeout;
        public int PingMonitor_Timeout
        {
            get => _pingMonitor_Timeout;
            set
            {
                if (value == _pingMonitor_Timeout)
                    return;

                _pingMonitor_Timeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _pingMonitor_TTL = GlobalStaticConfiguration.PingMonitor_TTL;
        public int PingMonitor_TTL
        {
            get => _pingMonitor_TTL;
            set
            {
                if (value == _pingMonitor_TTL)
                    return;

                _pingMonitor_TTL = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _pingMonitor_WaitTime = GlobalStaticConfiguration.PingMonitor_WaitTime;
        public int PingMonitor_WaitTime
        {
            get => _pingMonitor_WaitTime;
            set
            {
                if (value == _pingMonitor_WaitTime)
                    return;

                _pingMonitor_WaitTime = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _pingMonitor_ResolveHostnamePreferIPv4 = true;
        public bool PingMonitor_ResolveHostnamePreferIPv4
        {
            get => _pingMonitor_ResolveHostnamePreferIPv4;
            set
            {
                if (value == _pingMonitor_ResolveHostnamePreferIPv4)
                    return;

                _pingMonitor_ResolveHostnamePreferIPv4 = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _pingMonitor_ExportFilePath;
        public string PingMonitor_ExportFilePath
        {
            get => _pingMonitor_ExportFilePath;
            set
            {
                if (value == _pingMonitor_ExportFilePath)
                    return;

                _pingMonitor_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _pingMonitor_ExportFileType = GlobalStaticConfiguration.PingMonitor_ExportFileType;
        public ExportManager.ExportFileType PingMonitor_ExportFileType
        {
            get => _pingMonitor_ExportFileType;
            set
            {
                if (value == _pingMonitor_ExportFileType)
                    return;

                _pingMonitor_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _pingMonitor_ExpandProfileView = true;
        public bool PingMonitor_ExpandProfileView
        {
            get => _pingMonitor_ExpandProfileView;
            set
            {
                if (value == _pingMonitor_ExpandProfileView)
                    return;

                _pingMonitor_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _pingMonitor_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double PingMonitor_ProfileWidth
        {
            get => _pingMonitor_ProfileWidth;
            set
            {
                if (Math.Abs(value - _pingMonitor_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _pingMonitor_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Traceroute
        private ObservableCollection<string> _traceroute_HostHistory = new();
        public ObservableCollection<string> Traceroute_HostHistory
        {
            get => _traceroute_HostHistory;
            set
            {
                if (value == _traceroute_HostHistory)
                    return;

                _traceroute_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _traceroute_MaximumHops = GlobalStaticConfiguration.Traceroute_MaximumHops;
        public int Traceroute_MaximumHops
        {
            get => _traceroute_MaximumHops;
            set
            {
                if (value == _traceroute_MaximumHops)
                    return;

                _traceroute_MaximumHops = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _traceroute_Timeout = GlobalStaticConfiguration.Traceroute_Timeout;
        public int Traceroute_Timeout
        {
            get => _traceroute_Timeout;
            set
            {
                if (value == _traceroute_Timeout)
                    return;

                _traceroute_Timeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _traceroute_Buffer = GlobalStaticConfiguration.Traceroute_Buffer;
        public int Traceroute_Buffer
        {
            get => _traceroute_Buffer;
            set
            {
                if (value == _traceroute_Buffer)
                    return;

                _traceroute_Buffer = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _traceroute_ResolveHostname = true;
        public bool Traceroute_ResolveHostname
        {
            get => _traceroute_ResolveHostname;
            set
            {
                if (value == _traceroute_ResolveHostname)
                    return;

                _traceroute_ResolveHostname = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _traceroute_ResolveHostnamePreferIPv4 = true;
        public bool Traceroute_ResolveHostnamePreferIPv4
        {
            get => _traceroute_ResolveHostnamePreferIPv4;
            set
            {
                if (value == _traceroute_ResolveHostnamePreferIPv4)
                    return;

                _traceroute_ResolveHostnamePreferIPv4 = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _traceroute_ExpandProfileView = true;
        public bool Traceroute_ExpandProfileView
        {
            get => _traceroute_ExpandProfileView;
            set
            {
                if (value == _traceroute_ExpandProfileView)
                    return;

                _traceroute_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _traceroute_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double Traceroute_ProfileWidth
        {
            get => _traceroute_ProfileWidth;
            set
            {
                if (Math.Abs(value - _traceroute_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _traceroute_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _traceroute_ExportFilePath;
        public string Traceroute_ExportFilePath
        {
            get => _traceroute_ExportFilePath;
            set
            {
                if (value == _traceroute_ExportFilePath)
                    return;

                _traceroute_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _traceroute_ExportFileType = GlobalStaticConfiguration.Traceroute_ExportFileType;
        public ExportManager.ExportFileType Traceroute_ExportFileType
        {
            get => _traceroute_ExportFileType;
            set
            {
                if (value == _traceroute_ExportFileType)
                    return;

                _traceroute_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region DNS Lookup
        private ObservableCollection<string> _dnsLookup_HostHistory = new();
        public ObservableCollection<string> DNSLookup_HostHistory
        {
            get => _dnsLookup_HostHistory;
            set
            {
                if (value == _dnsLookup_HostHistory)
                    return;

                _dnsLookup_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<DNSServerInfo> _dnsLookup_DNSServers = new();
        public ObservableCollection<DNSServerInfo> DNSLookup_DNSServers
        {
            get => _dnsLookup_DNSServers;
            set
            {
                if (value == _dnsLookup_DNSServers)
                    return;

                _dnsLookup_DNSServers = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private DNSServerInfo _dnsLookup_SelectedDNSServer = new();
        public DNSServerInfo DNSLookup_SelectedDNSServer
        {
            get => _dnsLookup_SelectedDNSServer;
            set
            {
                if (value == _dnsLookup_SelectedDNSServer)
                    return;

                _dnsLookup_SelectedDNSServer = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private QueryClass _dnsLookup_QueryClass = GlobalStaticConfiguration.DNSLookup_QueryClass;
        public QueryClass DNSLookup_QueryClass
        {
            get => _dnsLookup_QueryClass;
            set
            {
                if (value == _dnsLookup_QueryClass)
                    return;

                _dnsLookup_QueryClass = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_ShowOnlyMostCommonQueryTypes = true;
        public bool DNSLookup_ShowOnlyMostCommonQueryTypes
        {
            get => _dnsLookup_ShowOnlyMostCommonQueryTypes;
            set
            {
                if (value == _dnsLookup_ShowOnlyMostCommonQueryTypes)
                    return;

                _dnsLookup_ShowOnlyMostCommonQueryTypes = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private QueryType _dnsLookup_QueryType = GlobalStaticConfiguration.DNSLookup_QueryType;
        public QueryType DNSLookup_QueryType
        {
            get => _dnsLookup_QueryType;
            set
            {
                if (value == _dnsLookup_QueryType)
                    return;

                _dnsLookup_QueryType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_AddDNSSuffix = true;
        public bool DNSLookup_AddDNSSuffix
        {
            get => _dnsLookup_AddDNSSuffix;
            set
            {
                if (value == _dnsLookup_AddDNSSuffix)
                    return;

                _dnsLookup_AddDNSSuffix = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_UseCustomDNSSuffix;
        public bool DNSLookup_UseCustomDNSSuffix
        {
            get => _dnsLookup_UseCustomDNSSuffix;
            set
            {
                if (value == _dnsLookup_UseCustomDNSSuffix)
                    return;

                _dnsLookup_UseCustomDNSSuffix = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _dnsLookup_CustomDNSSuffix = string.Empty;
        public string DNSLookup_CustomDNSSuffix
        {
            get => _dnsLookup_CustomDNSSuffix;
            set
            {
                if (value == _dnsLookup_CustomDNSSuffix)
                    return;

                _dnsLookup_CustomDNSSuffix = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_Recursion = true;
        public bool DNSLookup_Recursion
        {
            get => _dnsLookup_Recursion;
            set
            {
                if (value == _dnsLookup_Recursion)
                    return;

                _dnsLookup_Recursion = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_UseCache;
        public bool DNSLookup_UseCache
        {
            get => _dnsLookup_UseCache;
            set
            {
                if (value == _dnsLookup_UseCache)
                    return;

                _dnsLookup_UseCache = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_UseTCPOnly = GlobalStaticConfiguration.DNSLookup_UseTCPOnly;
        public bool DNSLookup_UseTCPOnly
        {
            get => _dnsLookup_UseTCPOnly;
            set
            {
                if (value == _dnsLookup_UseTCPOnly)
                    return;

                _dnsLookup_UseTCPOnly = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _dnsLookup_Retries = GlobalStaticConfiguration.DNSLookup_Retries;
        public int DNSLookup_Retries
        {
            get => _dnsLookup_Retries;
            set
            {
                if (value == _dnsLookup_Retries)
                    return;

                _dnsLookup_Retries = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _dnsLookup_Timeout = GlobalStaticConfiguration.DNSLookup_Timeout;
        public int DNSLookup_Timeout
        {
            get => _dnsLookup_Timeout;
            set
            {
                if (value == _dnsLookup_Timeout)
                    return;

                _dnsLookup_Timeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_ExpandProfileView = true;
        public bool DNSLookup_ExpandProfileView
        {
            get => _dnsLookup_ExpandProfileView;
            set
            {
                if (value == _dnsLookup_ExpandProfileView)
                    return;

                _dnsLookup_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _dnsLookup_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double DNSLookup_ProfileWidth
        {
            get => _dnsLookup_ProfileWidth;
            set
            {
                if (Math.Abs(value - _dnsLookup_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _dnsLookup_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _dnsLookup_ExportFilePath;
        public string DNSLookup_ExportFilePath
        {
            get => _dnsLookup_ExportFilePath;
            set
            {
                if (value == _dnsLookup_ExportFilePath)
                    return;

                _dnsLookup_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _dnsLookup_ExportFileType = GlobalStaticConfiguration.DNSLookup_ExportFileType;
        public ExportManager.ExportFileType DNSLookup_ExportFileType
        {
            get => _dnsLookup_ExportFileType;
            set
            {
                if (value == _dnsLookup_ExportFileType)
                    return;

                _dnsLookup_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Remote Desktop 
        private ObservableCollection<string> _remoteDesktop_HostHistory = new();
        public ObservableCollection<string> RemoteDesktop_HostHistory
        {
            get => _remoteDesktop_HostHistory;
            set
            {
                if (value == _remoteDesktop_HostHistory)
                    return;

                _remoteDesktop_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_AdjustScreenAutomatically;
        public bool RemoteDesktop_AdjustScreenAutomatically
        {
            get => _remoteDesktop_AdjustScreenAutomatically;
            set
            {
                if (value == _remoteDesktop_AdjustScreenAutomatically)
                    return;

                _remoteDesktop_AdjustScreenAutomatically = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_UseCurrentViewSize = GlobalStaticConfiguration.RemoteDesktop_UseCurrentViewSize;
        public bool RemoteDesktop_UseCurrentViewSize
        {
            get => _remoteDesktop_UseCurrentViewSize;
            set
            {
                if (value == _remoteDesktop_UseCurrentViewSize)
                    return;

                _remoteDesktop_UseCurrentViewSize = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_UseFixedScreenSize;
        public bool RemoteDesktop_UseFixedScreenSize
        {
            get => _remoteDesktop_UseFixedScreenSize;
            set
            {
                if (value == _remoteDesktop_UseFixedScreenSize)
                    return;

                _remoteDesktop_UseFixedScreenSize = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_ScreenWidth = GlobalStaticConfiguration.RemoteDesktop_ScreenWidth;
        public int RemoteDesktop_ScreenWidth
        {
            get => _remoteDesktop_ScreenWidth;
            set
            {
                if (value == _remoteDesktop_ScreenWidth)
                    return;

                _remoteDesktop_ScreenWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_ScreenHeight = GlobalStaticConfiguration.RemoteDesktop_ScreenHeight;
        public int RemoteDesktop_ScreenHeight
        {
            get => _remoteDesktop_ScreenHeight;
            set
            {
                if (value == _remoteDesktop_ScreenHeight)
                    return;

                _remoteDesktop_ScreenHeight = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_UseCustomScreenSize;
        public bool RemoteDesktop_UseCustomScreenSize
        {
            get => _remoteDesktop_UseCustomScreenSize;
            set
            {
                if (value == _remoteDesktop_UseCustomScreenSize)
                    return;

                _remoteDesktop_UseCustomScreenSize = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_CustomScreenWidth;
        public int RemoteDesktop_CustomScreenWidth
        {
            get => _remoteDesktop_CustomScreenWidth;
            set
            {
                if (value == _remoteDesktop_CustomScreenWidth)
                    return;

                _remoteDesktop_CustomScreenWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_CustomScreenHeight;
        public int RemoteDesktop_CustomScreenHeight
        {
            get => _remoteDesktop_CustomScreenHeight;
            set
            {
                if (value == _remoteDesktop_CustomScreenHeight)
                    return;

                _remoteDesktop_CustomScreenHeight = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_ColorDepth = GlobalStaticConfiguration.RemoteDesktop_ColorDepth;
        public int RemoteDesktop_ColorDepth
        {
            get => _remoteDesktop_ColorDepth;
            set
            {
                if (value == _remoteDesktop_ColorDepth)
                    return;

                _remoteDesktop_ColorDepth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_Port = GlobalStaticConfiguration.RemoteDesktop_Port;
        public int RemoteDesktop_Port
        {
            get => _remoteDesktop_Port;
            set
            {
                if (value == _remoteDesktop_Port)
                    return;

                _remoteDesktop_Port = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_EnableCredSspSupport = true;
        public bool RemoteDesktop_EnableCredSspSupport
        {
            get => _remoteDesktop_EnableCredSspSupport;
            set
            {
                if (value == _remoteDesktop_EnableCredSspSupport)
                    return;

                _remoteDesktop_EnableCredSspSupport = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private uint _remoteDesktop_AuthenticationLevel = GlobalStaticConfiguration.RemoteDesktop_AuthenticationLevel;
        public uint RemoteDesktop_AuthenticationLevel
        {
            get => _remoteDesktop_AuthenticationLevel;
            set
            {
                if (value == _remoteDesktop_AuthenticationLevel)
                    return;

                _remoteDesktop_AuthenticationLevel = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private AudioRedirectionMode _remoteDesktop_AudioRedirectionMode = GlobalStaticConfiguration.RemoteDesktop_AudioRedirectionMode;
        public AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
        {
            get => _remoteDesktop_AudioRedirectionMode;
            set
            {
                if (value == _remoteDesktop_AudioRedirectionMode)
                    return;

                _remoteDesktop_AudioRedirectionMode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private AudioCaptureRedirectionMode _remoteDesktop_AudioCaptureRedirectionMode = GlobalStaticConfiguration.RemoteDesktop_AudioCaptureRedirectionMode;
        public AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
        {
            get => _remoteDesktop_AudioCaptureRedirectionMode;
            set
            {
                if (value == _remoteDesktop_AudioCaptureRedirectionMode)
                    return;

                _remoteDesktop_AudioCaptureRedirectionMode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private KeyboardHookMode _remoteDesktop_KeyboardHookMode = GlobalStaticConfiguration.RemoteDesktop_KeyboardHookMode;
        public KeyboardHookMode RemoteDesktop_KeyboardHookMode
        {
            get => _remoteDesktop_KeyboardHookMode;
            set
            {
                if (value == _remoteDesktop_KeyboardHookMode)
                    return;

                _remoteDesktop_KeyboardHookMode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectClipboard = true;
        public bool RemoteDesktop_RedirectClipboard
        {
            get => _remoteDesktop_RedirectClipboard;
            set
            {
                if (value == _remoteDesktop_RedirectClipboard)
                    return;

                _remoteDesktop_RedirectClipboard = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectDevices;
        public bool RemoteDesktop_RedirectDevices
        {
            get => _remoteDesktop_RedirectDevices;
            set
            {
                if (value == _remoteDesktop_RedirectDevices)
                    return;

                _remoteDesktop_RedirectDevices = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectDrives;
        public bool RemoteDesktop_RedirectDrives
        {
            get => _remoteDesktop_RedirectDrives;
            set
            {
                if (value == _remoteDesktop_RedirectDrives)
                    return;

                _remoteDesktop_RedirectDrives = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectPorts;
        public bool RemoteDesktop_RedirectPorts
        {
            get => _remoteDesktop_RedirectPorts;
            set
            {
                if (value == _remoteDesktop_RedirectPorts)
                    return;

                _remoteDesktop_RedirectPorts = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectSmartCards;
        public bool RemoteDesktop_RedirectSmartCards
        {
            get => _remoteDesktop_RedirectSmartCards;
            set
            {
                if (value == _remoteDesktop_RedirectSmartCards)
                    return;

                _remoteDesktop_RedirectSmartCards = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectPrinters;
        public bool RemoteDesktop_RedirectPrinters
        {
            get => _remoteDesktop_RedirectPrinters;
            set
            {
                if (value == _remoteDesktop_RedirectPrinters)
                    return;

                _remoteDesktop_RedirectPrinters = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_PersistentBitmapCaching;
        public bool RemoteDesktop_PersistentBitmapCaching
        {
            get => _remoteDesktop_PersistentBitmapCaching;
            set
            {
                if (value == _remoteDesktop_PersistentBitmapCaching)
                    return;

                _remoteDesktop_PersistentBitmapCaching = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_ReconnectIfTheConnectionIsDropped;
        public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped
        {
            get => _remoteDesktop_ReconnectIfTheConnectionIsDropped;
            set
            {
                if (value == _remoteDesktop_ReconnectIfTheConnectionIsDropped)
                    return;

                _remoteDesktop_ReconnectIfTheConnectionIsDropped = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private NetworkConnectionType _remoteDesktop_NetworkConnectionType = GlobalStaticConfiguration.RemoteDesktop_NetworkConnectionType;
        public NetworkConnectionType RemoteDesktop_NetworkConnectionType
        {
            get => _remoteDesktop_NetworkConnectionType;
            set
            {
                if (value == _remoteDesktop_NetworkConnectionType)
                    return;

                _remoteDesktop_NetworkConnectionType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_DesktopBackground;
        public bool RemoteDesktop_DesktopBackground
        {
            get => _remoteDesktop_DesktopBackground;
            set
            {
                if (value == _remoteDesktop_DesktopBackground)
                    return;

                _remoteDesktop_DesktopBackground = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_FontSmoothing;
        public bool RemoteDesktop_FontSmoothing
        {
            get => _remoteDesktop_FontSmoothing;
            set
            {
                if (value == _remoteDesktop_FontSmoothing)
                    return;

                _remoteDesktop_FontSmoothing = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_DesktopComposition;
        public bool RemoteDesktop_DesktopComposition
        {
            get => _remoteDesktop_DesktopComposition;
            set
            {
                if (value == _remoteDesktop_DesktopComposition)
                    return;

                _remoteDesktop_DesktopComposition = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_ShowWindowContentsWhileDragging;
        public bool RemoteDesktop_ShowWindowContentsWhileDragging
        {
            get => _remoteDesktop_ShowWindowContentsWhileDragging;
            set
            {
                if (value == _remoteDesktop_ShowWindowContentsWhileDragging)
                    return;

                _remoteDesktop_ShowWindowContentsWhileDragging = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_MenuAndWindowAnimation;
        public bool RemoteDesktop_MenuAndWindowAnimation
        {
            get => _remoteDesktop_MenuAndWindowAnimation;
            set
            {
                if (value == _remoteDesktop_MenuAndWindowAnimation)
                    return;

                _remoteDesktop_MenuAndWindowAnimation = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_VisualStyles;
        public bool RemoteDesktop_VisualStyles
        {
            get => _remoteDesktop_VisualStyles;
            set
            {
                if (value == _remoteDesktop_VisualStyles)
                    return;

                _remoteDesktop_VisualStyles = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_ExpandProfileView = true;
        public bool RemoteDesktop_ExpandProfileView
        {
            get => _remoteDesktop_ExpandProfileView;
            set
            {
                if (value == _remoteDesktop_ExpandProfileView)
                    return;

                _remoteDesktop_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _remoteDesktop_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double RemoteDesktop_ProfileWidth
        {
            get => _remoteDesktop_ProfileWidth;
            set
            {
                if (Math.Abs(value - _remoteDesktop_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _remoteDesktop_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region PowerShell
        private ObservableCollection<string> _powerShell_HostHistory = new();
        public ObservableCollection<string> PowerShell_HostHistory
        {
            get => _powerShell_HostHistory;
            set
            {
                if (value == _powerShell_HostHistory)
                    return;

                _powerShell_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _powerShell_ApplicationFilePath;
        public string PowerShell_ApplicationFilePath
        {
            get => _powerShell_ApplicationFilePath;
            set
            {
                if (value == _powerShell_ApplicationFilePath)
                    return;

                _powerShell_ApplicationFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _powerShell_Command = GlobalStaticConfiguration.PowerShell_Command;
        public string PowerShell_Command
        {
            get => _powerShell_Command;
            set
            {
                if (value == _powerShell_Command)
                    return;

                _powerShell_Command = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _powerShell_AdditionalCommandLine;
        public string PowerShell_AdditionalCommandLine
        {
            get => _powerShell_AdditionalCommandLine;
            set
            {
                if (value == _powerShell_AdditionalCommandLine)
                    return;

                _powerShell_AdditionalCommandLine = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private Models.PowerShell.PowerShell.ExecutionPolicy _powerShell_ExecutionPolicy = GlobalStaticConfiguration.PowerShell_ExecutionPolicy;
        public Models.PowerShell.PowerShell.ExecutionPolicy PowerShell_ExecutionPolicy
        {
            get => _powerShell_ExecutionPolicy;
            set
            {
                if (value == _powerShell_ExecutionPolicy)
                    return;

                _powerShell_ExecutionPolicy = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _powerShell_ExpandProfileView = true;
        public bool PowerShell_ExpandProfileView
        {
            get => _powerShell_ExpandProfileView;
            set
            {
                if (value == _powerShell_ExpandProfileView)
                    return;

                _powerShell_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _powerShell_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double PowerShell_ProfileWidth
        {
            get => _powerShell_ProfileWidth;
            set
            {
                if (Math.Abs(value - _powerShell_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _powerShell_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region PuTTY
        private ObservableCollection<string> _puTTY_HostHistory = new();
        public ObservableCollection<string> PuTTY_HostHistory
        {
            get => _puTTY_HostHistory;
            set
            {
                if (value == _puTTY_HostHistory)
                    return;

                _puTTY_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ConnectionMode _puTTY_DefaultConnectionMode = GlobalStaticConfiguration.PuTTY_DefaultConnectionMode;
        public ConnectionMode PuTTY_DefaultConnectionMode
        {
            get => _puTTY_DefaultConnectionMode;
            set
            {
                if (value == _puTTY_DefaultConnectionMode)
                    return;

                _puTTY_DefaultConnectionMode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_Username;
        public string PuTTY_Username
        {
            get => _puTTY_Username;
            set
            {
                if (value == _puTTY_Username)
                    return;

                _puTTY_Username = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_PrivateKeyFile;
        public string PuTTY_PrivateKeyFile
        {
            get => _puTTY_PrivateKeyFile;
            set
            {
                if (value == _puTTY_PrivateKeyFile)
                    return;

                _puTTY_PrivateKeyFile = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_Profile = GlobalStaticConfiguration.PuTTY_DefaultProfile;
        public string PuTTY_Profile
        {
            get => _puTTY_Profile;
            set
            {
                if (value == _puTTY_Profile)
                    return;

                _puTTY_Profile = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _puTTY_EnableSessionLog;
        public bool PuTTY_EnableSessionLog
        {
            get => _puTTY_EnableSessionLog;
            set
            {
                if (value == _puTTY_EnableSessionLog)
                    return;

                _puTTY_EnableSessionLog = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private LogMode _puTTY_LogMode = GlobalStaticConfiguration.PuTTY_LogMode;
        public LogMode PuTTY_LogMode
        {
            get => _puTTY_LogMode;
            set
            {
                if (value == _puTTY_LogMode)
                    return;

                _puTTY_LogMode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_LogPath = GlobalStaticConfiguration.PuTTY_LogPath;
        public string PuTTY_LogPath
        {
            get => _puTTY_LogPath;
            set
            {
                if (value == _puTTY_LogPath)
                    return;

                _puTTY_LogPath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_LogFileName = GlobalStaticConfiguration.PuTTY_LogFileName;
        public string PuTTY_LogFileName
        {
            get => _puTTY_LogFileName;
            set
            {
                if (value == _puTTY_LogFileName)
                    return;

                _puTTY_LogFileName = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_AdditionalCommandLine;
        public string PuTTY_AdditionalCommandLine
        {
            get => _puTTY_AdditionalCommandLine;
            set
            {
                if (value == _puTTY_AdditionalCommandLine)
                    return;

                _puTTY_AdditionalCommandLine = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _puTTY_SerialLineHistory = new();
        public ObservableCollection<string> PuTTY_SerialLineHistory
        {
            get => _puTTY_SerialLineHistory;
            set
            {
                if (value == _puTTY_SerialLineHistory)
                    return;

                _puTTY_SerialLineHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _puTTY_PortHistory = new();
        public ObservableCollection<string> PuTTY_PortHistory
        {
            get => _puTTY_PortHistory;
            set
            {
                if (value == _puTTY_PortHistory)
                    return;

                _puTTY_PortHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _puTTY_BaudHistory = new();
        public ObservableCollection<string> PuTTY_BaudHistory
        {
            get => _puTTY_BaudHistory;
            set
            {
                if (value == _puTTY_BaudHistory)
                    return;

                _puTTY_BaudHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _puTTY_UsernameHistory = new();
        public ObservableCollection<string> PuTTY_UsernameHistory
        {
            get => _puTTY_UsernameHistory;
            set
            {
                if (value == _puTTY_UsernameHistory)
                    return;

                _puTTY_UsernameHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _puTTY_PrivateKeyFileHistory = new();
        public ObservableCollection<string> PuTTY_PrivateKeyFileHistory
        {
            get => _puTTY_PrivateKeyFileHistory;
            set
            {
                if (value == _puTTY_PrivateKeyFileHistory)
                    return;

                _puTTY_PrivateKeyFileHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _puTTY_ProfileHistory = new();
        public ObservableCollection<string> PuTTY_ProfileHistory
        {
            get => _puTTY_ProfileHistory;
            set
            {
                if (value == _puTTY_ProfileHistory)
                    return;

                _puTTY_ProfileHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _puTTY_ExpandProfileView = true;
        public bool PuTTY_ExpandProfileView
        {
            get => _puTTY_ExpandProfileView;
            set
            {
                if (value == _puTTY_ExpandProfileView)
                    return;

                _puTTY_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _puTTY_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double PuTTY_ProfileWidth
        {
            get => _puTTY_ProfileWidth;
            set
            {
                if (Math.Abs(value - _puTTY_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _puTTY_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_ApplicationFilePath;
        public string PuTTY_ApplicationFilePath
        {
            get => _puTTY_ApplicationFilePath;
            set
            {
                if (value == _puTTY_ApplicationFilePath)
                    return;

                _puTTY_ApplicationFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _puTTY_SerialLine = GlobalStaticConfiguration.PuTTY_SerialLine;
        public string PuTTY_SerialLine
        {
            get => _puTTY_SerialLine;
            set
            {
                if (value == _puTTY_SerialLine)
                    return;

                _puTTY_SerialLine = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _puTTY_SSHPort = GlobalStaticConfiguration.PuTTY_SSHPort;
        public int PuTTY_SSHPort
        {
            get => _puTTY_SSHPort;
            set
            {
                if (value == _puTTY_SSHPort)
                    return;

                _puTTY_SSHPort = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _puTTY_TelnetPort = GlobalStaticConfiguration.PuTTY_TelnetPort;
        public int PuTTY_TelnetPort
        {
            get => _puTTY_TelnetPort;
            set
            {
                if (value == _puTTY_TelnetPort)
                    return;

                _puTTY_TelnetPort = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _puTTY_RloginPort = GlobalStaticConfiguration.PuTTY_RloginPort;
        public int PuTTY_RloginPort
        {
            get => _puTTY_RloginPort;
            set
            {
                if (value == _puTTY_RloginPort)
                    return;

                _puTTY_RloginPort = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _puTTY_BaudRate = GlobalStaticConfiguration.PuTTY_BaudRate;
        public int PuTTY_BaudRate
        {
            get => _puTTY_BaudRate;
            set
            {
                if (value == _puTTY_BaudRate)
                    return;

                _puTTY_BaudRate = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _puTTY_DefaultRaw = GlobalStaticConfiguration.PuTTY_Raw;
        public int PuTTY_DefaultRaw
        {
            get => _puTTY_DefaultRaw;
            set
            {
                if (value == _puTTY_DefaultRaw)
                    return;

                _puTTY_DefaultRaw = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region AWSSessionManager
        private bool _awsSessionManager_EnableSyncInstanceIDsFromAWS = GlobalStaticConfiguration.AWSSessionManager_EnableSyncInstanceIDsFromAWS;
        public bool AWSSessionManager_EnableSyncInstanceIDsFromAWS
        {
            get => _awsSessionManager_EnableSyncInstanceIDsFromAWS;
            set
            {
                if (value == _awsSessionManager_EnableSyncInstanceIDsFromAWS)
                    return;

                _awsSessionManager_EnableSyncInstanceIDsFromAWS = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<AWSProfileInfo> _awsSessionManager_AWSProfiles = new();
        public ObservableCollection<AWSProfileInfo> AWSSessionManager_AWSProfiles
        {
            get => _awsSessionManager_AWSProfiles;
            set
            {
                if (value == _awsSessionManager_AWSProfiles)
                    return;

                _awsSessionManager_AWSProfiles = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool awsSessionManager_SyncOnlyRunningInstancesFromAWS = GlobalStaticConfiguration.AWSSessionManager_SyncOnlyRunningInstancesFromAWS;
        public bool AWSSessionManager_SyncOnlyRunningInstancesFromAWS
        {
            get => awsSessionManager_SyncOnlyRunningInstancesFromAWS;
            set
            {
                if (value == awsSessionManager_SyncOnlyRunningInstancesFromAWS)
                    return;

                awsSessionManager_SyncOnlyRunningInstancesFromAWS = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _awsSessionManager_DefaultProfile;
        public string AWSSessionManager_DefaultProfile
        {
            get => _awsSessionManager_DefaultProfile;
            set
            {
                if (value == _awsSessionManager_DefaultProfile)
                    return;

                _awsSessionManager_DefaultProfile = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _awsSessionManager_DefaultRegion;
        public string AWSSessionManager_DefaultRegion
        {
            get => _awsSessionManager_DefaultRegion;
            set
            {
                if (value == _awsSessionManager_DefaultRegion)
                    return;

                _awsSessionManager_DefaultRegion = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _awsSessionManager_ApplicationFilePath;
        public string AWSSessionManager_ApplicationFilePath
        {
            get => _awsSessionManager_ApplicationFilePath;
            set
            {
                if (value == _awsSessionManager_ApplicationFilePath)
                    return;

                _awsSessionManager_ApplicationFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _awsSessionManager_InstanceIDHistory = new();
        public ObservableCollection<string> AWSSessionManager_InstanceIDHistory
        {
            get => _awsSessionManager_InstanceIDHistory;
            set
            {
                if (value == _awsSessionManager_InstanceIDHistory)
                    return;

                _awsSessionManager_InstanceIDHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _awsSessionManager_ProfileHistory = new();
        public ObservableCollection<string> AWSSessionManager_ProfileHistory
        {
            get => _awsSessionManager_ProfileHistory;
            set
            {
                if (value == _awsSessionManager_ProfileHistory)
                    return;

                _awsSessionManager_ProfileHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _awsSessionManager_RegionHistory = new();
        public ObservableCollection<string> AWSSessionManager_RegionHistory
        {
            get => _awsSessionManager_RegionHistory;
            set
            {
                if (value == _awsSessionManager_RegionHistory)
                    return;

                _awsSessionManager_RegionHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _awsSessionManager_ExpandProfileView = true;
        public bool AWSSessionManager_ExpandProfileView
        {
            get => _awsSessionManager_ExpandProfileView;
            set
            {
                if (value == _awsSessionManager_ExpandProfileView)
                    return;

                _awsSessionManager_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _awsSessionManager_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double AWSSessionManager_ProfileWidth
        {
            get => _awsSessionManager_ProfileWidth;
            set
            {
                if (Math.Abs(value - _awsSessionManager_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _awsSessionManager_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region TigerVNC
        private ObservableCollection<string> _tigerVNC_HostHistory = new();
        public ObservableCollection<string> TigerVNC_HostHistory
        {
            get => _tigerVNC_HostHistory;
            set
            {
                if (value == _tigerVNC_HostHistory)
                    return;

                _tigerVNC_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<int> _tigerVNC_PortHistory = new();
        public ObservableCollection<int> TigerVNC_PortHistory
        {
            get => _tigerVNC_PortHistory;
            set
            {
                if (value == _tigerVNC_PortHistory)
                    return;

                _tigerVNC_PortHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _tigerVNC_ExpandProfileView = true;
        public bool TigerVNC_ExpandProfileView
        {
            get => _tigerVNC_ExpandProfileView;
            set
            {
                if (value == _tigerVNC_ExpandProfileView)
                    return;

                _tigerVNC_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _tigerVNC_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double TigerVNC_ProfileWidth
        {
            get => _tigerVNC_ProfileWidth;
            set
            {
                if (Math.Abs(value - _tigerVNC_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _tigerVNC_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _tigerVNC_ApplicationFilePath;
        public string TigerVNC_ApplicationFilePath
        {
            get => _tigerVNC_ApplicationFilePath;
            set
            {
                if (value == _tigerVNC_ApplicationFilePath)
                    return;

                _tigerVNC_ApplicationFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _tigerVNC_Port = GlobalStaticConfiguration.TigerVNC_DefaultVNCPort;
        public int TigerVNC_Port
        {
            get => _tigerVNC_Port;
            set
            {
                if (value == _tigerVNC_Port)
                    return;

                _tigerVNC_Port = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region WebConsole
        private ObservableCollection<string> _webConsole_UrlHistory = new();
        public ObservableCollection<string> WebConsole_UrlHistory
        {
            get => _webConsole_UrlHistory;
            set
            {
                if (value == _webConsole_UrlHistory)
                    return;

                _webConsole_UrlHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _webConsole_ExpandProfileView = true;
        public bool WebConsole_ExpandProfileView
        {
            get => _webConsole_ExpandProfileView;
            set
            {
                if (value == _webConsole_ExpandProfileView)
                    return;

                _webConsole_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _webConsole_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double WebConsole_ProfileWidth
        {
            get => _webConsole_ProfileWidth;
            set
            {
                if (Math.Abs(value - _webConsole_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _webConsole_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region SNMP
        private WalkMode _snmp_WalkMode = GlobalStaticConfiguration.SNMP_WalkMode;
        public WalkMode SNMP_WalkMode
        {
            get => _snmp_WalkMode;
            set
            {
                if (value == _snmp_WalkMode)
                    return;

                _snmp_WalkMode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _snmp_Timeout = GlobalStaticConfiguration.SNMP_Timeout;
        public int SNMP_Timeout
        {
            get => _snmp_Timeout;
            set
            {
                if (value == _snmp_Timeout)
                    return;

                _snmp_Timeout = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _snmp_port = 161;
        public int SNMP_Port
        {
            get => _snmp_port;
            set
            {
                if (value == _snmp_port)
                    return;

                _snmp_port = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _snmp_ResolveHostnamePreferIPv4 = true;
        public bool SNMP_ResolveHostnamePreferIPv4
        {
            get => _snmp_ResolveHostnamePreferIPv4;
            set
            {
                if (value == _snmp_ResolveHostnamePreferIPv4)
                    return;

                _snmp_ResolveHostnamePreferIPv4 = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _snmp_HostHistory = new();
        public ObservableCollection<string> SNMP_HostHistory
        {
            get => _snmp_HostHistory;
            set
            {
                if (value == _snmp_HostHistory)
                    return;

                _snmp_HostHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _snmp_OIDHistory = new();
        public ObservableCollection<string> SNMP_OIDHistory
        {
            get => _snmp_OIDHistory;
            set
            {
                if (value == _snmp_OIDHistory)
                    return;

                _snmp_OIDHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private SNMPMode _snmp_Mode = GlobalStaticConfiguration.SNMP_Mode;
        public SNMPMode SNMP_Mode
        {
            get => _snmp_Mode;
            set
            {
                if (value == _snmp_Mode)
                    return;

                _snmp_Mode = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private SNMPVersion _snmp_Version = GlobalStaticConfiguration.SNMP_Version;
        public SNMPVersion SNMP_Version
        {
            get => _snmp_Version;
            set
            {
                if (value == _snmp_Version)
                    return;

                _snmp_Version = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private SNMPV3Security _snmp_Security = GlobalStaticConfiguration.SNMP_Security;
        public SNMPV3Security SNMP_Security
        {
            get => _snmp_Security;
            set
            {
                if (value == _snmp_Security)
                    return;

                _snmp_Security = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private SNMPV3AuthenticationProvider _snmp_AuthenticationProvider = GlobalStaticConfiguration.SNMP_AuthenticationProvider;
        public SNMPV3AuthenticationProvider SNMP_AuthenticationProvider
        {
            get => _snmp_AuthenticationProvider;
            set
            {
                if (value == _snmp_AuthenticationProvider)
                    return;

                _snmp_AuthenticationProvider = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private SNMPV3PrivacyProvider _snmp_PrivacyProvider = GlobalStaticConfiguration.SNMP_PrivacyProvider;
        public SNMPV3PrivacyProvider SNMP_PrivacyProvider
        {
            get => _snmp_PrivacyProvider;
            set
            {
                if (value == _snmp_PrivacyProvider)
                    return;

                _snmp_PrivacyProvider = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _snmp_ExportFilePath;
        public string SNMP_ExportFilePath
        {
            get => _snmp_ExportFilePath;
            set
            {
                if (value == _snmp_ExportFilePath)
                    return;

                _snmp_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _snmp_ExportFileType = GlobalStaticConfiguration.SNMP_ExportFileType;
        public ExportManager.ExportFileType SNMP_ExportFileType
        {
            get => _snmp_ExportFileType;
            set
            {
                if (value == _snmp_ExportFileType)
                    return;

                _snmp_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Discovery Protocol
        private string _discoveryProtocol_SelectedInterfaceId;
        public string DiscoveryProtocol_InterfaceId
        {
            get => _discoveryProtocol_SelectedInterfaceId;
            set
            {
                if (value == _discoveryProtocol_SelectedInterfaceId)
                    return;

                _discoveryProtocol_SelectedInterfaceId = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private DiscoveryProtocol.Protocol _discoveryProtocol_Protocol = GlobalStaticConfiguration.DiscoveryProtocol_Protocol;
        public DiscoveryProtocol.Protocol DiscoveryProtocol_Protocol
        {
            get => _discoveryProtocol_Protocol;
            set
            {
                if (value == _discoveryProtocol_Protocol)
                    return;

                _discoveryProtocol_Protocol = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private int _discoveryProtocol_Duration = GlobalStaticConfiguration.DiscoveryProtocol_Duration;
        public int DiscoveryProtocol_Duration
        {
            get => _discoveryProtocol_Duration;
            set
            {
                if (value == _discoveryProtocol_Duration)
                    return;

                _discoveryProtocol_Duration = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region WakeOnLAN
        private int _wakeOnLAN_Port = GlobalStaticConfiguration.WakeOnLAN_Port;
        public int WakeOnLAN_Port
        {
            get => _wakeOnLAN_Port;
            set
            {
                if (value == _wakeOnLAN_Port)
                    return;

                _wakeOnLAN_Port = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _wakeOnLAN_ExpandClientView = true;
        public bool WakeOnLAN_ExpandClientView
        {
            get => _wakeOnLAN_ExpandClientView;
            set
            {
                if (value == _wakeOnLAN_ExpandClientView)
                    return;

                _wakeOnLAN_ExpandClientView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _wakeOnLAN_ClientWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double WakeOnLAN_ClientWidth
        {
            get => _wakeOnLAN_ClientWidth;
            set
            {
                if (Math.Abs(value - _wakeOnLAN_ClientWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _wakeOnLAN_ClientWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Subnet Calculator

        #region Calculator
        private ObservableCollection<string> _subnetCalculator_Calculator_SubnetHistory = new();
        public ObservableCollection<string> SubnetCalculator_Calculator_SubnetHistory
        {
            get => _subnetCalculator_Calculator_SubnetHistory;
            set
            {
                if (value == _subnetCalculator_Calculator_SubnetHistory)
                    return;

                _subnetCalculator_Calculator_SubnetHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Subnetting
        private ObservableCollection<string> _subnetCalculator_Subnetting_SubnetHistory = new();
        public ObservableCollection<string> SubnetCalculator_Subnetting_SubnetHistory
        {
            get => _subnetCalculator_Subnetting_SubnetHistory;
            set
            {
                if (value == _subnetCalculator_Subnetting_SubnetHistory)
                    return;

                _subnetCalculator_Subnetting_SubnetHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _subnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory = new();
        public ObservableCollection<string> SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory
        {
            get => _subnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory;
            set
            {
                if (value == _subnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory)
                    return;

                _subnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _subnetCalculator_Subnetting_ExportFilePath;
        public string SubnetCalculator_Subnetting_ExportFilePath
        {
            get => _subnetCalculator_Subnetting_ExportFilePath;
            set
            {
                if (value == _subnetCalculator_Subnetting_ExportFilePath)
                    return;

                _subnetCalculator_Subnetting_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _subnetCalculator_Subnetting_ExportFileType = GlobalStaticConfiguration.SubnetCalculator_Subnetting_ExportFileType;
        public ExportManager.ExportFileType SubnetCalculator_Subnetting_ExportFileType
        {
            get => _subnetCalculator_Subnetting_ExportFileType;
            set
            {
                if (value == _subnetCalculator_Subnetting_ExportFileType)
                    return;

                _subnetCalculator_Subnetting_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region WideSubnet
        private ObservableCollection<string> _subnetCalculator_WideSubnet_Subnet1 = new();
        public ObservableCollection<string> SubnetCalculator_WideSubnet_Subnet1
        {
            get => _subnetCalculator_WideSubnet_Subnet1;
            set
            {
                if (value == _subnetCalculator_WideSubnet_Subnet1)
                    return;

                _subnetCalculator_WideSubnet_Subnet1 = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _subnetCalculator_WideSubnet_Subnet2 = new();
        public ObservableCollection<string> SubnetCalculator_WideSubnet_Subnet2
        {
            get => _subnetCalculator_WideSubnet_Subnet2;
            set
            {
                if (value == _subnetCalculator_WideSubnet_Subnet2)
                    return;

                _subnetCalculator_WideSubnet_Subnet2 = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #endregion

        #region Lookup
        private ObservableCollection<string> _lookup_OUI_MACAddressOrVendorHistory = new();
        public ObservableCollection<string> Lookup_OUI_MACAddressOrVendorHistory
        {
            get => _lookup_OUI_MACAddressOrVendorHistory;
            set
            {
                if (value == _lookup_OUI_MACAddressOrVendorHistory)
                    return;

                _lookup_OUI_MACAddressOrVendorHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _lookup_OUI_ExportFilePath;
        public string Lookup_OUI_ExportFilePath
        {
            get => _lookup_OUI_ExportFilePath;
            set
            {
                if (value == _lookup_OUI_ExportFilePath)
                    return;

                _lookup_OUI_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _lookup_OUI_ExportFileType = GlobalStaticConfiguration.Lookup_OUI_ExportFileType;
        public ExportManager.ExportFileType Lookup_OUI_ExportFileType
        {
            get => _lookup_OUI_ExportFileType;
            set
            {
                if (value == _lookup_OUI_ExportFileType)
                    return;

                _lookup_OUI_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ObservableCollection<string> _lookup_Port_PortsHistory = new();
        public ObservableCollection<string> Lookup_Port_PortsHistory
        {
            get => _lookup_Port_PortsHistory;
            set
            {
                if (value == _lookup_Port_PortsHistory)
                    return;

                _lookup_Port_PortsHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _lookup_Port_ExportFilePath;
        public string Lookup_Port_ExportFilePath
        {
            get => _lookup_Port_ExportFilePath;
            set
            {
                if (value == _lookup_Port_ExportFilePath)
                    return;

                _lookup_Port_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _lookup_Port_ExportFileType = GlobalStaticConfiguration.Lookup_Port_ExportFileType;
        public ExportManager.ExportFileType Lookup_Port_ExportFileType
        {
            get => _lookup_Port_ExportFileType;
            set
            {
                if (value == _lookup_Port_ExportFileType)
                    return;

                _lookup_Port_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Whois
        private ObservableCollection<string> _whois_DomainHistory = new();
        public ObservableCollection<string> Whois_DomainHistory
        {
            get => _whois_DomainHistory;
            set
            {
                if (value == _whois_DomainHistory)
                    return;

                _whois_DomainHistory = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private bool _whois_ExpandProfileView = true;
        public bool Whois_ExpandProfileView
        {
            get => _whois_ExpandProfileView;
            set
            {
                if (value == _whois_ExpandProfileView)
                    return;

                _whois_ExpandProfileView = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private double _whois_ProfileWidth = GlobalStaticConfiguration.Profile_DefaultWidthExpanded;
        public double Whois_ProfileWidth
        {
            get => _whois_ProfileWidth;
            set
            {
                if (Math.Abs(value - _whois_ProfileWidth) < GlobalStaticConfiguration.FloatPointFix)
                    return;

                _whois_ProfileWidth = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _whois_ExportFilePath;
        public string Whois_ExportFilePath
        {
            get => _whois_ExportFilePath;
            set
            {
                if (value == _whois_ExportFilePath)
                    return;

                _whois_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _whois_ExportFileType = GlobalStaticConfiguration.Whois_ExportFileType;
        public ExportManager.ExportFileType Whois_ExportFileType
        {
            get => _whois_ExportFileType;
            set
            {
                if (value == _whois_ExportFileType)
                    return;

                _whois_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Connections
        private bool _connections_AutoRefresh;
        public bool Connections_AutoRefresh
        {
            get => _connections_AutoRefresh;
            set
            {
                if (value == _connections_AutoRefresh)
                    return;

                _connections_AutoRefresh = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private AutoRefreshTimeInfo _connections_AutoRefreshTime = GlobalStaticConfiguration.Connections_AutoRefreshTime;
        public AutoRefreshTimeInfo Connections_AutoRefreshTime
        {
            get => _connections_AutoRefreshTime;
            set
            {
                if (value == _connections_AutoRefreshTime)
                    return;

                _connections_AutoRefreshTime = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _connections_ExportFilePath;
        public string Connections_ExportFilePath
        {
            get => _connections_ExportFilePath;
            set
            {
                if (value == _connections_ExportFilePath)
                    return;

                _connections_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _connections_ExportFileType = GlobalStaticConfiguration.Connections_ExportFileType;
        public ExportManager.ExportFileType Connections_ExportFileType
        {
            get => _connections_ExportFileType;
            set
            {
                if (value == _connections_ExportFileType)
                    return;

                _connections_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region Listeners
        private bool _listeners_AutoRefresh;
        public bool Listeners_AutoRefresh
        {
            get => _listeners_AutoRefresh;
            set
            {
                if (value == _listeners_AutoRefresh)
                    return;

                _listeners_AutoRefresh = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private AutoRefreshTimeInfo _listeners_AutoRefreshTime = GlobalStaticConfiguration.Listeners_AutoRefreshTime;
        public AutoRefreshTimeInfo Listeners_AutoRefreshTime
        {
            get => _listeners_AutoRefreshTime;
            set
            {
                if (value == _listeners_AutoRefreshTime)
                    return;

                _listeners_AutoRefreshTime = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _listeners_ExportFilePath;
        public string Listeners_ExportFilePath
        {
            get => _listeners_ExportFilePath;
            set
            {
                if (value == _listeners_ExportFilePath)
                    return;

                _listeners_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _listeners_ExportFileType = GlobalStaticConfiguration.Listeners_ExportFileType;
        public ExportManager.ExportFileType Listeners_ExportFileType
        {
            get => _listeners_ExportFileType;
            set
            {
                if (value == _listeners_ExportFileType)
                    return;

                _listeners_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #region ARPTable
        private bool _arpTable_AutoRefresh;
        public bool ARPTable_AutoRefresh
        {
            get => _arpTable_AutoRefresh;
            set
            {
                if (value == _arpTable_AutoRefresh)
                    return;

                _arpTable_AutoRefresh = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private AutoRefreshTimeInfo _arpTable_AutoRefreshTime = GlobalStaticConfiguration.ARPTable_AutoRefreshTime;
        public AutoRefreshTimeInfo ARPTable_AutoRefreshTime
        {
            get => _arpTable_AutoRefreshTime;
            set
            {
                if (value == _arpTable_AutoRefreshTime)
                    return;

                _arpTable_AutoRefreshTime = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private string _arpTable_ExportFilePath;
        public string ARPTable_ExportFilePath
        {
            get => _arpTable_ExportFilePath;
            set
            {
                if (value == _arpTable_ExportFilePath)
                    return;

                _arpTable_ExportFilePath = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }

        private ExportManager.ExportFileType _arpTable_ExportFileType = GlobalStaticConfiguration.ARPTable_ExportFileType;
        public ExportManager.ExportFileType ARPTable_ExportFileType
        {
            get => _arpTable_ExportFileType;
            set
            {
                if (value == _arpTable_ExportFileType)
                    return;

                _arpTable_ExportFileType = value;
                OnPropertyChanged();
                SettingsChanged = true;
            }
        }
        #endregion

        #endregion

        #region Constructor
        public SettingsInfo()
        {
            // General
            General_ApplicationList.CollectionChanged += CollectionChanged;

            // IP Scanner
            IPScanner_HostsHistory.CollectionChanged += CollectionChanged;
            IPScanner_CustomCommands.CollectionChanged += CollectionChanged;

            // Port Scanner
            PortScanner_HostsHistory.CollectionChanged += CollectionChanged;
            PortScanner_PortsHistory.CollectionChanged += CollectionChanged;
            PortScanner_PortProfiles.CollectionChanged += CollectionChanged;

            // Ping Monitor
            PingMonitor_HostHistory.CollectionChanged += CollectionChanged;

            // Traceroute
            Traceroute_HostHistory.CollectionChanged += CollectionChanged;

            // DNS Lookup
            DNSLookup_HostHistory.CollectionChanged += CollectionChanged;
            DNSLookup_DNSServers.CollectionChanged += CollectionChanged;

            // Remote Desktop
            RemoteDesktop_HostHistory.CollectionChanged += CollectionChanged;

            // PowerShell
            PowerShell_HostHistory.CollectionChanged += CollectionChanged;

            // PuTTY
            PuTTY_HostHistory.CollectionChanged += CollectionChanged;
            PuTTY_SerialLineHistory.CollectionChanged += CollectionChanged;
            PuTTY_PortHistory.CollectionChanged += CollectionChanged;
            PuTTY_BaudHistory.CollectionChanged += CollectionChanged;
            PuTTY_UsernameHistory.CollectionChanged += CollectionChanged;
            PuTTY_PrivateKeyFileHistory.CollectionChanged += CollectionChanged;
            PuTTY_ProfileHistory.CollectionChanged += CollectionChanged;

            // AWSSessionManager
            AWSSessionManager_InstanceIDHistory.CollectionChanged += CollectionChanged;
            AWSSessionManager_AWSProfiles.CollectionChanged += CollectionChanged;

            // TigerVNC
            TigerVNC_HostHistory.CollectionChanged += CollectionChanged;
            TigerVNC_PortHistory.CollectionChanged += CollectionChanged;

            // WebConsole
            WebConsole_UrlHistory.CollectionChanged += CollectionChanged;

            // SNMP
            SNMP_HostHistory.CollectionChanged += CollectionChanged;
            SNMP_OIDHistory.CollectionChanged += CollectionChanged;

            // Subnet Calculator / Calculator
            SubnetCalculator_Calculator_SubnetHistory.CollectionChanged += CollectionChanged;

            // Subnet Calculator / Subnetting
            SubnetCalculator_Subnetting_SubnetHistory.CollectionChanged += CollectionChanged;
            SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.CollectionChanged += CollectionChanged;

            // Subnet Calculator / Supernetting
            SubnetCalculator_WideSubnet_Subnet1.CollectionChanged += CollectionChanged;
            SubnetCalculator_WideSubnet_Subnet2.CollectionChanged += CollectionChanged;

            // Lookup / OUI
            Lookup_OUI_MACAddressOrVendorHistory.CollectionChanged += CollectionChanged;

            // Lookup / Port
            Lookup_Port_PortsHistory.CollectionChanged += CollectionChanged;

            // Whois
            Whois_DomainHistory.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SettingsChanged = true;
        }
        #endregion
    }
}