using NETworkManager.Views;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public class SettingsInfo
    {
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

                SettingsChanged = true;

                _window_ConfirmClose = value;
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

                SettingsChanged = true;

                _window_MinimizeInsteadOfTerminating = value;
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

                SettingsChanged = true;

                _window_MinimizeToTrayInsteadOfTaskbar = value;
            }
        }

        private bool _window_StartMaximized;
        public bool Window_StartMaximized
        {
            get { return _window_StartMaximized; }
            set
            {
                if (value == _window_StartMaximized)
                    return;

                SettingsChanged = true;

                _window_StartMaximized = value;
            }
        }

        // Applications        
        private ApplicationView.Name _application_DefaultApplicationViewName = ApplicationView.Name.NetworkInterface;
        public ApplicationView.Name Application_DefaultApplicationViewName
        {
            get { return _application_DefaultApplicationViewName; }
            set
            {
                if (value == _application_DefaultApplicationViewName)
                    return;

                SettingsChanged = true;

                _application_DefaultApplicationViewName = value;
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

                SettingsChanged = true;
                _application_HistoryListEntries = value;
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

                SettingsChanged = true;

                _trayIcon_AlwaysShowIcon = value;
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

                SettingsChanged = true;

                _appearance_AppTheme = value;
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

                SettingsChanged = true;

                _appearance_Accent = value;
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

                SettingsChanged = true;

                _localization_CultureCode = value;
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

                SettingsChanged = true;

                _autostart_StartMinimizedInTray = value;
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

                SettingsChanged = true;

                _hotKey_ShowWindowEnabled = value;
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

                SettingsChanged = true;

                _hotKey_ShowWindowKey = value;
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

                SettingsChanged = true;

                _hotKey_ShowWindowModifier = value;
            }
        }

        // Developer
        private bool _developerMode;
        public bool DeveloperMode
        {
            get { return _developerMode; }
            set
            {
                if (value == _developerMode)
                    return;

                SettingsChanged = true;

                _developerMode = value;
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

                SettingsChanged = true;

                _applicationView_Expand = value;
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

                SettingsChanged = true;

                _ipScanner_Attempts = value;
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

                SettingsChanged = true;

                _ipScanner_Buffer = value;
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

                SettingsChanged = true;

                _ipScanner_Threads = value;
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

                SettingsChanged = true;

                _ipScanner_IPRangeHistory = value;
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

                SettingsChanged = true;

                _ipScanner_ResolveHostname = value;
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

                SettingsChanged = true;

                _ipScanner_ResolveMACAddress = value;
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

                SettingsChanged = true;

                _ipScanner_Timeout = value;
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

                SettingsChanged = true;

                _networkInterface_SelectedInterfaceId = value;
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

                SettingsChanged = true;

                _ping_Attempts = value;
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

                SettingsChanged = true;

                _ping_Buffer = value;
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

                SettingsChanged = true;

                _ping_DontFragement = value;
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

                SettingsChanged = true;

                _ping_HostnameOrIPAddressHistory = value;
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

                SettingsChanged = true;

                _ping_ResolveHostnamePreferIPv4 = value;
            }
        }

        public int _ping_Timeout = 4000;
        public int Ping_Timeout
        {
            get { return _ping_Timeout; }
            set
            {
                if (value == _ping_Timeout)
                    return;

                SettingsChanged = true;

                _ping_Timeout = value;
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

                SettingsChanged = true;

                _ping_TTL = value;
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

                SettingsChanged = true;

                _ping_WaitTime = value;
            }
        }
        #endregion

        #region Traceroute
        private int _traceroute_Buffer = 32;
        public int Traceroute_Buffer
        {
            get { return _traceroute_Buffer; }
            set
            {
                if (value == _traceroute_Buffer)
                    return;

                SettingsChanged = true;

                _traceroute_Buffer = value;
            }
        }

        private List<string> _traceroute_HostnameOrIPAddressHistory = new List<string>();
        public List<string> Traceroute_HostnameOrIPAddressHistory
        {
            get { return _traceroute_HostnameOrIPAddressHistory; }
            set
            {
                if (value == _traceroute_HostnameOrIPAddressHistory)
                    return;

                SettingsChanged = true;

                _traceroute_HostnameOrIPAddressHistory = value;
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

                SettingsChanged = true;

                _traceroute_MaximumHops = value;
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

                SettingsChanged = true;

                _traceroute_ResolveHostnamePreferIPv4 = value;
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

                SettingsChanged = true;

                _traceroute_Timeout = value;
            }
        }
        #endregion

        #region Lookup
        private List<string> _lookup_MACAddressHistory = new List<string>();
        public List<string> Lookup_MACAddressHistory
        {
            get { return _lookup_MACAddressHistory; }
            set
            {
                if (value == _lookup_MACAddressHistory)
                    return;

                SettingsChanged = true;

                _lookup_MACAddressHistory = value;
            }
        }

        private List<string> _lookup_PortsHistory = new List<string>();
        public List<string> Lookup_PortsHistory
        {
            get { return _lookup_PortsHistory; }
            set
            {
                if (value == _lookup_PortsHistory)
                    return;

                SettingsChanged = true;

                _lookup_PortsHistory = value;
            }
        }
        #endregion

        #region PortScanner
        private List<string> _portScanner_HostnameOrIPAddressHistory = new List<string>();
        public List<string> PortScanner_HostnameOrIPAddressHistory
        {
            get { return _portScanner_HostnameOrIPAddressHistory; }
            set
            {
                if (value == _portScanner_HostnameOrIPAddressHistory)
                    return;

                SettingsChanged = true;

                _portScanner_HostnameOrIPAddressHistory = value;
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

                SettingsChanged = true;

                _portScanner_PortsHistory = value;
            }
        }

        private int _portScanner_Threads = 50;
        public int PortScanner_Threads
        {
            get { return _portScanner_Threads; }
            set
            {
                if (value == _portScanner_Threads)
                    return;

                SettingsChanged = true;

                _portScanner_Threads = value;
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

                SettingsChanged = true;

                _portScanner_ShowClosed = value;
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

                SettingsChanged = true;

                _portScanner_Timeout = value;
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

                SettingsChanged = true;

                _portScanner_ResolveHostnamePreferIPv4 = value;
            }
        }
        #endregion

        public SettingsInfo()
        {

        }
    }
}
