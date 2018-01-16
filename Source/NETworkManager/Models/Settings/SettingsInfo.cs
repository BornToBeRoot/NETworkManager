using Heijden.DNS;
using Lextm.SharpSnmpLib.Messaging;
using NETworkManager.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static NETworkManager.Models.Network.SNMP;

namespace NETworkManager.Models.Settings
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
        [XmlIgnore]
        public bool SettingsChanged { get; set; }

        #region General 
        // Window
        private bool _window_ConfirmClose;
        public bool Window_ConfirmClose
        {
            get { return _window_ConfirmClose; }
            set
            {
                if (value == _window_ConfirmClose)
                    return;

                _window_ConfirmClose = value;
                SettingsChanged = true;
            }
        }

        private bool _window_MinimizeInsteadOfTerminating;
        public bool Window_MinimizeInsteadOfTerminating
        {
            get { return _window_MinimizeInsteadOfTerminating; }
            set
            {
                if (value == _window_MinimizeInsteadOfTerminating)
                    return;

                _window_MinimizeInsteadOfTerminating = value;
                SettingsChanged = true;
            }
        }

        private bool _window_MultipleInstances;
        public bool Window_MultipleInstances
        {
            get { return _window_MultipleInstances; }
            set
            {
                if (value == _window_MultipleInstances)
                    return;

                _window_MultipleInstances = value;
                SettingsChanged = true;
            }
        }

        private bool _window_MinimizeToTrayInsteadOfTaskbar;
        public bool Window_MinimizeToTrayInsteadOfTaskbar
        {
            get { return _window_MinimizeToTrayInsteadOfTaskbar; }
            set
            {
                if (value == _window_MinimizeToTrayInsteadOfTaskbar)
                    return;

                _window_MinimizeToTrayInsteadOfTaskbar = value;
                SettingsChanged = true;
            }
        }

        // Applications        
        private ApplicationViewManager.Name _application_DefaultApplicationViewName = ApplicationViewManager.Name.NetworkInterface;
        public ApplicationViewManager.Name Application_DefaultApplicationViewName
        {
            get { return _application_DefaultApplicationViewName; }
            set
            {
                if (value == _application_DefaultApplicationViewName)
                    return;

                _application_DefaultApplicationViewName = value;
                SettingsChanged = true;
            }
        }

        private int _application_HistoryListEntries = 5;
        public int Application_HistoryListEntries
        {
            get { return _application_HistoryListEntries; }
            set
            {
                if (value == _application_HistoryListEntries)
                    return;

                _application_HistoryListEntries = value;
                SettingsChanged = true;
            }
        }

        // TrayIcon
        private bool _trayIcon_AlwaysShowIcon;
        public bool TrayIcon_AlwaysShowIcon
        {
            get { return _trayIcon_AlwaysShowIcon; }
            set
            {
                if (value == _trayIcon_AlwaysShowIcon)
                    return;

                _trayIcon_AlwaysShowIcon = value;
                SettingsChanged = true;
            }
        }

        // Appearance
        private string _appearance_AppTheme;
        public string Appearance_AppTheme
        {
            get { return _appearance_AppTheme; }
            set
            {
                if (value == _appearance_AppTheme)
                    return;

                _appearance_AppTheme = value;
                SettingsChanged = true;
            }
        }

        private string _appearance_Accent;
        public string Appearance_Accent
        {
            get { return _appearance_Accent; }
            set
            {
                if (value == _appearance_Accent)
                    return;

                _appearance_Accent = value;
                SettingsChanged = true;
            }
        }

        private bool _appearance_EnableTransparency;
        public bool Appearance_EnableTransparency
        {
            get { return _appearance_EnableTransparency; }
            set
            {
                if (value == _appearance_EnableTransparency)
                    return;

                _appearance_EnableTransparency = value;
                SettingsChanged = true;
            }
        }

        private double _appearance_Opacity = 0.85;
        public double Appearance_Opacity
        {
            get { return _appearance_Opacity; }
            set
            {
                if (value == _appearance_Opacity)
                    return;

                _appearance_Opacity = value;
                SettingsChanged = true;
            }
        }

        // Localization
        private string _localization_CultureCode;
        public string Localization_CultureCode
        {
            get { return _localization_CultureCode; }
            set
            {
                if (value == _localization_CultureCode)
                    return;

                _localization_CultureCode = value;
                SettingsChanged = true;
            }
        }

        // Autostart
        private bool _autostart_StartMinimizedInTray;
        public bool Autostart_StartMinimizedInTray
        {
            get { return _autostart_StartMinimizedInTray; }
            set
            {
                if (value == _autostart_StartMinimizedInTray)
                    return;

                _autostart_StartMinimizedInTray = value;
                SettingsChanged = true;
            }
        }

        // HotKey
        private bool _hotKey_ShowWindowEnabled;
        public bool HotKey_ShowWindowEnabled
        {
            get { return _hotKey_ShowWindowEnabled; }
            set
            {
                if (value == _hotKey_ShowWindowEnabled)
                    return;

                _hotKey_ShowWindowEnabled = value;
                SettingsChanged = true;
            }
        }

        private int _hotKey_ShowWindowKey = 79;
        public int HotKey_ShowWindowKey
        {
            get { return _hotKey_ShowWindowKey; }
            set
            {
                if (value == _hotKey_ShowWindowKey)
                    return;

                _hotKey_ShowWindowKey = value;
                SettingsChanged = true;
            }
        }

        private int _hotKey_ShowWindowModifier = 3;
        public int HotKey_ShowWindowModifier
        {
            get { return _hotKey_ShowWindowModifier; }
            set
            {
                if (value == _hotKey_ShowWindowModifier)
                    return;

                _hotKey_ShowWindowModifier = value;
                SettingsChanged = true;
            }
        }

        // Application view       
        private bool _applicationView_Expand;
        public bool ApplicationView_Expand
        {
            get { return _applicationView_Expand; }
            set
            {
                if (value == _applicationView_Expand)
                    return;

                _applicationView_Expand = value;
                SettingsChanged = true;
            }
        }

        #endregion

        #region IPScanner        
        private int _ipScanner_Attempts = 2;
        public int IPScanner_Attempts
        {
            get { return _ipScanner_Attempts; }
            set
            {
                if (value == _ipScanner_Attempts)
                    return;

                _ipScanner_Attempts = value;
                SettingsChanged = true;
            }
        }

        private int _ipScanner_Buffer = 32;
        public int IPScanner_Buffer
        {
            get { return _ipScanner_Buffer; }
            set
            {
                if (value == _ipScanner_Buffer)
                    return;

                _ipScanner_Buffer = value;
                SettingsChanged = true;
            }
        }

        private int _ipScanner_Threads = 256;
        public int IPScanner_Threads
        {
            get { return _ipScanner_Threads; }
            set
            {
                if (value == _ipScanner_Threads)
                    return;

                _ipScanner_Threads = value;
                SettingsChanged = true;
            }
        }

        private List<string> _ipScanner_IPRangeHistory = new List<string>();
        public List<string> IPScanner_IPRangeHistory
        {
            get { return _ipScanner_IPRangeHistory; }
            set
            {
                if (value == _ipScanner_IPRangeHistory)
                    return;

                _ipScanner_IPRangeHistory = value;
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ResolveHostname = true;
        public bool IPScanner_ResolveHostname
        {
            get { return _ipScanner_ResolveHostname; }
            set
            {
                if (value == _ipScanner_ResolveHostname)
                    return;

                _ipScanner_ResolveHostname = value;

                OnPropertyChanged();

                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ResolveMACAddress;
        public bool IPScanner_ResolveMACAddress
        {
            get { return _ipScanner_ResolveMACAddress; }
            set
            {
                if (value == _ipScanner_ResolveMACAddress)
                    return;

                _ipScanner_ResolveMACAddress = value;

                OnPropertyChanged();

                SettingsChanged = true;
            }
        }

        private int _ipScanner_Timeout = 4000;
        public int IPScanner_Timeout
        {
            get { return _ipScanner_Timeout; }
            set
            {
                if (value == _ipScanner_Timeout)
                    return;

                _ipScanner_Timeout = value;
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ExpandStatistics = true;
        public bool IPScanner_ExpandStatistics
        {
            get { return _ipScanner_ExpandStatistics; }
            set
            {
                if (value == _ipScanner_ExpandStatistics)
                    return;

                _ipScanner_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }

        private bool _ipScanner_ExpandProfileView = true;
        public bool IPScanner_ExpandProfileView
        {
            get { return _ipScanner_ExpandProfileView; }
            set
            {
                if (value == _ipScanner_ExpandProfileView)

                    return;
                _ipScanner_ExpandProfileView = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region NetworkInterface        
        private string _networkInterface_SelectedInterfaceId;
        public string NetworkInterface_SelectedInterfaceId
        {
            get { return _networkInterface_SelectedInterfaceId; }
            set
            {
                if (value == _networkInterface_SelectedInterfaceId)
                    return;

                _networkInterface_SelectedInterfaceId = value;
                SettingsChanged = true;
            }
        }

        private bool _networkInterface_ExpandProfileView = true;
        public bool NetworkInterface_ExpandProfileView
        {
            get { return _networkInterface_ExpandProfileView; }
            set
            {
                if (value == _networkInterface_ExpandProfileView)
                    return;

                _networkInterface_ExpandProfileView = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region Ping
        private int _ping_Attempts;
        public int Ping_Attempts
        {
            get { return _ping_Attempts; }
            set
            {
                if (value == _ping_Attempts)
                    return;

                _ping_Attempts = value;
                SettingsChanged = true;
            }
        }

        private int _ping_Buffer = 32;
        public int Ping_Buffer
        {
            get { return _ping_Buffer; }
            set
            {
                if (value == _ping_Buffer)
                    return;

                _ping_Buffer = value;
                SettingsChanged = true;
            }
        }

        private bool _ping_DontFragement = true;
        public bool Ping_DontFragment
        {
            get { return _ping_DontFragement; }
            set
            {
                if (value = _ping_DontFragement)
                    return;

                _ping_DontFragement = value;
                SettingsChanged = true;
            }
        }

        private List<string> _ping_HostnameOrIPAddressHistory = new List<string>();
        public List<string> Ping_HostnameOrIPAddressHistory
        {
            get { return _ping_HostnameOrIPAddressHistory; }
            set
            {
                if (value == _ping_HostnameOrIPAddressHistory)
                    return;

                _ping_HostnameOrIPAddressHistory = value;
                SettingsChanged = true;
            }
        }

        private bool _ping_ResolveHostnamePreferIPv4 = true;
        public bool Ping_ResolveHostnamePreferIPv4
        {
            get { return _ping_ResolveHostnamePreferIPv4; }
            set
            {
                if (value == _ping_ResolveHostnamePreferIPv4)
                    return;

                _ping_ResolveHostnamePreferIPv4 = value;
                SettingsChanged = true;
            }
        }

        private int _ping_Timeout = 4000;
        public int Ping_Timeout
        {
            get { return _ping_Timeout; }
            set
            {
                if (value == _ping_Timeout)
                    return;

                _ping_Timeout = value;
                SettingsChanged = true;
            }
        }

        private int _ping_TTL = 64;
        public int Ping_TTL
        {
            get { return _ping_TTL; }
            set
            {
                if (value == _ping_TTL)
                    return;

                _ping_TTL = value;
                SettingsChanged = true;
            }
        }

        private int _ping_WaitTime = 1000;
        public int Ping_WaitTime
        {
            get { return _ping_WaitTime; }
            set
            {
                if (value == _ping_WaitTime)
                    return;

                _ping_WaitTime = value;
                SettingsChanged = true;
            }
        }

        private int _ping_ExceptionCancelCount = 3;
        public int Ping_ExceptionCancelCount
        {
            get { return _ping_ExceptionCancelCount; }
            set
            {
                if (value == _ping_ExceptionCancelCount)
                    return;

                _ping_ExceptionCancelCount = value;
                SettingsChanged = true;
            }
        }

        private bool _ping_ExpandStatistics = true;
        public bool Ping_ExpandStatistics
        {
            get { return _ping_ExpandStatistics; }
            set
            {
                if (value == _ping_ExpandStatistics)
                    return;

                _ping_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region Traceroute
        private List<string> _traceroute_HostnameHistory = new List<string>();
        public List<string> Traceroute_HostnameHistory
        {
            get { return _traceroute_HostnameHistory; }
            set
            {
                if (value == _traceroute_HostnameHistory)
                    return;

                _traceroute_HostnameHistory = value;
                SettingsChanged = true;
            }
        }

        private int _traceroute_MaximumHops = 30;
        public int Traceroute_MaximumHops
        {
            get { return _traceroute_MaximumHops; }
            set
            {
                if (value == _traceroute_MaximumHops)
                    return;

                _traceroute_MaximumHops = value;
                SettingsChanged = true;
            }
        }

        private int _traceroute_Timeout = 4000;
        public int Traceroute_Timeout
        {
            get { return _traceroute_Timeout; }
            set
            {
                if (value == _traceroute_Timeout)
                    return;

                _traceroute_Timeout = value;
                SettingsChanged = true;
            }
        }

        private int _traceroute_Buffer = 32;
        public int Traceroute_Buffer
        {
            get { return _traceroute_Buffer; }
            set
            {
                if (value == _traceroute_Buffer)
                    return;

                _traceroute_Buffer = value;
                SettingsChanged = true;
            }
        }

        private bool _traceroute_ResolveHostname = true;
        public bool Traceroute_ResolveHostname
        {
            get { return _traceroute_ResolveHostname; }
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
            get { return _traceroute_ResolveHostnamePreferIPv4; }
            set
            {
                if (value == _traceroute_ResolveHostnamePreferIPv4)
                    return;

                _traceroute_ResolveHostnamePreferIPv4 = value;
                SettingsChanged = true;
            }
        }

        private bool _traceroute_ExpandStatistics;
        public bool Traceroute_ExpandStatistics
        {
            get { return _traceroute_ExpandStatistics; }
            set
            {
                if (value == _traceroute_ExpandStatistics)
                    return;

                _traceroute_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region Lookup
        private List<string> _lookup_OUI_MACAddressOrVendorHistory = new List<string>();
        public List<string> Lookup_OUI_MACAddressOrVendorHistory
        {
            get { return _lookup_OUI_MACAddressOrVendorHistory; }
            set
            {
                if (value == _lookup_OUI_MACAddressOrVendorHistory)
                    return;

                _lookup_OUI_MACAddressOrVendorHistory = value;
                SettingsChanged = true;
            }
        }

        private List<string> _lookup_Port_PortsHistory = new List<string>();
        public List<string> Lookup_Port_PortsHistory
        {
            get { return _lookup_Port_PortsHistory; }
            set
            {
                if (value == _lookup_Port_PortsHistory)
                    return;

                _lookup_Port_PortsHistory = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region PortScanner
        private List<string> _portScanner_HostnameHistory = new List<string>();
        public List<string> PortScanner_HostnameHistory
        {
            get { return _portScanner_HostnameHistory; }
            set
            {
                if (value == _portScanner_HostnameHistory)
                    return;

                _portScanner_HostnameHistory = value;
                SettingsChanged = true;
            }
        }

        private List<string> _portScanner_PortsHistory = new List<string>();
        public List<string> PortScanner_PortsHistory
        {
            get { return _portScanner_PortsHistory; }
            set
            {
                if (value == _portScanner_PortsHistory)
                    return;

                _portScanner_PortsHistory = value;
                SettingsChanged = true;
            }
        }

        private int _portScanner_Threads = 100;
        public int PortScanner_Threads
        {
            get { return _portScanner_Threads; }
            set
            {
                if (value == _portScanner_Threads)
                    return;

                _portScanner_Threads = value;
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ShowClosed;
        public bool PortScanner_ShowClosed
        {
            get { return _portScanner_ShowClosed; }
            set
            {
                if (value == _portScanner_ShowClosed)
                    return;

                _portScanner_ShowClosed = value;
                SettingsChanged = true;
            }
        }

        private int _portScanner_Timeout = 4000;
        public int PortScanner_Timeout
        {
            get { return _portScanner_Timeout; }
            set
            {
                if (value == _portScanner_Timeout)
                    return;

                _portScanner_Timeout = value;
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ResolveHostnamePreferIPv4 = true;
        public bool PortScanner_ResolveHostnamePreferIPv4
        {
            get { return _portScanner_ResolveHostnamePreferIPv4; }
            set
            {
                if (value == _portScanner_ResolveHostnamePreferIPv4)
                    return;

                _portScanner_ResolveHostnamePreferIPv4 = value;
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ExpandStatistics = true;
        public bool PortScanner_ExpandStatistics
        {
            get { return _portScanner_ExpandStatistics; }
            set
            {
                if (value == _portScanner_ExpandStatistics)
                    return;

                _portScanner_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }

        private bool _portScanner_ExpandProfileView = true;
        public bool PortScanner_ExpandProfileView
        {
            get { return _portScanner_ExpandProfileView; }
            set
            {
                if (value == _portScanner_ExpandProfileView)
                    return;

                _portScanner_ExpandProfileView = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region WakeOnLAN
        private int _wakeOnLAN_DefaultPort = 7;
        public int WakeOnLAN_DefaultPort
        {
            get { return _wakeOnLAN_DefaultPort; }
            set
            {
                if (value == _wakeOnLAN_DefaultPort)
                    return;

                _wakeOnLAN_DefaultPort = value;
                SettingsChanged = true;
            }
        }

        private bool _wakeOnLAN_ExpandClientView = true;
        public bool WakeOnLAN_ExpandClientView
        {
            get { return _wakeOnLAN_ExpandClientView; }
            set
            {
                if (value == _wakeOnLAN_ExpandClientView)
                    return;

                _wakeOnLAN_ExpandClientView = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region DNS Lookup
        private List<string> _dnsLookup_HostHistory = new List<string>();
        public List<string> DNSLookup_HostHistory
        {
            get { return _dnsLookup_HostHistory; }
            set
            {
                if (value == _dnsLookup_HostHistory)
                    return;

                _dnsLookup_HostHistory = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_UseCustomDNSServer;
        public bool DNSLookup_UseCustomDNSServer
        {
            get { return _dnsLookup_UseCustomDNSServer; }
            set
            {
                if (value == _dnsLookup_UseCustomDNSServer)
                    return;

                _dnsLookup_UseCustomDNSServer = value;
                SettingsChanged = true;
            }
        }

        private string _dnsLookup_CustomDNSServer;
        public string DNSLookup_CustomDNSServer
        {
            get { return _dnsLookup_CustomDNSServer; }
            set
            {
                if (value == _dnsLookup_CustomDNSServer)
                    return;

                _dnsLookup_CustomDNSServer = value;
                SettingsChanged = true;
            }
        }

        private QClass _dnsLookup_Class = QClass.IN;
        public QClass DNSLookup_Class
        {
            get { return _dnsLookup_Class; }
            set
            {
                if (value == _dnsLookup_Class)
                    return;

                _dnsLookup_Class = value;
                SettingsChanged = true;
            }
        }

        private QType _dnsLookup_Type = QType.ANY;
        public QType DNSLookup_Type
        {
            get { return _dnsLookup_Type; }
            set
            {
                if (value == _dnsLookup_Type)
                    return;

                _dnsLookup_Type = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_AddDNSSuffix = true;
        public bool DNSLookup_AddDNSSuffix
        {
            get { return _dnsLookup_AddDNSSuffix; }
            set
            {
                if (value == _dnsLookup_AddDNSSuffix)
                    return;

                _dnsLookup_AddDNSSuffix = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_UseCustomDNSSuffix;
        public bool DNSLookup_UseCustomDNSSuffix
        {
            get { return _dnsLookup_UseCustomDNSSuffix; }
            set
            {
                if (value == _dnsLookup_UseCustomDNSSuffix)
                    return;

                _dnsLookup_UseCustomDNSSuffix = value;
                SettingsChanged = true;
            }
        }

        private string _dnsLookup_CustomDNSSuffix;
        public string DNSLookup_CustomDNSSuffix
        {
            get { return _dnsLookup_CustomDNSSuffix; }
            set
            {
                if (value == _dnsLookup_CustomDNSSuffix)
                    return;

                _dnsLookup_CustomDNSSuffix = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_ResolveCNAME = true;
        public bool DNSLookup_ResolveCNAME
        {
            get { return _dnsLookup_ResolveCNAME; }
            set
            {
                if (value == _dnsLookup_ResolveCNAME)
                    return;

                _dnsLookup_ResolveCNAME = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_Recursion = true;
        public bool DNSLookup_Recursion
        {
            get { return _dnsLookup_Recursion; }
            set
            {
                if (value == _dnsLookup_Recursion)
                    return;

                _dnsLookup_Recursion = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_UseResolverCache = false;
        public bool DNSLookup_UseResolverCache
        {
            get { return _dnsLookup_UseResolverCache; }
            set
            {
                if (value == _dnsLookup_UseResolverCache)
                    return;

                _dnsLookup_UseResolverCache = value;
                SettingsChanged = true;
            }
        }

        private TransportType _dnsLookup_TransportType = TransportType.Udp;
        public TransportType DNSLookup_TransportType
        {
            get { return _dnsLookup_TransportType; }
            set
            {
                if (value == _dnsLookup_TransportType)
                    return;

                _dnsLookup_TransportType = value;
                SettingsChanged = true;
            }
        }

        private int _dnsLookup_Attempts = 3;
        public int DNSLookup_Attempts
        {
            get { return _dnsLookup_Attempts; }
            set
            {
                if (value == _dnsLookup_Attempts)
                    return;

                _dnsLookup_Attempts = value;
                SettingsChanged = true;
            }
        }

        private int _dnsLookup_Timeout = 2000;
        public int DNSLookup_Timeout
        {
            get { return _dnsLookup_Timeout; }
            set
            {
                if (value == _dnsLookup_Timeout)
                    return;

                _dnsLookup_Timeout = value;
                SettingsChanged = true;
            }
        }

        private bool _dnsLookup_ExpandStatistics = false;
        public bool DNSLookup_ExpandStatistics
        {
            get { return _dnsLookup_ExpandStatistics; }
            set
            {
                if (value == _dnsLookup_ExpandStatistics)
                    return;

                _dnsLookup_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region Subnet Calculator
        #region IPv4 Calculator
        private List<string> _subnetCalculator_IPv4Calculator_SubnetHistory = new List<string>();
        public List<string> SubnetCalculator_IPv4Calculator_SubnetHistory
        {
            get { return _subnetCalculator_IPv4Calculator_SubnetHistory; }
            set
            {
                if (value == _subnetCalculator_IPv4Calculator_SubnetHistory)
                    return;

                _subnetCalculator_IPv4Calculator_SubnetHistory = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region IPv4 Splitter
        private List<string> _subnetCalculator_IPv4Splitter_SubnetHistory = new List<string>();
        public List<string> SubnetCalculator_IPv4Splitter_SubnetHistory
        {
            get { return _subnetCalculator_IPv4Splitter_SubnetHistory; }
            set
            {
                if (value == _subnetCalculator_IPv4Splitter_SubnetHistory)
                    return;

                _subnetCalculator_IPv4Splitter_SubnetHistory = value;
                SettingsChanged = true;
            }
        }

        private List<string> _subnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory = new List<string>();
        public List<string> SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory
        {
            get { return _subnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory; }
            set
            {
                if (value == _subnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory)
                    return;

                _subnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory = value;
                SettingsChanged = true;
            }
        }
        #endregion
        #endregion

        #region RemoteDesktop  
        private bool _remoteDesktop_AdjustScreenAutomatically;
        public bool RemoteDesktop_AdjustScreenAutomatically
        {
            get { return _remoteDesktop_AdjustScreenAutomatically; }
            set
            {
                if (value == _remoteDesktop_AdjustScreenAutomatically)
                    return;

                _remoteDesktop_AdjustScreenAutomatically = value;
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_DesktopWidth = 1280;
        public int RemoteDesktop_DesktopWidth
        {
            get { return _remoteDesktop_DesktopWidth; }
            set
            {
                if (value == _remoteDesktop_DesktopWidth)
                    return;

                _remoteDesktop_DesktopWidth = value;
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_DesktopHeight = 768;
        public int RemoteDesktop_DesktopHeight
        {
            get { return _remoteDesktop_DesktopHeight; }
            set
            {
                if (value == _remoteDesktop_DesktopHeight)
                    return;

                _remoteDesktop_DesktopHeight = value;
                SettingsChanged = true;
            }
        }

        private int _remoteDesktop_ColorDepth = 32;
        public int RemoteDesktop_ColorDepth
        {
            get { return _remoteDesktop_ColorDepth; }
            set
            {
                if (value == _remoteDesktop_ColorDepth)
                    return;

                _remoteDesktop_ColorDepth = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_EnableCredSspSupport = true;
        public bool RemoteDesktop_EnableCredSspSupport
        {
            get { return _remoteDesktop_EnableCredSspSupport; }
            set
            {
                if (value == _remoteDesktop_EnableCredSspSupport)
                    return;

                _remoteDesktop_EnableCredSspSupport = value;
                SettingsChanged = true;
            }
        }

        private uint _remoteDesktop_AuthenticationLevel = 2;
        public uint RemoteDesktop_AuthenticationLevel
        {
            get { return _remoteDesktop_AuthenticationLevel; }
            set
            {
                if (value == _remoteDesktop_AuthenticationLevel)
                    return;

                _remoteDesktop_AuthenticationLevel = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectClipboard = true;
        public bool RemoteDesktop_RedirectClipboard
        {
            get { return _remoteDesktop_RedirectClipboard; }
            set
            {
                if (value == _remoteDesktop_RedirectClipboard)
                    return;

                _remoteDesktop_RedirectClipboard = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectDevices;
        public bool RemoteDesktop_RedirectDevices
        {
            get { return _remoteDesktop_RedirectDevices; }
            set
            {
                if (value == _remoteDesktop_RedirectDevices)
                    return;

                _remoteDesktop_RedirectDevices = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectDrives;
        public bool RemoteDesktop_RedirectDrives
        {
            get { return _remoteDesktop_RedirectDrives; }
            set
            {
                if (value == _remoteDesktop_RedirectDrives)
                    return;

                _remoteDesktop_RedirectDrives = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectPorts;
        public bool RemoteDesktop_RedirectPorts
        {
            get { return _remoteDesktop_RedirectPorts; }
            set
            {
                if (value == _remoteDesktop_RedirectPorts)
                    return;

                _remoteDesktop_RedirectPorts = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectSmartCards;
        public bool RemoteDesktop_RedirectSmartCards
        {
            get { return _remoteDesktop_RedirectSmartCards; }
            set
            {
                if (value == _remoteDesktop_RedirectSmartCards)
                    return;

                _remoteDesktop_RedirectSmartCards = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_RedirectPrinters;
        public bool RemoteDesktop_RedirectPrinters
        {
            get { return _remoteDesktop_RedirectPrinters; }
            set
            {
                if (value == _remoteDesktop_RedirectPrinters)
                    return;

                _remoteDesktop_RedirectPrinters = value;
                SettingsChanged = true;
            }
        }

        private bool _remoteDesktop_ExpandSessionView = true;
        public bool RemoteDesktop_ExpandSessionView
        {
            get { return _remoteDesktop_ExpandSessionView; }
            set
            {
                if (value == _remoteDesktop_ExpandSessionView)
                    return;

                _remoteDesktop_ExpandSessionView = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region HTTP Headers
        private List<string> _httpHeader_WebsiteUriHistory = new List<string>();
        public List<string> HTTPHeader_WebsiteUriHistory
        {
            get { return _httpHeader_WebsiteUriHistory; }
            set
            {
                if (value == _httpHeader_WebsiteUriHistory)
                    return;

                _httpHeader_WebsiteUriHistory = value;
                SettingsChanged = true;
            }
        }

        private bool _httpHeader_ExpandStatistics = true;
        public bool HTTPHeader_ExpandStatistics
        {
            get { return _httpHeader_ExpandStatistics; }
            set
            {
                if (value == _httpHeader_ExpandStatistics)
                    return;

                _httpHeader_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }
        #endregion

        #region SNMP
               private WalkMode _snmp_WalkMode = WalkMode.WithinSubtree;
        public WalkMode SNMP_WalkMode
        {
            get { return _snmp_WalkMode; }
            set
            {
                if (value == _snmp_WalkMode)
                    return;

                _snmp_WalkMode = value;
                SettingsChanged = true;
            }
        }

        private int _snmp_Timeout = 60000;
        public int SNMP_Timeout
        {
            get { return _snmp_Timeout; }
            set
            {
                if (value == _snmp_Timeout)
                    return;

                _snmp_Timeout = value;
                SettingsChanged = true;
            }
        }

        private int _snmp_port = 161;
        public int SNMP_Port
        {
            get
            { return _snmp_port; }
            set
            {
                if (value == _snmp_port)
                    return;

                _snmp_port = value;
                SettingsChanged = true;
            }
        }

        private bool _snmp_ResolveHostnamePreferIPv4 = true;
        public bool SNMP_ResolveHostnamePreferIPv4
        {
            get { return _snmp_ResolveHostnamePreferIPv4; }
            set
            {
                if (value == _snmp_ResolveHostnamePreferIPv4)
                    return;

                _snmp_ResolveHostnamePreferIPv4 = value;
                SettingsChanged = true;
            }
        }

        private List<string> _snmp_v1v2c_HostnameHistory = new List<string>();
        public List<string> SNMP_v1v2c_HostnameHistory
        {
            get { return _snmp_v1v2c_HostnameHistory; }
            set
            {
                if (value == _snmp_v1v2c_HostnameHistory)
                    return;

                _snmp_v1v2c_HostnameHistory = value;
                SettingsChanged = true;
            }
        }

        private List<string> _snmp_v1v2c_OIDHistory = new List<string>();
        public List<string> SNMP_v1v2c_OIDHistory
        {
            get { return _snmp_v1v2c_OIDHistory; }
            set
            {
                if (value == _snmp_v1v2c_OIDHistory)
                    return;

                _snmp_v1v2c_OIDHistory = value;
                SettingsChanged = true;
            }
        }

        private bool _snmp_v1v2c_Walk = true;
        public bool SNMP_v1v2c_Walk
        {
            get { return _snmp_v1v2c_Walk; }
            set
            {
                if (value == _snmp_v1v2c_Walk)
                    return;

                _snmp_v1v2c_Walk = value;
                SettingsChanged = true;
            }
        }

        private SNMPVersion _snmp_v1v2c_Version = SNMPVersion.v2c;
        public SNMPVersion SNMP_v1v2c_Version
        {
            get { return _snmp_v1v2c_Version; }
            set
            {
                if (value == _snmp_v1v2c_Version)
                    return;

                _snmp_v1v2c_Version = value;
                SettingsChanged = true;
            }
        }

        private bool _snmp_v1v2c_ExpandStatistics = true;
        public bool SNMP_v1v2c_ExpandStatistics
        {
            get { return _snmp_v1v2c_ExpandStatistics; }
            set
            {
                if (value == _snmp_v1v2c_ExpandStatistics)
                    return;

                _snmp_v1v2c_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }

        private List<string> _snmp_v3_HostnameHistory = new List<string>();
        public List<string> SNMP_v3_HostnameHistory
        {
            get { return _snmp_v3_HostnameHistory; }
            set
            {
                if (value == _snmp_v3_HostnameHistory)
                    return;

                _snmp_v3_HostnameHistory = value;
                SettingsChanged = true;
            }
        }

        private List<string> _snmp_v3_OIDHistory = new List<string>();
        public List<string> SNMP_v3_OIDHistory
        {
            get { return _snmp_v3_OIDHistory; }
            set
            {
                if (value == _snmp_v3_OIDHistory)
                    return;

                _snmp_v3_OIDHistory = value;
                SettingsChanged = true;
            }
        }

        private bool _snmp_v3_Walk = true;
        public bool SNMP_v3_Walk
        {
            get { return _snmp_v3_Walk; }
            set
            {
                if (value == _snmp_v3_Walk)
                    return;

                _snmp_v3_Walk = value;
                SettingsChanged = true;
            }
        }

        private SNMPv3Security _snmp_v3_Security = SNMPv3Security.AuthPriv;
        public SNMPv3Security SNMP_v3_Security
        {
            get { return _snmp_v3_Security; }
            set
            {
                if (value == _snmp_v3_Security)
                    return;

                _snmp_v3_Security = value;
                SettingsChanged = true;
            }
        }

        private SNMPv3AuthenticationProvider _snmp_v3_AuthenticationProvider = SNMPv3AuthenticationProvider.SHA1;
        public SNMPv3AuthenticationProvider SNMP_v3_AuthenticationProvider
        {
            get { return _snmp_v3_AuthenticationProvider; }
            set
            {
                if (value == _snmp_v3_AuthenticationProvider)
                    return;

                _snmp_v3_AuthenticationProvider = value;
                SettingsChanged = true;
            }
        }

        private SNMPv3PrivacyProvider _snmp_v3_PrivacyProvider = SNMPv3PrivacyProvider.AES;
        public SNMPv3PrivacyProvider SNMP_v3_PrivacyProvider
        {
            get { return _snmp_v3_PrivacyProvider; }
            set
            {
                if (value == _snmp_v3_PrivacyProvider)
                    return;

                _snmp_v3_PrivacyProvider = value;
                SettingsChanged = true;
            }
        }

        private bool _snmp_v3_ExpandStatistics = true;
        public bool SNMP_v3_ExpandStatistics
        {
            get { return _snmp_v3_ExpandStatistics; }
            set
            {
                if (value == _snmp_v3_ExpandStatistics)
                    return;

                _snmp_v3_ExpandStatistics = value;
                SettingsChanged = true;
            }
        }
        #endregion
        #endregion

        #region Constructor
        public SettingsInfo()
        {

        }
        #endregion
    }
}
