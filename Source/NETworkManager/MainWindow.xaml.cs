using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Views;
using NETworkManager.Models.Settings;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;
using NETworkManager.Utilities;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using NETworkManager.Controls;
using NETworkManager.Models.Update;
using NETworkManager.Models.Documentation;
using NETworkManager.ViewModels;
using NETworkManager.Models.EventSystem;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace NETworkManager
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables        
        private NotifyIcon _notifyIcon;
        private StatusWindow statusWindow;

        private readonly bool _isLoading;
        private bool isApplicationListLoading;

        private bool _isInTray;
        private bool _closeApplication;

        // Indicates a restart message, when settings changed
        private string _cultureCode;

        private bool _expandApplicationView;
        public bool ExpandApplicationView
        {
            get => _expandApplicationView;
            set
            {
                if (value == _expandApplicationView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.ExpandApplicationView = value;

                if (!value)
                    ClearSearchOnApplicationListMinimize();

                _expandApplicationView = value;
                OnPropertyChanged();
            }
        }

        private bool _isTextBoxSearchFocused;
        public bool IsTextBoxSearchFocused
        {
            get => _isTextBoxSearchFocused;
            set
            {
                if (value == _isTextBoxSearchFocused)
                    return;

                if (!value)
                    ClearSearchOnApplicationListMinimize();

                _isTextBoxSearchFocused = value;
                OnPropertyChanged();
            }
        }

        private bool _isApplicationListOpen;
        public bool IsApplicationListOpen
        {
            get => _isApplicationListOpen;
            set
            {
                if (value == _isApplicationListOpen)
                    return;

                if (!value)
                    ClearSearchOnApplicationListMinimize();

                _isApplicationListOpen = value;
                OnPropertyChanged();
            }
        }

        private bool _isMouseOverApplicationList;
        public bool IsMouseOverApplicationList
        {
            get => _isMouseOverApplicationList;
            set
            {
                if (value == _isMouseOverApplicationList)
                    return;

                if (!value)
                    ClearSearchOnApplicationListMinimize();

                _isMouseOverApplicationList = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _applications;
        public ICollectionView Applications
        {
            get => _applications;
            set
            {
                if (value == _applications)
                    return;

                _applications = value;
                OnPropertyChanged();
            }
        }

        private ApplicationViewInfo _selectedApplication;
        public ApplicationViewInfo SelectedApplication
        {
            get => _selectedApplication;
            set
            {
                if (isApplicationListLoading)
                    return;

                if (Equals(value, _selectedApplication))
                    return;

                if (value != null)
                    ChangeApplicationView(value.Name);

                _selectedApplication = value;
                OnPropertyChanged();
            }
        }

        private ApplicationViewManager.Name _filterLastViewName;
        private int? _filterLastCount;

        private string _search = string.Empty;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                if (SelectedApplication != null)
                    _filterLastViewName = SelectedApplication.Name;

                Applications.Refresh();

                var sourceCollection = Applications.SourceCollection.Cast<ApplicationViewInfo>();
                var filteredCollection = Applications.Cast<ApplicationViewInfo>();

                var sourceInfos = sourceCollection as ApplicationViewInfo[] ?? sourceCollection.ToArray();
                var filteredInfos = filteredCollection as ApplicationViewInfo[] ?? filteredCollection.ToArray();

                if (_filterLastCount == null)
                    _filterLastCount = sourceInfos.Length;

                SelectedApplication = _filterLastCount > filteredInfos.Length ? filteredInfos.FirstOrDefault() : sourceInfos.FirstOrDefault(x => x.Name == _filterLastViewName);

                _filterLastCount = filteredInfos.Length;

                // Show note when there was nothing found
                SearchNothingFound = filteredInfos.Length == 0;

                OnPropertyChanged();
            }
        }

        private bool _searchNothingFound;
        public bool SearchNothingFound
        {
            get => _searchNothingFound;
            set
            {
                if (value == _searchNothingFound)
                    return;

                _searchNothingFound = value;
                OnPropertyChanged();
            }
        }

        private SettingsView _settingsView;

        private bool _showSettingsView;
        public bool ShowSettingsView
        {
            get => _showSettingsView;
            set
            {
                if (value == _showSettingsView)
                    return;

                _showSettingsView = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                if (value == _isUpdateAvailable)
                    return;

                _isUpdateAvailable = value;
                OnPropertyChanged();
            }
        }

        private string _updateText;
        public string UpdateText
        {
            get => _updateText;
            set
            {
                if (value == _updateText)
                    return;

                _updateText = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, window load and close events
        public MainWindow()
        {
            _isLoading = true;

            InitializeComponent();
            DataContext = this;

            // Language Meta
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(LocalizationManager.Culture.IetfLanguageTag)));

            // Load appearance
            AppearanceManager.Load();

            // Transparency
            if (SettingsManager.Current.Appearance_EnableTransparency)
            {
                AllowsTransparency = true;
                Opacity = SettingsManager.Current.Appearance_Opacity;

                ConfigurationManager.Current.IsTransparencyEnabled = true;
            }

            // NotifyIcon for Autostart
            if (CommandLineManager.Current.Autostart && SettingsManager.Current.Autostart_StartMinimizedInTray || SettingsManager.Current.TrayIcon_AlwaysShowIcon)
                InitNotifyIcon();

            // Set windows title if admin
            if (ConfigurationManager.Current.IsAdmin)
                Title = $"[{NETworkManager.Resources.Localization.Strings.Administrator}] {Title}";

            // Load Profiles
            ProfileManager.Load();

            // Load settings
            ExpandApplicationView = SettingsManager.Current.ExpandApplicationView;

            // Check if settings have changed
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            // Register event system...
            EventSystem.RedirectProfileToApplicationEvent += EventSystem_RedirectProfileToApplicationEvent;
            EventSystem.RedirectDataToApplicationEvent += EventSystem_RedirectDataToApplicationEvent;
            EventSystem.RedirectToSettingsEvent += EventSystem_RedirectToSettingsEvent;

            _isLoading = false;
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Show settings reset note...
            if (ConfigurationManager.Current.ShowSettingsResetNoteOnStartup)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = NETworkManager.Resources.Localization.Strings.OK;

                ConfigurationManager.Current.FixAirspace = true;

                await this.ShowMessageAsync(NETworkManager.Resources.Localization.Strings.SettingsHaveBeenReset, NETworkManager.Resources.Localization.Strings.SettingsFileFoundWasCorruptOrNotCompatibleMessage, MessageDialogStyle.Affirmative, settings);

                ConfigurationManager.Current.FixAirspace = false;
            }

            if (SettingsManager.Current.FirstRun)
            {
                // Show first run dialog...
                var customDialog = new CustomDialog
                {
                    Title = NETworkManager.Resources.Localization.Strings.Welcome
                };

                var arpTableAddEntryViewModel = new FirstRunViewModel(async instance =>
                {
                    await this.HideMetroDialogAsync(customDialog);

                    SettingsManager.Current.FirstRun = false;
                    SettingsManager.Current.Update_CheckForUpdatesAtStartup = instance.CheckForUpdatesAtStartup;
                    SettingsManager.Current.Dashboard_CheckPublicIPAddress = instance.CheckPublicIPAddress;

                    AfterContentRendered();
                });

                customDialog.Content = new FirstRunDialog
                {
                    DataContext = arpTableAddEntryViewModel
                };

                await this.ShowMetroDialogAsync(customDialog).ConfigureAwait(true);
            }
            else
            {
                AfterContentRendered();
            }
        }

        private void AfterContentRendered()
        {
            // Load application list, filter, sort, etc.
            LoadApplicationList();

            // Hide to tray after the window shows up... not nice, but otherwise the hotkeys do not work
            if (CommandLineManager.Current.Autostart && SettingsManager.Current.Autostart_StartMinimizedInTray)
                HideWindowToTray();

            // Init status window
            statusWindow = new StatusWindow(this);

            // Search for updates... 
            if (SettingsManager.Current.Update_CheckForUpdatesAtStartup)
                CheckForUpdates();
        }

        private void LoadApplicationList()
        {
            isApplicationListLoading = true;

            // Create a new list if empty
            if (SettingsManager.Current.General_ApplicationList.Count == 0)
            {
                SettingsManager.Current.General_ApplicationList = new ObservableSetCollection<ApplicationViewInfo>(ApplicationViewManager.GetList());
            }
            else // Check for missing applications and add them
            {
                foreach (ApplicationViewInfo info in ApplicationViewManager.GetList())
                {
                    bool isInList = false;

                    foreach (ApplicationViewInfo info2 in SettingsManager.Current.General_ApplicationList)
                    {
                        if (info.Name == info2.Name)
                            isInList = true;
                    }

                    if (!isInList)
                        SettingsManager.Current.General_ApplicationList.Add(info);
                }
            }

            Applications = new CollectionViewSource { Source = SettingsManager.Current.General_ApplicationList }.View;

            Applications.SortDescriptions.Add(new SortDescription(nameof(ApplicationViewInfo.Name), ListSortDirection.Ascending)); // Always have the same order, even if it is translated...
            Applications.Filter = o =>
            {
                if (!(o is ApplicationViewInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.IsVisible;

                var regex = new Regex(@" |-");

                var search = regex.Replace(Search, "");

                    // Search by TranslatedName and Name
                    return info.IsVisible && (regex.Replace(ApplicationViewManager.GetTranslatedNameByName(info.Name), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || regex.Replace(info.Name.ToString(), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            };

            SettingsManager.Current.General_ApplicationList.CollectionChanged += (sender, args) => Applications.Refresh();

            isApplicationListLoading = false;

            // Select the application
            SelectedApplication = Applications.SourceCollection.Cast<ApplicationViewInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.General_DefaultApplicationViewName);

            // Scroll into view
            if (SelectedApplication != null)
                ListViewApplication.ScrollIntoView(SelectedApplication);
        }

        private async void MetroWindowMain_Closing(object sender, CancelEventArgs e)
        {
            // Force restart (if user has reset the settings or import them)
            if (SettingsManager.ForceRestart || ImportExportManager.ForceRestart)
            {
                RestartApplication(false);

                _closeApplication = true;
            }

            // Hide the application to tray
            if (!_closeApplication && (SettingsManager.Current.Window_MinimizeInsteadOfTerminating && WindowState != WindowState.Minimized))
            {
                e.Cancel = true;

                WindowState = WindowState.Minimized;

                return;
            }

            // Confirm close
            if (!_closeApplication && SettingsManager.Current.Window_ConfirmClose)
            {
                e.Cancel = true;

                // If the window is minimized, bring it to front
                if (WindowState == WindowState.Minimized)
                    BringWindowToFront();

                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = NETworkManager.Resources.Localization.Strings.Close;
                settings.NegativeButtonText = NETworkManager.Resources.Localization.Strings.Cancel;
                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                // Fix airspace issues
                ConfigurationManager.Current.FixAirspace = true;

                var result = await this.ShowMessageAsync(NETworkManager.Resources.Localization.Strings.Confirm, NETworkManager.Resources.Localization.Strings.ConfirmCloseMessage, MessageDialogStyle.AffirmativeAndNegative, settings);

                ConfigurationManager.Current.FixAirspace = false;

                if (result != MessageDialogResult.Affirmative)
                    return;

                _closeApplication = true;
                Close();

                return;
            }

            // Unregister HotKeys
            if (_registeredHotKeys.Count > 0)
                UnregisterHotKeys();

            // Dispose the notify icon to prevent errors
            _notifyIcon?.Dispose();
        }
        #endregion

        #region Application Views
        private DashboardView _overviewView;
        private NetworkInterfaceView _networkInterfaceView;
        private WiFiView _wiFiView;
        private IPScannerHostView _ipScannerHostView;
        private PortScannerHostView _portScannerHostView;
        private PingHostView _pingHostView;
        private PingMonitorView _pingMonitorView;
        private TracerouteHostView _tracerouteHostView;
        private DNSLookupHostView _dnsLookupHostView;
        private RemoteDesktopHostView _remoteDesktopHostView;
        private PowerShellHostView _powerShellHostView;
        private PuTTYHostView _puttyHostView;
        private TigerVNCHostView _tigerVNCHostView;
        private SNMPHostView _snmpHostView;
        private WakeOnLANView _wakeOnLanView;
        private SubnetCalculatorHostView _subnetCalculatorHostView;
        private HTTPHeadersHostView _httpHeadersHostView;
        private LookupHostView _lookupHostView;
        private WhoisHostView _whoisHostView;
        private ConnectionsView _connectionsView;
        private ListenersView _listenersView;
        private ARPTableView _arpTableView;

        private ApplicationViewManager.Name? _currentApplicationViewName;

        private void ChangeApplicationView(ApplicationViewManager.Name name, bool refresh = false)
        {
            if (!refresh && _currentApplicationViewName == name)
                return;

            // Stop some functions on the old view
            switch (_currentApplicationViewName)
            {
                case ApplicationViewManager.Name.NetworkInterface:
                    _networkInterfaceView?.OnViewHide();
                    break;
                case ApplicationViewManager.Name.WiFi:
                    _wiFiView?.OnViewHide();
                    break;
                case ApplicationViewManager.Name.Connections:
                    _connectionsView?.OnViewHide();
                    break;
                case ApplicationViewManager.Name.Listeners:
                    _listenersView?.OnViewHide();
                    break;
                case ApplicationViewManager.Name.ARPTable:
                    _arpTableView?.OnViewHide();
                    break;
            }

            // Create new view / start some functions
            switch (name)
            {
                case ApplicationViewManager.Name.Dashboard:
                    if (_overviewView == null)
                        _overviewView = new DashboardView();
                    else
                        _overviewView.OnViewVisible();

                    ContentControlApplication.Content = _overviewView;
                    break;
                case ApplicationViewManager.Name.NetworkInterface:
                    if (_networkInterfaceView == null)
                        _networkInterfaceView = new NetworkInterfaceView();
                    else
                        _networkInterfaceView.OnViewVisible();

                    ContentControlApplication.Content = _networkInterfaceView;
                    break;
                case ApplicationViewManager.Name.WiFi:
                    if (_wiFiView == null)
                        _wiFiView = new WiFiView();
                    else
                        _wiFiView.OnViewVisible();

                    ContentControlApplication.Content = _wiFiView;
                    break;
                case ApplicationViewManager.Name.IPScanner:
                    if (_ipScannerHostView == null)
                        _ipScannerHostView = new IPScannerHostView();
                    else
                        _ipScannerHostView.OnViewVisible();

                    ContentControlApplication.Content = _ipScannerHostView;
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    if (_portScannerHostView == null)
                        _portScannerHostView = new PortScannerHostView();
                    else
                        _portScannerHostView.OnViewVisible();

                    ContentControlApplication.Content = _portScannerHostView;
                    break;
                case ApplicationViewManager.Name.Ping:
                    if (_pingHostView == null)
                        _pingHostView = new PingHostView();
                    else
                        _pingHostView.OnViewVisible();

                    ContentControlApplication.Content = _pingHostView;
                    break;
                case ApplicationViewManager.Name.PingMonitor:
                    if (_pingMonitorView == null)
                        _pingMonitorView = new PingMonitorView();
                    else
                        _pingMonitorView.OnViewVisible();

                    ContentControlApplication.Content = _pingMonitorView;
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    if (_tracerouteHostView == null)
                        _tracerouteHostView = new TracerouteHostView();
                    else
                        _tracerouteHostView.OnViewVisible();

                    ContentControlApplication.Content = _tracerouteHostView;
                    break;
                case ApplicationViewManager.Name.DNSLookup:
                    if (_dnsLookupHostView == null)
                        _dnsLookupHostView = new DNSLookupHostView();
                    else
                        _dnsLookupHostView.OnViewVisible();

                    ContentControlApplication.Content = _dnsLookupHostView;
                    break;
                case ApplicationViewManager.Name.RemoteDesktop:
                    if (_remoteDesktopHostView == null)
                        _remoteDesktopHostView = new RemoteDesktopHostView();
                    else
                        _remoteDesktopHostView.OnViewVisible();

                    ContentControlApplication.Content = _remoteDesktopHostView;
                    break;
                case ApplicationViewManager.Name.PowerShell:
                    if (_powerShellHostView == null)
                        _powerShellHostView = new PowerShellHostView();
                    else
                        _powerShellHostView.OnViewVisible();

                    ContentControlApplication.Content = _powerShellHostView;
                    break;
                case ApplicationViewManager.Name.PuTTY:
                    if (_puttyHostView == null)
                        _puttyHostView = new PuTTYHostView();
                    else
                        _puttyHostView.OnViewVisible();

                    ContentControlApplication.Content = _puttyHostView;
                    break;
                case ApplicationViewManager.Name.TigerVNC:
                    if (_tigerVNCHostView == null)
                        _tigerVNCHostView = new TigerVNCHostView();
                    else
                        _tigerVNCHostView.OnViewVisible();

                    ContentControlApplication.Content = _tigerVNCHostView;
                    break;
                case ApplicationViewManager.Name.SNMP:
                    if (_snmpHostView == null)
                        _snmpHostView = new SNMPHostView();
                    else
                        _snmpHostView.OnViewVisible();

                    ContentControlApplication.Content = _snmpHostView;
                    break;
                case ApplicationViewManager.Name.WakeOnLAN:
                    if (_wakeOnLanView == null)
                        _wakeOnLanView = new WakeOnLANView();
                    else
                        _wakeOnLanView.OnViewVisible();

                    ContentControlApplication.Content = _wakeOnLanView;
                    break;
                case ApplicationViewManager.Name.HTTPHeaders:
                    if (_httpHeadersHostView == null)
                        _httpHeadersHostView = new HTTPHeadersHostView();
                    else
                        _httpHeadersHostView.OnViewVisible();

                    ContentControlApplication.Content = _httpHeadersHostView;
                    break;
                case ApplicationViewManager.Name.Whois:
                    if (_whoisHostView == null)
                        _whoisHostView = new WhoisHostView();
                    else
                        _whoisHostView.OnViewVisible();

                    ContentControlApplication.Content = _whoisHostView;
                    break;
                case ApplicationViewManager.Name.SubnetCalculator:
                    if (_subnetCalculatorHostView == null)
                        _subnetCalculatorHostView = new SubnetCalculatorHostView();

                    ContentControlApplication.Content = _subnetCalculatorHostView;
                    break;
                case ApplicationViewManager.Name.Lookup:
                    if (_lookupHostView == null)
                        _lookupHostView = new LookupHostView();

                    ContentControlApplication.Content = _lookupHostView;
                    break;
                case ApplicationViewManager.Name.Connections:
                    if (_connectionsView == null)
                        _connectionsView = new ConnectionsView();
                    else
                        _connectionsView.OnViewVisible();

                    ContentControlApplication.Content = _connectionsView;
                    break;
                case ApplicationViewManager.Name.Listeners:
                    if (_listenersView == null)
                        _listenersView = new ListenersView();
                    else
                        _listenersView.OnViewVisible();

                    ContentControlApplication.Content = _listenersView;
                    break;
                case ApplicationViewManager.Name.ARPTable:
                    if (_arpTableView == null)
                        _arpTableView = new ARPTableView();
                    else
                        _arpTableView.OnViewVisible();

                    ContentControlApplication.Content = _arpTableView;
                    break;
                case ApplicationViewManager.Name.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(name), name, null);
            }

            _currentApplicationViewName = name;
        }

        private void ClearSearchOnApplicationListMinimize()
        {
            if (ExpandApplicationView)
                return;

            if (IsApplicationListOpen && IsTextBoxSearchFocused)
                return;

            if (IsApplicationListOpen && IsMouseOverApplicationList)
                return;

            Search = string.Empty;

            // Scroll into view
            ListViewApplication.ScrollIntoView(SelectedApplication);
        }

        // This works, but is currently not used :) 
        private void EventSystem_RedirectProfileToApplicationEvent(object sender, EventArgs e)
        {
            if (!(e is EventSystemRedirectProfileApplicationArgs profile))
                return;

            // Change view
            SelectedApplication = Applications.SourceCollection.Cast<ApplicationViewInfo>().FirstOrDefault(x => x.Name == profile.Application);

            // Crate a new tab / perform action
            switch (profile.Application)
            {
                case ApplicationViewManager.Name.None:
                    break;
                case ApplicationViewManager.Name.Dashboard:
                    break;
                case ApplicationViewManager.Name.NetworkInterface:
                    break;
                case ApplicationViewManager.Name.IPScanner:
                    _ipScannerHostView.AddTab(profile.Profile);
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    _portScannerHostView.AddTab(profile.Profile);
                    break;
                case ApplicationViewManager.Name.Ping:
                    _pingHostView.AddTab(profile.Profile);
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    _tracerouteHostView.AddTab(profile.Profile);
                    break;
                case ApplicationViewManager.Name.DNSLookup:
                    break;
                case ApplicationViewManager.Name.RemoteDesktop:
                    break;
                case ApplicationViewManager.Name.PowerShell:
                    break;
                case ApplicationViewManager.Name.PuTTY:
                    break;
                case ApplicationViewManager.Name.TigerVNC:
                    break;
                case ApplicationViewManager.Name.SNMP:
                    break;
                case ApplicationViewManager.Name.WakeOnLAN:
                    break;
                case ApplicationViewManager.Name.HTTPHeaders:
                    break;
                case ApplicationViewManager.Name.Whois:
                    break;
                case ApplicationViewManager.Name.SubnetCalculator:
                    break;
                case ApplicationViewManager.Name.Lookup:
                    break;
                case ApplicationViewManager.Name.Connections:
                    break;
                case ApplicationViewManager.Name.Listeners:
                    break;
                case ApplicationViewManager.Name.ARPTable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EventSystem_RedirectDataToApplicationEvent(object sender, EventArgs e)
        {
            if (!(e is EventSystemRedirectDataApplicationArgs data))
                return;

            // Change view
            SelectedApplication = Applications.SourceCollection.Cast<ApplicationViewInfo>().FirstOrDefault(x => x.Name == data.Application);

            // Crate a new tab / perform action
            switch (data.Application)
            {
                case ApplicationViewManager.Name.IPScanner:
                    _ipScannerHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    _portScannerHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.Ping:
                    _pingHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    _tracerouteHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.DNSLookup:
                    _dnsLookupHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.RemoteDesktop:
                    _remoteDesktopHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.PowerShell:
                    _powerShellHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.PuTTY:
                    _puttyHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.TigerVNC:
                    _tigerVNCHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.SNMP:
                    _snmpHostView.AddTab(data.Args);
                    break;
                case ApplicationViewManager.Name.NetworkInterface:
                    break;
                case ApplicationViewManager.Name.WakeOnLAN:
                    break;
                case ApplicationViewManager.Name.HTTPHeaders:
                    break;
                case ApplicationViewManager.Name.Whois:
                    break;
                case ApplicationViewManager.Name.SubnetCalculator:
                    break;
                case ApplicationViewManager.Name.Lookup:
                    break;
                case ApplicationViewManager.Name.Connections:
                    break;
                case ApplicationViewManager.Name.Listeners:
                    break;
                case ApplicationViewManager.Name.ARPTable:
                    break;
                case ApplicationViewManager.Name.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region Settings
        private void OpenSettings()
        {
            // Save current language code
            if (string.IsNullOrEmpty(_cultureCode))
                _cultureCode = SettingsManager.Current.Localization_CultureCode;

            // Init settings view
            if (_settingsView == null)
            {
                _settingsView = new SettingsView(SelectedApplication.Name);
                ContentControlSettings.Content = _settingsView;
            }
            else // Change view
            {
                _settingsView.ChangeSettingsView(SelectedApplication.Name);
                _settingsView.Refresh();
            }

            // Show the view (this will hide other content)
            ShowSettingsView = true;

            // Bring window to front
            ShowWindowAction();
        }

        private void EventSystem_RedirectToSettingsEvent(object sender, EventArgs e)
        {
            OpenSettings();
        }

        private async void CloseSettings()
        {
            ShowSettingsView = false;

            // Enable/disable tray icon
            if (!_isInTray)
            {
                if (SettingsManager.Current.TrayIcon_AlwaysShowIcon && _notifyIcon == null)
                    InitNotifyIcon();

                if (_notifyIcon != null)
                    _notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;

                MetroMainWindow.HideOverlay();
            }

            // Ask the user to restart (if he has changed the language)
            if (_cultureCode != SettingsManager.Current.Localization_CultureCode || AllowsTransparency != SettingsManager.Current.Appearance_EnableTransparency)
            {
                ShowWindowAction();

                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = NETworkManager.Resources.Localization.Strings.RestartNow;
                settings.NegativeButtonText = NETworkManager.Resources.Localization.Strings.OK;
                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                ConfigurationManager.Current.FixAirspace = true;

                if (await this.ShowMessageAsync(NETworkManager.Resources.Localization.Strings.RestartRequired, NETworkManager.Resources.Localization.Strings.RestartRequiredSettingsChangedMessage, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
                {
                    RestartApplication();
                    return;
                }

                ConfigurationManager.Current.FixAirspace = false;
            }

            // Change the transparency
            if (AllowsTransparency != SettingsManager.Current.Appearance_EnableTransparency || (Opacity != SettingsManager.Current.Appearance_Opacity))
            {
                if (!AllowsTransparency || !SettingsManager.Current.Appearance_EnableTransparency)
                    Opacity = 1;
                else
                    Opacity = SettingsManager.Current.Appearance_Opacity;
            }

            // Change HotKeys
            if (SettingsManager.HotKeysChanged)
            {
                UnregisterHotKeys();
                RegisterHotKeys();

                SettingsManager.HotKeysChanged = false;
            }

            // Save the settings
            if (SettingsManager.Current.SettingsChanged)
                SettingsManager.Save();

            // Refresh the view
            ChangeApplicationView(SelectedApplication.Name, true);
        }
        #endregion

        #region Handle WndProc messages (Single instance, handle HotKeys)
        private HwndSource _hwndSoure;

        // This is called after MainWindow() and before OnContentRendered() --> to register hotkeys...
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwndSoure = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);

            _hwndSoure?.AddHook(HwndHook);

            RegisterHotKeys();
        }

        [DebuggerStepThrough]
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Single instance or Hotkey --> Show window
            if (msg == SingleInstance.WM_SHOWME || msg == WmHotkey && wParam.ToInt32() == 1)
            {
                ShowWindowAction();
                handled = true;
            }

            return IntPtr.Zero;
        }
        #endregion

        #region Update check
        private void CheckForUpdates()
        {
            var updater = new Updater();

            updater.UpdateAvailable += Updater_UpdateAvailable;
            updater.Error += Updater_Error;
            updater.Check();
        }

        private static void Updater_Error(object sender, EventArgs e)
        {
            //  Log
        }

        private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
        {
            UpdateText = string.Format(NETworkManager.Resources.Localization.Strings.VersionxxIsAvailable, e.Version);
            IsUpdateAvailable = true;
        }
        #endregion

        #region HotKeys (Register / Unregister)
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // WM_HOTKEY
        private const int WmHotkey = 0x0312;

        /* ID | Command
        *  ---|-------------------
        *  1  | ShowWindowAction()
        */

        private readonly List<int> _registeredHotKeys = new List<int>();

        private void RegisterHotKeys()
        {
            if (!SettingsManager.Current.HotKey_ShowWindowEnabled)
                return;

            RegisterHotKey(new WindowInteropHelper(this).Handle, 1, SettingsManager.Current.HotKey_ShowWindowModifier, SettingsManager.Current.HotKey_ShowWindowKey);
            _registeredHotKeys.Add(1);
        }

        private void UnregisterHotKeys()
        {
            // Unregister all registred keys
            foreach (var i in _registeredHotKeys)
                UnregisterHotKey(new WindowInteropHelper(this).Handle, i);

            // Clear list
            _registeredHotKeys.Clear();
        }
        #endregion

        #region NotifyIcon
        private void InitNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();

            // Get the application icon for the tray
            using (var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Images/NETworkManager.ico"))?.Stream)
            {
                if (iconStream != null)
                    _notifyIcon.Icon = new Icon(iconStream);
            }

            _notifyIcon.Text = Title;
            _notifyIcon.Click += NotifyIcon_Click;
            _notifyIcon.MouseDown += NotifyIcon_MouseDown;
            _notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
        }

        private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var trayMenu = (ContextMenu)FindResource("ContextMenuNotifyIcon");

            trayMenu.IsOpen = true;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs mouse = (System.Windows.Forms.MouseEventArgs)e;

            if (mouse.Button != MouseButtons.Left)
                return;

            if (OpenStatusWindowCommand.CanExecute(null))
                OpenStatusWindowCommand.Execute(null);
        }

        private void MetroWindowMain_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                return;

            if (SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar)
                HideWindowToTray();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = this;
        }

        private void HideWindowToTray()
        {
            if (_notifyIcon == null)
                InitNotifyIcon();

            _isInTray = true;

            _notifyIcon.Visible = true;

            Hide();
        }

        private void ShowWindowFromTray()
        {
            _isInTray = false;

            Show();

            _notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
        }

        private void BringWindowToFront()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();
        }
        #endregion

        #region ICommands & Actions
        public ICommand OpenStatusWindowCommand => new RelayCommand(p => OpenStatusWindowAction());

        private void OpenStatusWindowAction()
        {
            OpenStatusWindow();
        }

        public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

        private static void OpenWebsiteAction(object url)
        {
            Process.Start((string)url);
        }

        public ICommand OpenDocumentationCommand
        {
            get { return new RelayCommand(p => OpenDocumentationAction()); }
        }

        private void OpenDocumentationAction()
        {
            // ToDo: if(settingsView) --> Show help for settings ?!
            DocumentationManager.OpenDocumentation(DocumentationManager.GetIdentifierByAppliactionName(SelectedApplication.Name));
        }

        public ICommand OpenApplicationListCommand
        {
            get { return new RelayCommand(p => OpenApplicationListAction()); }
        }

        private void OpenApplicationListAction()
        {
            IsApplicationListOpen = true;
            TextBoxSearch.Focus();
        }

        public ICommand OpenSettingsCommand
        {
            get { return new RelayCommand(p => OpenSettingsAction()); }
        }

        private void OpenSettingsAction()
        {
            OpenSettings();
        }

        public ICommand CloseSettingsCommand
        {
            get { return new RelayCommand(p => CloseSettingsAction()); }
        }

        private void CloseSettingsAction()
        {
            CloseSettings();
        }

        public ICommand ShowWindowCommand
        {
            get { return new RelayCommand(p => ShowWindowAction()); }
        }

        private void ShowWindowAction()
        {
            if (_isInTray)
                ShowWindowFromTray();

            if (!IsActive)
                BringWindowToFront();
        }

        public ICommand CloseApplicationCommand
        {
            get { return new RelayCommand(p => CloseApplicationAction()); }
        }

        private void CloseApplicationAction()
        {
            _closeApplication = true;
            Close();
        }

        private void RestartApplication(bool closeApplication = true)
        {
            new Process
            {
                StartInfo =
                {
                    FileName = ConfigurationManager.Current.ApplicationFullName,
                    Arguments = $"--restart-pid:{Process.GetCurrentProcess().Id}"
                }
            }.Start();

            if (!closeApplication)
                return;

            _closeApplication = true;
            Close();
        }

        public ICommand ApplicationListMouseEnterCommand
        {
            get { return new RelayCommand(p => ApplicationListMouseEnterAction()); }
        }

        private void ApplicationListMouseEnterAction()
        {
            IsMouseOverApplicationList = true;
        }

        public ICommand ApplicationListMouseLeaveCommand
        {
            get { return new RelayCommand(p => ApplicationListMouseLeaveAction()); }
        }

        private void ApplicationListMouseLeaveAction()
        {
            // Don't minmize the list, if the user has accidently moved the mouse while searching
            if (!IsTextBoxSearchFocused)
                IsApplicationListOpen = false;

            IsMouseOverApplicationList = false;
        }

        public ICommand TextBoxSearchGotKeyboardFocusCommand
        {
            get { return new RelayCommand(p => TextBoxSearchGotKeyboardFocusAction()); }
        }

        private void TextBoxSearchGotKeyboardFocusAction()
        {
            IsTextBoxSearchFocused = true;
        }

        public ICommand TextBoxSearchLostKeyboardFocusCommand
        {
            get { return new RelayCommand(p => TextBoxSearchLostKeyboardFocusAction()); }
        }

        private void TextBoxSearchLostKeyboardFocusAction()
        {
            if (!IsMouseOverApplicationList)
                IsApplicationListOpen = false;

            IsTextBoxSearchFocused = false;
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion

        #region Methods
        private void OpenStatusWindow()
        {
            statusWindow.ShowFromExternal();
        }
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
        #endregion

        #region Bugfixes
        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Window helper
        // Move the window when the user hold the title...
        private void HeaderBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        #endregion
    }
}
