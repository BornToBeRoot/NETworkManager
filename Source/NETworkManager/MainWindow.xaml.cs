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
using NETworkManager.Settings;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;
using NETworkManager.Utilities;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using NETworkManager.Controls;
using NETworkManager.Documentation;
using NETworkManager.ViewModels;
using ContextMenu = System.Windows.Controls.ContextMenu;
using NETworkManager.Profiles;
using NETworkManager.Localization;
using NETworkManager.Localization.Translators;
using NETworkManager.Update;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;

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
        private bool _isProfileLoading;
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

        private ApplicationInfo _selectedApplication;
        public ApplicationInfo SelectedApplication
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

        private ApplicationName _filterLastViewName;
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

                var sourceCollection = Applications.SourceCollection.Cast<ApplicationInfo>();
                var filteredCollection = Applications.Cast<ApplicationInfo>();

                var sourceInfos = sourceCollection as ApplicationInfo[] ?? sourceCollection.ToArray();
                var filteredInfos = filteredCollection as ApplicationInfo[] ?? filteredCollection.ToArray();

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

        private ICollectionView _profileFiles;
        public ICollectionView ProfileFiles
        {
            get => _profileFiles;
            set
            {
                if (value == _profileFiles)
                    return;

                _profileFiles = value;
                OnPropertyChanged();
            }
        }

        private ProfileFileInfo _selectedProfileFile = null;
        public ProfileFileInfo SelectedProfileFile
        {
            get => _selectedProfileFile;
            set
            {
                if (_isProfileLoading || (value != null && value.Equals(_selectedProfileFile)))
                    return;

                _selectedProfileFile = value;

                // Switch profile...
                if (value != null && !value.Equals(ProfileManager.LoadedProfileFile))
                {
                    ProfileManager.SwitchProfile(value);
                    SettingsManager.Current.Profiles_LastSelected = value.Name;
                }

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
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(LocalizationManager.GetInstance().Culture.IetfLanguageTag)));

            // Load / Change appearance
            AppearanceManager.Load();

            // Set title
            Title = $"NETworkManager {AssemblyManager.Current.Version.Major}.{AssemblyManager.Current.Version.Minor}.{AssemblyManager.Current.Version.Build}";

            // NotifyIcon for autostart
            if (CommandLineManager.Current.Autostart && SettingsManager.Current.Autostart_StartMinimizedInTray || SettingsManager.Current.TrayIcon_AlwaysShowIcon)
                InitNotifyIcon();

            // Load settings
            ExpandApplicationView = SettingsManager.Current.ExpandApplicationView;

            // Register event system...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
            //EventSystem.RedirectProfileToApplicationEvent += EventSystem_RedirectProfileToApplicationEvent;
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
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                ConfigurationManager.Current.FixAirspace = true;

                await this.ShowMessageAsync(Localization.Resources.Strings.SettingsHaveBeenReset, Localization.Resources.Strings.SettingsFileFoundWasCorruptOrNotCompatibleMessage, MessageDialogStyle.Affirmative, settings);

                ConfigurationManager.Current.FixAirspace = false;
            }

            if (SettingsManager.Current.FirstRun)
            {
                // Show first run dialog...
                var customDialog = new CustomDialog
                {
                    Title = Localization.Resources.Strings.Welcome
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

            // Load profiles    
            _isProfileLoading = true;
            ProfileFiles = new CollectionViewSource { Source = ProfileManager.ProfileFiles }.View;
            ProfileFiles.SortDescriptions.Add(new SortDescription(nameof(ProfileFileInfo.Name), ListSortDirection.Ascending));
            ProfileManager.OnProfileFileChangedEvent += ProfileManager_OnProfileFileChangedEvent;
            _isProfileLoading = false;

            // Switch profile
            SelectedProfileFile = ProfileFiles.SourceCollection.Cast<ProfileFileInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Profiles_LastSelected);

            if (SelectedProfileFile == null)
                SelectedProfileFile = ProfileFiles.SourceCollection.Cast<ProfileFileInfo>().FirstOrDefault();

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
                SettingsManager.Current.General_ApplicationList = new ObservableSetCollection<ApplicationInfo>(ApplicationManager.GetList());
            }
            else // Check for missing applications and add them
            {
                foreach (ApplicationInfo info in ApplicationManager.GetList())
                {
                    bool isInList = false;

                    foreach (ApplicationInfo info2 in SettingsManager.Current.General_ApplicationList)
                    {
                        if (info.Name == info2.Name)
                            isInList = true;
                    }

                    if (!isInList)
                        SettingsManager.Current.General_ApplicationList.Add(info);
                }
            }

            Applications = new CollectionViewSource { Source = SettingsManager.Current.General_ApplicationList }.View;

            Applications.SortDescriptions.Add(new SortDescription(nameof(ApplicationInfo.Name), ListSortDirection.Ascending)); // Always have the same order, even if it is translated...
            Applications.Filter = o =>
            {
                if (!(o is ApplicationInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.IsVisible;

                var regex = new Regex(@" |-");

                var search = regex.Replace(Search, "");

                // Search by TranslatedName and Name
                return info.IsVisible && (regex.Replace(ApplicationNameTranslator.GetInstance().Translate(info.Name), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || regex.Replace(info.Name.ToString(), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            };

            SettingsManager.Current.General_ApplicationList.CollectionChanged += (sender, args) => Applications.Refresh();

            isApplicationListLoading = false;

            // Select the application
            SelectedApplication = Applications.SourceCollection.Cast<ApplicationInfo>().FirstOrDefault(x => x.Name == (CommandLineManager.Current.Application != ApplicationName.None ? CommandLineManager.Current.Application : SettingsManager.Current.General_DefaultApplicationViewName));

            // Scroll into view
            if (SelectedApplication != null)
                ListViewApplication.ScrollIntoView(SelectedApplication);
        }

        /// <summary>
        /// Update the view when the loaded profile file changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfileManager_OnProfileFileChangedEvent(object sender, ProfileFileInfoArgs e)
        {
            SelectedProfileFile = null;

            SelectedProfileFile = ProfileFiles.SourceCollection.Cast<ProfileFileInfo>().FirstOrDefault(x => x.Name == e.ProfileFileInfo.Name);

            if (SelectedProfileFile == null)
                SelectedProfileFile = ProfileFiles.SourceCollection.Cast<ProfileFileInfo>().FirstOrDefault();
        }

        private async void MetroWindowMain_Closing(object sender, CancelEventArgs e)
        {
            // Force restart --> Import, Reset, etc.
            if (ConfigurationManager.Current.ForceRestart)
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

                settings.AffirmativeButtonText = Localization.Resources.Strings.Close;
                settings.NegativeButtonText = Localization.Resources.Strings.Cancel;
                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                // Fix airspace issues
                ConfigurationManager.Current.FixAirspace = true;

                var result = await this.ShowMessageAsync(Localization.Resources.Strings.Confirm, Localization.Resources.Strings.ConfirmCloseMessage, MessageDialogStyle.AffirmativeAndNegative, settings);

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
        private PingMonitorHostView _pingMonitorHostView;
        private TracerouteHostView _tracerouteHostView;
        private DNSLookupHostView _dnsLookupHostView;
        private RemoteDesktopHostView _remoteDesktopHostView;
        private PowerShellHostView _powerShellHostView;
        private PuTTYHostView _puttyHostView;
        private TigerVNCHostView _tigerVNCHostView;
        private WebConsoleHostView _webConsoleHostView;
        private SNMPHostView _snmpHostView;
        private DiscoveryProtocolView _discoveryProtocolView;
        private WakeOnLANView _wakeOnLanView;
        private SubnetCalculatorHostView _subnetCalculatorHostView;
        private LookupHostView _lookupHostView;
        private WhoisHostView _whoisHostView;
        private ConnectionsView _connectionsView;
        private ListenersView _listenersView;
        private ARPTableView _arpTableView;

        private ApplicationName _currentApplicationViewName = ApplicationName.None;

        private void ChangeApplicationView(ApplicationName name, bool refresh = false)
        {
            if (!refresh && _currentApplicationViewName == name)
                return;

            // Stop some functions on the old view
            switch (_currentApplicationViewName)
            {
                case ApplicationName.NetworkInterface:
                    _networkInterfaceView?.OnViewHide();
                    break;
                case ApplicationName.WiFi:
                    _wiFiView?.OnViewHide();
                    break;
                case ApplicationName.Connections:
                    _connectionsView?.OnViewHide();
                    break;
                case ApplicationName.Listeners:
                    _listenersView?.OnViewHide();
                    break;
                case ApplicationName.ARPTable:
                    _arpTableView?.OnViewHide();
                    break;
            }

            // Create new view / start some functions
            switch (name)
            {
                case ApplicationName.Dashboard:
                    if (_overviewView == null)
                        _overviewView = new DashboardView();
                    else
                        _overviewView.OnViewVisible();

                    ContentControlApplication.Content = _overviewView;
                    break;
                case ApplicationName.NetworkInterface:
                    if (_networkInterfaceView == null)
                        _networkInterfaceView = new NetworkInterfaceView();
                    else
                        _networkInterfaceView.OnViewVisible();

                    ContentControlApplication.Content = _networkInterfaceView;
                    break;
                case ApplicationName.WiFi:
                    if (_wiFiView == null)
                        _wiFiView = new WiFiView();
                    else
                        _wiFiView.OnViewVisible();

                    ContentControlApplication.Content = _wiFiView;
                    break;
                case ApplicationName.IPScanner:
                    if (_ipScannerHostView == null)
                        _ipScannerHostView = new IPScannerHostView();
                    else
                        _ipScannerHostView.OnViewVisible();

                    ContentControlApplication.Content = _ipScannerHostView;
                    break;
                case ApplicationName.PortScanner:
                    if (_portScannerHostView == null)
                        _portScannerHostView = new PortScannerHostView();
                    else
                        _portScannerHostView.OnViewVisible();

                    ContentControlApplication.Content = _portScannerHostView;
                    break;
                case ApplicationName.PingMonitor:
                    if (_pingMonitorHostView == null)
                        _pingMonitorHostView = new PingMonitorHostView();
                    else
                        _pingMonitorHostView.OnViewVisible();

                    ContentControlApplication.Content = _pingMonitorHostView;
                    break;
                case ApplicationName.Traceroute:
                    if (_tracerouteHostView == null)
                        _tracerouteHostView = new TracerouteHostView();
                    else
                        _tracerouteHostView.OnViewVisible();

                    ContentControlApplication.Content = _tracerouteHostView;
                    break;
                case ApplicationName.DNSLookup:
                    if (_dnsLookupHostView == null)
                        _dnsLookupHostView = new DNSLookupHostView();
                    else
                        _dnsLookupHostView.OnViewVisible();

                    ContentControlApplication.Content = _dnsLookupHostView;
                    break;
                case ApplicationName.RemoteDesktop:
                    if (_remoteDesktopHostView == null)
                        _remoteDesktopHostView = new RemoteDesktopHostView();
                    else
                        _remoteDesktopHostView.OnViewVisible();

                    ContentControlApplication.Content = _remoteDesktopHostView;
                    break;
                case ApplicationName.PowerShell:
                    if (_powerShellHostView == null)
                        _powerShellHostView = new PowerShellHostView();
                    else
                        _powerShellHostView.OnViewVisible();

                    ContentControlApplication.Content = _powerShellHostView;
                    break;
                case ApplicationName.PuTTY:
                    if (_puttyHostView == null)
                        _puttyHostView = new PuTTYHostView();
                    else
                        _puttyHostView.OnViewVisible();

                    ContentControlApplication.Content = _puttyHostView;
                    break;
                case ApplicationName.TigerVNC:
                    if (_tigerVNCHostView == null)
                        _tigerVNCHostView = new TigerVNCHostView();
                    else
                        _tigerVNCHostView.OnViewVisible();

                    ContentControlApplication.Content = _tigerVNCHostView;
                    break;
                case ApplicationName.WebConsole:
                    if (_webConsoleHostView == null)
                        _webConsoleHostView = new WebConsoleHostView();
                    else
                        _webConsoleHostView.OnViewVisible();

                    ContentControlApplication.Content = _webConsoleHostView;
                    break;
                case ApplicationName.SNMP:
                    if (_snmpHostView == null)
                        _snmpHostView = new SNMPHostView();
                    else
                        _snmpHostView.OnViewVisible();

                    ContentControlApplication.Content = _snmpHostView;
                    break;
                case ApplicationName.DiscoveryProtocol:
                    if (_discoveryProtocolView == null)
                        _discoveryProtocolView = new DiscoveryProtocolView();
                    else
                        _discoveryProtocolView.OnViewVisible();

                    ContentControlApplication.Content = _discoveryProtocolView;
                    break;
                case ApplicationName.WakeOnLAN:
                    if (_wakeOnLanView == null)
                        _wakeOnLanView = new WakeOnLANView();
                    else
                        _wakeOnLanView.OnViewVisible();

                    ContentControlApplication.Content = _wakeOnLanView;
                    break;
                case ApplicationName.Whois:
                    if (_whoisHostView == null)
                        _whoisHostView = new WhoisHostView();
                    else
                        _whoisHostView.OnViewVisible();

                    ContentControlApplication.Content = _whoisHostView;
                    break;
                case ApplicationName.SubnetCalculator:
                    if (_subnetCalculatorHostView == null)
                        _subnetCalculatorHostView = new SubnetCalculatorHostView();

                    ContentControlApplication.Content = _subnetCalculatorHostView;
                    break;
                case ApplicationName.Lookup:
                    if (_lookupHostView == null)
                        _lookupHostView = new LookupHostView();

                    ContentControlApplication.Content = _lookupHostView;
                    break;
                case ApplicationName.Connections:
                    if (_connectionsView == null)
                        _connectionsView = new ConnectionsView();
                    else
                        _connectionsView.OnViewVisible();

                    ContentControlApplication.Content = _connectionsView;
                    break;
                case ApplicationName.Listeners:
                    if (_listenersView == null)
                        _listenersView = new ListenersView();
                    else
                        _listenersView.OnViewVisible();

                    ContentControlApplication.Content = _listenersView;
                    break;
                case ApplicationName.ARPTable:
                    if (_arpTableView == null)
                        _arpTableView = new ARPTableView();
                    else
                        _arpTableView.OnViewVisible();

                    ContentControlApplication.Content = _arpTableView;
                    break;
                case ApplicationName.None:
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

        private void EventSystem_RedirectDataToApplicationEvent(object sender, EventArgs e)
        {
            if (!(e is EventSystemRedirectArgs data))
                return;

            // Change view
            SelectedApplication = Applications.SourceCollection.Cast<ApplicationInfo>().FirstOrDefault(x => x.Name == data.Application);

            // Crate a new tab / perform action
            switch (data.Application)
            {
                case ApplicationName.IPScanner:
                    _ipScannerHostView.AddTab(data.Args);
                    break;
                case ApplicationName.PortScanner:
                    _portScannerHostView.AddTab(data.Args);
                    break;
                case ApplicationName.PingMonitor:
                    _pingMonitorHostView.AddHost(data.Args);
                    break;
                case ApplicationName.Traceroute:
                    _tracerouteHostView.AddTab(data.Args);
                    break;
                case ApplicationName.DNSLookup:
                    _dnsLookupHostView.AddTab(data.Args);
                    break;
                case ApplicationName.RemoteDesktop:
                    _remoteDesktopHostView.AddTab(data.Args);
                    break;
                case ApplicationName.PowerShell:
                    _powerShellHostView.AddTab(data.Args);
                    break;
                case ApplicationName.PuTTY:
                    _puttyHostView.AddTab(data.Args);
                    break;
                case ApplicationName.TigerVNC:
                    _tigerVNCHostView.AddTab(data.Args);
                    break;
                case ApplicationName.SNMP:
                    _snmpHostView.AddTab(data.Args);
                    break;
                case ApplicationName.NetworkInterface:
                    break;
                case ApplicationName.WakeOnLAN:
                    break;
                case ApplicationName.Whois:
                    break;
                case ApplicationName.SubnetCalculator:
                    break;
                case ApplicationName.Lookup:
                    break;
                case ApplicationName.Connections:
                    break;
                case ApplicationName.Listeners:
                    break;
                case ApplicationName.ARPTable:
                    break;
                case ApplicationName.None:
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
            if (_cultureCode != SettingsManager.Current.Localization_CultureCode)
            {
                ShowWindowAction();

                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.RestartNow;
                settings.NegativeButtonText = Localization.Resources.Strings.OK;
                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                ConfigurationManager.Current.FixAirspace = true;

                if (await this.ShowMessageAsync(Localization.Resources.Strings.RestartRequired, Localization.Resources.Strings.RestartRequiredSettingsChangedMessage, MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
                {
                    RestartApplication();
                    return;
                }

                ConfigurationManager.Current.FixAirspace = false;
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
            updater.CheckOnGitHub(Properties.Resources.NETworkManager_GitHub_User, Properties.Resources.NETworkManager_GitHub_Repo, AssemblyManager.Current.Version);
        }

        private static void Updater_Error(object sender, EventArgs e)
        {
            //  Log
        }

        private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
        {
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
            using (var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/NETworkManager.ico"))?.Stream)
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
            ExternalProcessStarter.OpenUrl((string)url);
        }

        public ICommand OpenDocumentationCommand
        {
            get { return new RelayCommand(p => OpenDocumentationAction()); }
        }

        private void OpenDocumentationAction()
        {
            DocumentationManager.OpenDocumentation(ShowSettingsView ? DocumentationIdentifier.Default : DocumentationManager.GetIdentifierByAppliactionName(SelectedApplication.Name));
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

        public void RestartApplication(bool closeApplication = true, bool asAdmin = false)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = ConfigurationManager.Current.ApplicationFullName,
                Arguments = $"{CommandLineManager.GetParameterWithSplitIdentifier(CommandLineManager.ParameterRestartPid)}{Process.GetCurrentProcess().Id} {CommandLineManager.GetParameterWithSplitIdentifier(CommandLineManager.ParameterApplication)}{_currentApplicationViewName}",
                UseShellExecute = true
            };

            if (asAdmin)
                info.Verb = "runas";

            Process.Start(info);

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
    }
}
