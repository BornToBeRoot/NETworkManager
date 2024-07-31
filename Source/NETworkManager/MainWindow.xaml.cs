using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Threading;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Documentation;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.AWS;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.Network;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.PuTTY;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Update;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;
using NETworkManager.Views;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace NETworkManager;

public sealed partial class MainWindow : INotifyPropertyChanged
{
    #region Events

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // Show restart required note for some settings
            case nameof(SettingsInfo.Localization_CultureCode):
                IsRestartRequired = true;

                break;

            // Update TrayIcon if changed in the settings    
            case nameof(SettingsInfo.TrayIcon_AlwaysShowIcon):
                if (SettingsManager.Current.TrayIcon_AlwaysShowIcon && _notifyIcon == null)
                    InitNotifyIcon();

                if (_notifyIcon != null)
                    _notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;

                break;

            // Update DNS server if changed in the settings
            case nameof(SettingsInfo.Network_UseCustomDNSServer):
            case nameof(SettingsInfo.Network_CustomDNSServer):
                ConfigureDNS();

                break;

            // Update PowerShell profile if changed in the settings
            case nameof(SettingsInfo.Appearance_PowerShellModifyGlobalProfile):
            case nameof(SettingsInfo.Appearance_Theme):
            case nameof(SettingsInfo.PowerShell_ApplicationFilePath):
            case nameof(SettingsInfo.AWSSessionManager_ApplicationFilePath):
                // Skip on welcome dialog
                if (SettingsManager.Current.WelcomeDialog_Show)
                    return;

                WriteDefaultPowerShellProfileToRegistry();

                break;
        }
    }

    #endregion

    #region Bugfixes

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }

    #endregion

    #region PropertyChangedEventHandler

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));

    private NotifyIcon _notifyIcon;
    private StatusWindow _statusWindow;

    private readonly bool _isLoading;
    private bool _isProfileFilesLoading;
    private bool _isProfileFileUpdating;
    private bool _isApplicationViewLoading;
    private bool _isNetworkChanging;

    private bool _isInTray;
    private bool _isClosing;

    private bool _applicationViewIsExpanded;

    public bool ApplicationViewIsExpanded
    {
        get => _applicationViewIsExpanded;
        set
        {
            if (value == _applicationViewIsExpanded)
                return;

            if (!_isLoading)
                SettingsManager.Current.ExpandApplicationView = value;

            if (!value)
                ClearSearchOnApplicationListMinimize();

            _applicationViewIsExpanded = value;
            OnPropertyChanged();
        }
    }

    private bool _textBoxApplicationSearchIsFocused;

    public bool TextBoxApplicationSearchIsFocused
    {
        get => _textBoxApplicationSearchIsFocused;
        set
        {
            if (value == _textBoxApplicationSearchIsFocused)
                return;

            if (!value)
                ClearSearchOnApplicationListMinimize();

            _textBoxApplicationSearchIsFocused = value;
            OnPropertyChanged();
        }
    }

    private bool _applicationViewIsOpen;

    public bool ApplicationViewIsOpen
    {
        get => _applicationViewIsOpen;
        set
        {
            if (value == _applicationViewIsOpen)
                return;

            if (!value)
                ClearSearchOnApplicationListMinimize();

            _applicationViewIsOpen = value;
            OnPropertyChanged();
        }
    }

    private bool _applicationViewIsMouseOver;

    public bool ApplicationViewIsMouseOver
    {
        get => _applicationViewIsMouseOver;
        set
        {
            if (value == _applicationViewIsMouseOver)
                return;

            if (!value)
                ClearSearchOnApplicationListMinimize();

            _applicationViewIsMouseOver = value;
            OnPropertyChanged();
        }
    }

    private ICollectionView _applications;

    public ICollectionView Applications
    {
        get => _applications;
        private set
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
            // Do not change the application view if the application view is loading
            if (_isApplicationViewLoading)
                return;

            // Should only be null if we try to re-set the selected application via search
            if (value == null && !_applicationViewIsEmpty)
                return;

            // Don't update the application view if the application is the same
            if (Equals(value, _selectedApplication))
                return;

            if (value != null && !_applicationViewIsEmpty)
            {
                // Hide the old application view
                if (_selectedApplication != null)
                    OnApplicationViewHide(_selectedApplication.Name);

                // Show the new application view
                OnApplicationViewVisible(value.Name);

                // Store the last selected application name
                ConfigurationManager.Current.CurrentApplication = value.Name;
            }

            _selectedApplication = value;
            OnPropertyChanged();
        }
    }

    private ApplicationInfo _previousSelectedApplication;

    private bool _applicationViewIsEmpty;

    private string _applicationSearch = string.Empty;

    public string ApplicationSearch
    {
        get => _applicationSearch;
        set
        {
            if (value == _applicationSearch)
                return;

            _applicationSearch = value;

            // Store the current selected application name
            if (SelectedApplication != null)
                _previousSelectedApplication = SelectedApplication;

            // Refresh (apply filter)
            Applications.Refresh();

            if (Applications.IsEmpty)
            {
                _applicationViewIsEmpty = true;
            }
            else if (_applicationViewIsEmpty) // Not empty anymore
            {
                SelectedApplication = null;

                // Try to select the last selected application, otherwise select the first one
                SelectedApplication = Applications.Cast<ApplicationInfo>()
                                          .FirstOrDefault(x => x.Name == _previousSelectedApplication.Name) ??
                                      Applications.Cast<ApplicationInfo>().FirstOrDefault();

                _applicationViewIsEmpty = false;
            }

            OnPropertyChanged();
        }
    }

    private SettingsView _settingsView;

    private bool _settingsViewIsOpen;

    public bool SettingsViewIsOpen
    {
        get => _settingsViewIsOpen;
        set
        {
            if (value == _settingsViewIsOpen)
                return;

            _settingsViewIsOpen = value;
            OnPropertyChanged();
        }
    }

    private bool _flyoutRunCommandIsOpen;

    public bool FlyoutRunCommandIsOpen
    {
        get => _flyoutRunCommandIsOpen;
        set
        {
            if (value == _flyoutRunCommandIsOpen)
                return;

            _flyoutRunCommandIsOpen = value;
            OnPropertyChanged();
        }
    }

    private bool _flyoutRunCommandAreAnimationsEnabled;

    public bool FlyoutRunCommandAreAnimationsEnabled
    {
        get => _flyoutRunCommandAreAnimationsEnabled;
        set
        {
            if (value == _flyoutRunCommandAreAnimationsEnabled)
                return;

            _flyoutRunCommandAreAnimationsEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _isRestartRequired;

    public bool IsRestartRequired
    {
        get => _isRestartRequired;
        set
        {
            if (value == _isRestartRequired)
                return;

            _isRestartRequired = value;
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

    private string _updateReleaseUrl;

    public string UpdateReleaseUrl
    {
        get => _updateReleaseUrl;
        private set
        {
            if (value == _updateReleaseUrl)
                return;

            _updateReleaseUrl = value;
            OnPropertyChanged();
        }
    }

    private ICollectionView _profileFiles;

    public ICollectionView ProfileFiles
    {
        get => _profileFiles;
        private set
        {
            if (value == _profileFiles)
                return;

            _profileFiles = value;
            OnPropertyChanged();
        }
    }

    private ProfileFileInfo _selectedProfileFile;

    public ProfileFileInfo SelectedProfileFile
    {
        get => _selectedProfileFile;
        set
        {
            if (_isProfileFilesLoading)
                return;

            if (value != null && value.Equals(_selectedProfileFile))
                return;

            _selectedProfileFile = value;

            if (value != null)
            {
                if (!_isProfileFileUpdating)
                    LoadProfile(value);

                ConfigurationManager.Current.ProfileManagerShowUnlock = value.IsEncrypted && !value.IsPasswordValid;
                SettingsManager.Current.Profiles_LastSelected = value.Name;
            }

            OnPropertyChanged();
        }
    }

    private bool _isProfileFileDropDownOpened;

    public bool IsProfileFileDropDownOpened
    {
        get => _isProfileFileDropDownOpened;
        set
        {
            if (value == _isProfileFileDropDownOpened)
                return;

            _isProfileFileDropDownOpened = value;
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

        // Language metadata
        LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(LocalizationManager.GetInstance().Culture.IetfLanguageTag)));

        // Load and change appearance
        AppearanceManager.Load();

        // Load and configure DNS
        ConfigureDNS();

        // Set window title
        Title = $"NETworkManager {AssemblyManager.Current.Version}";

        // Register event system...
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        EventSystem.OnRedirectDataToApplicationEvent += EventSystem_RedirectDataToApplicationEvent;
        EventSystem.OnRedirectToSettingsEvent += EventSystem_RedirectToSettingsEvent;

        _isLoading = false;
    }

    private void MetroMainWindow_ContentRendered(object sender, EventArgs e)
    {
        WelcomeThenLoadAsync();
    }

    private async void WelcomeThenLoadAsync()
    {
        // Show a note if settings have been reset
        if (ConfigurationManager.Current.ShowSettingsResetNoteOnStartup)
        {
            var settings = AppearanceManager.MetroDialog;
            settings.AffirmativeButtonText = Strings.OK;

            await this.ShowMessageAsync(Strings.SettingsHaveBeenReset,
                Strings.SettingsFileFoundWasCorruptOrNotCompatibleMessage,
                MessageDialogStyle.Affirmative, settings);
        }

        // Show welcome dialog
        if (SettingsManager.Current.WelcomeDialog_Show)
        {
            var customDialog = new CustomDialog
            {
                Title = Strings.Welcome
            };

            var welcomeViewModel = new WelcomeViewModel(async instance =>
            {
                await this.HideMetroDialogAsync(customDialog);

                // Set settings based on user choice
                SettingsManager.Current.Update_CheckForUpdatesAtStartup = instance.CheckForUpdatesAtStartup;
                SettingsManager.Current.Dashboard_CheckPublicIPAddress = instance.CheckPublicIPAddress;
                SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation =
                    instance.CheckIPApiIPGeolocation;
                SettingsManager.Current.Dashboard_CheckIPApiDNSResolver = instance.CheckIPApiDNSResolver;
                SettingsManager.Current.Traceroute_CheckIPApiIPGeolocation = instance.CheckIPApiIPGeolocation;
                SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile =
                    instance.PowerShellModifyGlobalProfile;

                // Generate lists at runtime
                SettingsManager.Current.General_ApplicationList =
                    new ObservableSetCollection<ApplicationInfo>(ApplicationManager.GetDefaultList());
                SettingsManager.Current.IPScanner_CustomCommands =
                    new ObservableCollection<CustomCommandInfo>(IPScannerCustomCommand.GetDefaultList());
                SettingsManager.Current.PortScanner_PortProfiles =
                    new ObservableCollection<PortProfileInfo>(PortProfile.GetDefaultList());
                SettingsManager.Current.DNSLookup_DNSServers =
                    new ObservableCollection<DNSServerConnectionInfoProfile>(DNSServer.GetDefaultList());
                SettingsManager.Current.AWSSessionManager_AWSProfiles =
                    new ObservableCollection<AWSProfileInfo>(AWSProfile.GetDefaultList());
                SettingsManager.Current.SNMP_OidProfiles =
                    new ObservableCollection<SNMPOIDProfileInfo>(SNMPOIDProfile.GetDefaultList());
                SettingsManager.Current.SNTPLookup_SNTPServers =
                    new ObservableCollection<ServerConnectionInfoProfile>(SNTPServer.GetDefaultList());

                // Check if PowerShell is installed
                foreach (var file in PowerShell.GetDefaultInstallationPaths.Where(File.Exists))
                {
                    SettingsManager.Current.PowerShell_ApplicationFilePath = file;
                    SettingsManager.Current.AWSSessionManager_ApplicationFilePath = file;

                    break;
                }

                // Check if PuTTY is installed
                foreach (var file in PuTTY.GetDefaultInstallationPaths.Where(File.Exists))
                {
                    SettingsManager.Current.PuTTY_ApplicationFilePath = file;
                    break;
                }

                SettingsManager.Current.WelcomeDialog_Show = false;

                // Save it to create a settings file
                SettingsManager.Save();

                Load();
            });

            customDialog.Content = new WelcomeDialog
            {
                DataContext = welcomeViewModel
            };

            await this.ShowMetroDialogAsync(customDialog).ConfigureAwait(true);
        }
        else
        {
            Load();
        }
    }

    private void Load()
    {
        // Load application list, filter, sort, etc.
        LoadApplicationList();

        // Load run commands
        SetRunCommandsView();

        // Load the profiles
        LoadProfiles();

        // Init notify icon
        if (SettingsManager.Current.TrayIcon_AlwaysShowIcon)
            InitNotifyIcon();

        // Hide to tray after the window shows up... not nice, but otherwise the hotkeys do not work
        if (CommandLineManager.Current.Autostart && SettingsManager.Current.Autostart_StartMinimizedInTray)
            HideWindowToTray();

        // Init status window
        _statusWindow = new StatusWindow(this);

        // Detect network changes...
        NetworkChange.NetworkAvailabilityChanged += (_, _) => OnNetworkHasChanged();
        NetworkChange.NetworkAddressChanged += (_, _) => OnNetworkHasChanged();

        // Set PowerShell global profile
        WriteDefaultPowerShellProfileToRegistry();

        // Search for updates... 
        if (SettingsManager.Current.Update_CheckForUpdatesAtStartup)
            CheckForUpdates();
    }

    private async void MetroWindowMain_Closing(object sender, CancelEventArgs e)
    {
        if (!_isClosing)
        {
            // Hide the application to tray
            if (SettingsManager.Current.Window_MinimizeInsteadOfTerminating && WindowState != WindowState.Minimized)
            {
                e.Cancel = true;

                WindowState = WindowState.Minimized;

                return;
            }

            // Confirm close
            if (SettingsManager.Current.Window_ConfirmClose)
            {
                e.Cancel = true;

                // If the window is minimized, bring it to front
                if (WindowState == WindowState.Minimized)
                    BringWindowToFront();

                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Strings.Close;
                settings.NegativeButtonText = Strings.Cancel;
                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                ConfigurationManager.OnDialogOpen();
                var result = await this.ShowMessageAsync(Strings.ConfirmClose,
                    Strings.ConfirmCloseMessage, MessageDialogStyle.AffirmativeAndNegative,
                    settings);
                ConfigurationManager.OnDialogClose();

                if (result != MessageDialogResult.Affirmative)
                    return;

                _isClosing = true;
                Close();

                return;
            }
        }

        // Unregister HotKeys
        if (_registeredHotKeys.Count > 0)
            UnregisterHotKeys();

        // Dispose the notify icon to prevent errors
        _notifyIcon?.Dispose();
    }

    #endregion

    #region Application

    private void LoadApplicationList()
    {
        _isApplicationViewLoading = true;

        Applications = new CollectionViewSource
        {
            Source = SettingsManager.Current.General_ApplicationList
        }.View;

        Applications.Filter = o =>
        {
            if (o is not ApplicationInfo info)
                return false;

            if (string.IsNullOrEmpty(ApplicationSearch))
                return info.IsVisible;

            var regex = new Regex(@" |-");

            var search = regex.Replace(ApplicationSearch, "");

            // Search by TranslatedName and Name
            return info.IsVisible && (
                regex.Replace(ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, info.Name), "")
                    .IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || regex
                    .Replace(info.Name.ToString(), "").Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        };

        SettingsManager.Current.General_ApplicationList.CollectionChanged += (_, _) => Applications.Refresh();

        _isApplicationViewLoading = false;

        // Select the application
        // Set application via command line, or select the default one, fallback to the first visible one
        var applicationList = Applications.Cast<ApplicationInfo>().ToArray();
        
        if (CommandLineManager.Current.Application != ApplicationName.None)
            SelectedApplication = applicationList.FirstOrDefault(x => x.Name == CommandLineManager.Current.Application);
        else
            SelectedApplication = applicationList.FirstOrDefault(x => x.IsDefault) ??
                                  applicationList.FirstOrDefault(x => x.IsVisible);

        // Scroll into view
        if (SelectedApplication != null)
            ListViewApplication.ScrollIntoView(SelectedApplication);

        // Expand application view
        ApplicationViewIsExpanded = SettingsManager.Current.ExpandApplicationView;
    }

    private DashboardView _dashboardView;
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
    private AWSSessionManagerHostView _awsSessionManagerHostView;
    private TigerVNCHostView _tigerVNCHostView;
    private WebConsoleHostView _webConsoleHostView;
    private SNMPHostView _snmpHostView;
    private SNTPLookupHostView _sntpLookupHostView;
    private DiscoveryProtocolView _discoveryProtocolView;
    private WakeOnLANView _wakeOnLanView;
    private SubnetCalculatorHostView _subnetCalculatorHostView;
    private BitCalculatorView _bitCalculatorView;
    private LookupHostView _lookupHostView;
    private WhoisHostView _whoisHostView;
    private IPGeolocationHostView _ipGeolocationHostView;
    private ConnectionsView _connectionsView;
    private ListenersView _listenersView;
    private ARPTableView _arpTableView;


    /// <summary>
    ///     Method when the application view becomes visible (again). Either when switching the applications
    ///     or after opening and closing the settings.
    /// </summary>
    /// <param name="name">Name of the application</param>
    /// <param name="fromSettings">Indicates whether the settings were previously open</param>
    private void OnApplicationViewVisible(ApplicationName name, bool fromSettings = false)
    {
        switch (name)
        {
            case ApplicationName.Dashboard:
                if (_dashboardView == null)
                    _dashboardView = new DashboardView();
                else
                    _dashboardView.OnViewVisible();

                ContentControlApplication.Content = _dashboardView;
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
            case ApplicationName.AWSSessionManager:
                if (_awsSessionManagerHostView == null)
                    _awsSessionManagerHostView = new AWSSessionManagerHostView();
                else
                    _awsSessionManagerHostView.OnViewVisible(fromSettings);

                ContentControlApplication.Content = _awsSessionManagerHostView;
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
            case ApplicationName.SNTPLookup:
                if (_sntpLookupHostView == null)
                    _sntpLookupHostView = new SNTPLookupHostView();
                else
                    _sntpLookupHostView.OnViewVisible();

                ContentControlApplication.Content = _sntpLookupHostView;
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
            case ApplicationName.IPGeolocation:
                if (_ipGeolocationHostView == null)
                    _ipGeolocationHostView = new IPGeolocationHostView();
                else
                    _ipGeolocationHostView.OnViewVisible();

                ContentControlApplication.Content = _ipGeolocationHostView;
                break;
            case ApplicationName.SubnetCalculator:
                if (_subnetCalculatorHostView == null)
                    _subnetCalculatorHostView = new SubnetCalculatorHostView();
                else
                    _subnetCalculatorHostView.OnViewVisible();

                ContentControlApplication.Content = _subnetCalculatorHostView;
                break;
            case ApplicationName.BitCalculator:
                if (_bitCalculatorView == null)
                    _bitCalculatorView = new BitCalculatorView();
                else
                    _bitCalculatorView.OnViewVisible();

                ContentControlApplication.Content = _bitCalculatorView;
                break;
            case ApplicationName.Lookup:
                if (_lookupHostView == null)
                    _lookupHostView = new LookupHostView();
                else
                    _lookupHostView.OnViewVisible();

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
        }
    }

    private void OnApplicationViewHide(ApplicationName name)
    {
        switch (name)
        {
            case ApplicationName.Dashboard:
                _dashboardView?.OnViewHide();
                break;
            case ApplicationName.NetworkInterface:
                _networkInterfaceView?.OnViewHide();
                break;
            case ApplicationName.WiFi:
                _wiFiView?.OnViewHide();
                break;
            case ApplicationName.IPScanner:
                _ipScannerHostView?.OnViewHide();
                break;
            case ApplicationName.PortScanner:
                _portScannerHostView?.OnViewHide();
                break;
            case ApplicationName.PingMonitor:
                _pingMonitorHostView?.OnViewHide();
                break;
            case ApplicationName.Traceroute:
                _tracerouteHostView?.OnViewHide();
                break;
            case ApplicationName.DNSLookup:
                _dnsLookupHostView?.OnViewHide();
                break;
            case ApplicationName.RemoteDesktop:
                _remoteDesktopHostView?.OnViewHide();
                break;
            case ApplicationName.PowerShell:
                _powerShellHostView?.OnViewHide();
                break;
            case ApplicationName.PuTTY:
                _puttyHostView?.OnViewHide();
                break;
            case ApplicationName.AWSSessionManager:
                _awsSessionManagerHostView?.OnViewHide();
                break;
            case ApplicationName.TigerVNC:
                _tigerVNCHostView?.OnViewHide();
                break;
            case ApplicationName.WebConsole:
                _webConsoleHostView?.OnViewHide();
                break;
            case ApplicationName.SNMP:
                _snmpHostView?.OnViewHide();
                break;
            case ApplicationName.SNTPLookup:
                _sntpLookupHostView?.OnViewHide();
                break;
            case ApplicationName.DiscoveryProtocol:
                _discoveryProtocolView?.OnViewHide();
                break;
            case ApplicationName.WakeOnLAN:
                _wakeOnLanView?.OnViewHide();
                break;
            case ApplicationName.Whois:
                _whoisHostView?.OnViewHide();
                break;
            case ApplicationName.IPGeolocation:
                _ipGeolocationHostView?.OnViewHide();
                break;
            case ApplicationName.Lookup:
                _lookupHostView?.OnViewHide();
                break;
            case ApplicationName.SubnetCalculator:
                _subnetCalculatorHostView?.OnViewHide();
                break;
            case ApplicationName.BitCalculator:
                _bitCalculatorView?.OnViewHide();
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
    }

    private void ClearSearchOnApplicationListMinimize()
    {
        if (ApplicationViewIsExpanded)
            return;

        if (ApplicationViewIsOpen && TextBoxApplicationSearchIsFocused)
            return;

        if (ApplicationViewIsOpen && ApplicationViewIsMouseOver)
            return;

        ApplicationSearch = string.Empty;

        // Scroll into view
        ListViewApplication.ScrollIntoView(SelectedApplication);
    }

    private async void EventSystem_RedirectDataToApplicationEvent(object sender, EventArgs e)
    {
        if (e is not EventSystemRedirectArgs data)
            return;

        // Try to find the application
        var application = Applications.Cast<ApplicationInfo>().FirstOrDefault(x => x.Name == data.Application);

        // Show error message if the application was not found
        if (application == null)
        {
            var settings = AppearanceManager.MetroDialog;
            settings.AffirmativeButtonText = Strings.OK;

            await this.ShowMessageAsync(Strings.Error,
                string.Format(Strings.CouldNotFindApplicationXXXMessage,
                    data.Application.ToString()));

            return;
        }

        // Change application view
        SelectedApplication = application;

        if (string.IsNullOrEmpty(data.Args))
            return;

        // Crate a new tab / perform action
        switch (data.Application)
        {
            case ApplicationName.Dashboard:
                break;
            case ApplicationName.NetworkInterface:
                break;
            case ApplicationName.WiFi:
                break;
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
            case ApplicationName.AWSSessionManager:
                break;
            case ApplicationName.TigerVNC:
                _tigerVNCHostView.AddTab(data.Args);
                break;
            case ApplicationName.WebConsole:
                break;
            case ApplicationName.SNMP:
                _snmpHostView.AddTab(data.Args);
                break;
            case ApplicationName.SNTPLookup:
                break;
            case ApplicationName.DiscoveryProtocol:
                break;
            case ApplicationName.WakeOnLAN:
                break;
            case ApplicationName.Whois:
                break;
            case ApplicationName.IPGeolocation:
                break;
            case ApplicationName.SubnetCalculator:
                break;
            case ApplicationName.BitCalculator:
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
            default:
                Log.Error($"Cannot redirect data to unknown application: {data.Application}");
                break;
        }
    }

    /// <summary>
    ///     Disable copy, cut, paste because we cannot handle it properly in the TextBoxApplicationSearch.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBoxApplicationSearch_OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        // Disable copy, cut, paste
        if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
            e.Handled = true;
    }

    #endregion

    #region Run Command

    #region Variables

    private ICollectionView _runCommands;

    public ICollectionView RunCommands
    {
        get => _runCommands;
        private set
        {
            if (value == _runCommands)
                return;

            _runCommands = value;
            OnPropertyChanged();
        }
    }

    private RunCommandInfo _selectedRunCommand;

    public RunCommandInfo SelectedRunCommand
    {
        get => _selectedRunCommand;
        set
        {
            if (value == _selectedRunCommand)
                return;

            _selectedRunCommand = value;
            OnPropertyChanged();
        }
    }

    private string _runCommandSearch;

    public string RunCommandSearch
    {
        get => _runCommandSearch;
        set
        {
            if (value == _runCommandSearch)
                return;

            _runCommandSearch = value;

            RefreshRunCommandsView();

            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand OpenRunCommand => new RelayCommand(_ => OpenRunAction());

    private void OpenRunAction()
    {
        ConfigurationManager.OnDialogOpen();

        FlyoutRunCommandAreAnimationsEnabled = true;
        FlyoutRunCommandIsOpen = true;
    }

    public ICommand RunCommandDoCommand => new RelayCommand(_ => RunCommandDoAction());

    private void RunCommandDoAction()
    {
        RunCommandDo();
    }

    public ICommand RunCommandCloseCommand => new RelayCommand(_ => RunCommandCloseAction());

    private void RunCommandCloseAction()
    {
        RunCommandFlyoutClose().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private void SetRunCommandsView(RunCommandInfo selectedRunCommand = null)
    {
        RunCommands = new CollectionViewSource { Source = RunCommandManager.GetList() }.View;

        RunCommands.Filter = o =>
        {
            if (o is not RunCommandInfo info)
                return false;

            if (string.IsNullOrEmpty(RunCommandSearch))
                return true;

            return info.TranslatedName.IndexOf(RunCommandSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Name.IndexOf(RunCommandSearch, StringComparison.OrdinalIgnoreCase) > -1;
        };

        if (selectedRunCommand != null)
            SelectedRunCommand = RunCommands.Cast<RunCommandInfo>()
                .FirstOrDefault(x => x.Name == selectedRunCommand.Name);

        SelectedRunCommand ??= RunCommands.Cast<RunCommandInfo>().FirstOrDefault();
    }

    private void RefreshRunCommandsView()
    {
        SetRunCommandsView(SelectedRunCommand);
    }

    /// <summary>
    ///     Execute the selected run command.
    /// </summary>
    private void RunCommandDo()
    {
        // Do nothing if no command is selected
        if (SelectedRunCommand == null)
            return;

        // Do the command
        switch (SelectedRunCommand.Type)
        {
            // Redirect to application
            case RunCommandType.Application:
                if (SettingsViewIsOpen)
                    CloseSettings();

                var applicationName = (ApplicationName)Enum.Parse(typeof(ApplicationName), SelectedRunCommand.Name);
                EventSystem.RedirectToApplication(applicationName, string.Empty);
                break;
            // Redirect to settings
            case RunCommandType.Setting:
                EventSystem.RedirectToSettings();
                break;
        }

        // Close the flyout
        RunCommandFlyoutClose(true).ConfigureAwait(false);
    }

    /// <summary>
    ///     Close the run command flyout and clear the search.
    /// </summary>
    private async Task RunCommandFlyoutClose(bool clearSearch = false)
    {
        if (!FlyoutRunCommandIsOpen)
            return;

        FlyoutRunCommandAreAnimationsEnabled = false;
        FlyoutRunCommandIsOpen = false;

        ConfigurationManager.OnDialogClose();

        // Clear the search
        if (clearSearch)
        {
            await Task.Delay(500); // Wait for the animation to finish
            RunCommandSearch = string.Empty;
        }
    }

    #endregion

    #region Events

    private void ListViewRunCommand_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        RunCommandDo();
    }

    private void FlyoutRunCommand_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Close flyout if the focus is lost.
        if (e.NewValue is not false)
            return;

        RunCommandFlyoutClose().ConfigureAwait(false);
    }

    #endregion

    #endregion

    #region Settings

    private void OpenSettings()
    {
        OnApplicationViewHide(SelectedApplication.Name);

        if (_settingsView == null)
        {
            _settingsView = new SettingsView();
            ContentControlSettings.Content = _settingsView;
        }
        else
        {
            _settingsView.OnViewVisible();
        }

        _settingsView.ChangeSettingsView(SelectedApplication.Name);

        // Show the view (this will hide other content)
        SettingsViewIsOpen = true;
    }

    private void EventSystem_RedirectToSettingsEvent(object sender, EventArgs e)
    {
        OpenSettings();
    }

    private void CloseSettings()
    {
        SettingsViewIsOpen = false;

        _settingsView.OnViewHide();

        // Change HotKeys
        if (SettingsManager.HotKeysChanged)
        {
            UnregisterHotKeys();
            RegisterHotKeys();

            SettingsManager.HotKeysChanged = false;
        }

        // Refresh the application view
        OnApplicationViewVisible(SelectedApplication.Name, true);
    }

    #endregion

    #region Profiles

    private void LoadProfiles()
    {
        _isProfileFilesLoading = true;
        ProfileFiles = new CollectionViewSource { Source = ProfileManager.ProfileFiles }.View;
        ProfileFiles.SortDescriptions.Add(
            new SortDescription(nameof(ProfileFileInfo.Name), ListSortDirection.Ascending));
        _isProfileFilesLoading = false;

        ProfileManager.OnLoadedProfileFileChangedEvent += ProfileManager_OnLoadedProfileFileChangedEvent;

        SelectedProfileFile = ProfileFiles.SourceCollection.Cast<ProfileFileInfo>()
            .FirstOrDefault(x => x.Name == SettingsManager.Current.Profiles_LastSelected);
        SelectedProfileFile ??= ProfileFiles.SourceCollection.Cast<ProfileFileInfo>().FirstOrDefault();
    }

    private async void LoadProfile(ProfileFileInfo info, bool showWrongPassword = false)
    {
        // Disable profile management while switching profiles
        ConfigurationManager.Current.ProfileManagerIsEnabled = false;
        ConfigurationManager.Current.ProfileManagerErrorMessage = string.Empty;

        if (info.IsEncrypted && !info.IsPasswordValid)
        {
            var customDialog = new CustomDialog
            {
                Title = Strings.UnlockProfileFile
            };

            var viewModel = new CredentialsPasswordProfileFileViewModel(async instance =>
            {
                await this.HideMetroDialogAsync(customDialog);
                ConfigurationManager.OnDialogClose();

                info.Password = instance.Password;

                SwitchProfile(info);
            }, async _ =>
            {
                await this.HideMetroDialogAsync(customDialog);
                ConfigurationManager.OnDialogClose();

                ProfileManager.Unload();
            }, info.Name, showWrongPassword);

            customDialog.Content = new CredentialsPasswordProfileFileDialog
            {
                DataContext = viewModel
            };

            ConfigurationManager.OnDialogOpen();
            await this.ShowMetroDialogAsync(customDialog);
        }
        else
        {
            SwitchProfile(info);
        }
    }

    private async void SwitchProfile(ProfileFileInfo info)
    {
        try
        {
            ProfileManager.Switch(info);

            // Enable profile management after successfully loading the profiles
            ConfigurationManager.Current.ProfileManagerShowUnlock = false;
            ConfigurationManager.Current.ProfileManagerIsEnabled = true;

            OnProfilesLoaded(SelectedApplication.Name);
        }
        catch (CryptographicException)
        {
            // Wrong password, try again...
            LoadProfile(info, true);
        }
        catch (Exception ex)
        {
            ConfigurationManager.Current.ProfileManagerErrorMessage =
                Strings.ProfileFileCouldNotBeLoaded;

            var settings = AppearanceManager.MetroDialog;
            settings.AffirmativeButtonText = Strings.OK;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            ConfigurationManager.OnDialogOpen();
            await this.ShowMessageAsync(Strings.ProfileFileCouldNotBeLoaded,
                string.Format(Strings.ProfileFileCouldNotBeLoadedMessage, ex.Message),
                MessageDialogStyle.Affirmative, settings);
            ConfigurationManager.OnDialogClose();
        }
    }

    private void OnProfilesLoaded(ApplicationName name)
    {
        switch (name)
        {
            case ApplicationName.AWSSessionManager:
                _awsSessionManagerHostView?.OnProfileLoaded();
                break;
        }
    }

    /// <summary>
    ///     Update the view when the loaded profile file changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ProfileManager_OnLoadedProfileFileChangedEvent(object sender, ProfileFileInfoArgs e)
    {
        _isProfileFileUpdating = e.ProfileFileUpdating;

        SelectedProfileFile = ProfileFiles.SourceCollection.Cast<ProfileFileInfo>()
            .FirstOrDefault(x => x.Equals(e.ProfileFileInfo));

        _isProfileFileUpdating = false;
    }

    #endregion

    #region Update check

    private void CheckForUpdates()
    {
        var updater = new Updater();
        updater.UpdateAvailable += Updater_UpdateAvailable;
        updater.CheckOnGitHub(Properties.Resources.NETworkManager_GitHub_User,
            Properties.Resources.NETworkManager_GitHub_Repo, AssemblyManager.Current.Version,
            SettingsManager.Current.Update_CheckForPreReleases);
    }

    private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
    {
        UpdateReleaseUrl = e.Release.Prerelease
            ? e.Release.HtmlUrl
            : Properties.Resources.NETworkManager_LatestReleaseUrl;
        IsUpdateAvailable = true;
    }

    #endregion

    #region Handle WndProc messages (Single instance, handle HotKeys)

    private HwndSource _hwndSource;

    // This is called after MainWindow() and before OnContentRendered() --> to register hotkeys...
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        _hwndSource?.AddHook(HwndHook);

        RegisterHotKeys();
    }

    [DebuggerStepThrough]
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // Single instance or Hotkey --> Show window
        if (msg == SingleInstance.WM_SHOWME || (msg == WmHotkey && wParam.ToInt32() == 1))
        {
            ShowWindow();
            handled = true;
        }

        return IntPtr.Zero;
    }

    #endregion

    #region Global HotKeys

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // WM_HOTKEY
    private const int WmHotkey = 0x0312;

    /* ID | Command
     *  ---|-------------------
     *  1  | ShowWindow()
     */

    private readonly List<int> _registeredHotKeys = new();

    private void RegisterHotKeys()
    {
        if (SettingsManager.Current.HotKey_ShowWindowEnabled)
        {
            RegisterHotKey(new WindowInteropHelper(this).Handle, 1, SettingsManager.Current.HotKey_ShowWindowModifier,
                SettingsManager.Current.HotKey_ShowWindowKey);
            _registeredHotKeys.Add(1);
        }
    }

    private void UnregisterHotKeys()
    {
        // Unregister all registered keys
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
        using (var iconStream = Application
                   .GetResourceStream(new Uri("pack://application:,,,/NETworkManager.ico"))?.Stream)
        {
            if (iconStream != null)
                _notifyIcon.Icon = new Icon(iconStream);
        }

        _notifyIcon.Text = Title;
        _notifyIcon.Click += NotifyIcon_Click;
        _notifyIcon.MouseDown += NotifyIcon_MouseDown;
        _notifyIcon.Visible = true;
    }

    private void NotifyIcon_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;

        var trayMenu = (ContextMenu)FindResource("ContextMenuNotifyIcon");

        trayMenu.IsOpen = true;
    }

    private void NotifyIcon_Click(object sender, EventArgs e)
    {
        var mouse = (MouseEventArgs)e;

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

    #endregion

    #region ICommands & Actions

    public ICommand OpenStatusWindowCommand => new RelayCommand(_ => OpenStatusWindowAction());

    private void OpenStatusWindowAction()
    {
        OpenStatusWindow();
    }

    public ICommand RestartApplicationCommand => new RelayCommand(_ => RestartApplicationAction());

    private void RestartApplicationAction()
    {
        RestartApplication();
    }

    public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

    private static void OpenWebsiteAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }

    public ICommand OpenDocumentationCommand => new RelayCommand(_ => OpenDocumentationAction());

    private void OpenDocumentationAction()
    {
        DocumentationManager.OpenDocumentation(SettingsViewIsOpen
            ? _settingsView.GetDocumentationIdentifier()
            : DocumentationManager.GetIdentifierByApplicationName(SelectedApplication.Name));
    }

    public ICommand OpenApplicationListCommand => new RelayCommand(_ => OpenApplicationListAction());

    private void OpenApplicationListAction()
    {
        ApplicationViewIsOpen = true;
        TextBoxApplicationSearch.Focus();
    }

    public ICommand UnlockProfileCommand => new RelayCommand(_ => UnlockProfileAction());

    private void UnlockProfileAction()
    {
        LoadProfile(SelectedProfileFile);
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private void OpenSettingsAction()
    {
        OpenSettings();
    }

    public ICommand OpenSettingsFromTrayCommand => new RelayCommand(_ => OpenSettingsFromTrayAction());

    private void OpenSettingsFromTrayAction()
    {
        // Bring window to front
        ShowWindow();

        OpenSettings();
    }

    public ICommand CloseSettingsCommand => new RelayCommand(_ => CloseSettingsAction());

    private void CloseSettingsAction()
    {
        CloseSettings();
    }

    public ICommand ShowWindowCommand => new RelayCommand(_ => ShowWindowAction());

    private void ShowWindowAction()
    {
        ShowWindow();
    }

    public ICommand CloseApplicationCommand => new RelayCommand(_ => CloseApplicationAction());

    private void CloseApplicationAction()
    {
        CloseApplication();
    }

    private void CloseApplication()
    {
        _isClosing = true;

        // Make it thread safe when it's called inside a dialog
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Close));
    }

    public void RestartApplication(bool asAdmin = false)
    {
        ExternalProcessStarter.RunProcess(ConfigurationManager.Current.ApplicationFullName,
            $"{CommandLineManager.GetParameterWithSplitIdentifier(CommandLineManager.ParameterRestartPid)}{Environment.ProcessId} {CommandLineManager.GetParameterWithSplitIdentifier(CommandLineManager.ParameterApplication)}{SelectedApplication.Name}",
            asAdmin);

        CloseApplication();
    }

    public ICommand ApplicationListMouseEnterCommand => new RelayCommand(_ => ApplicationListMouseEnterAction());

    private void ApplicationListMouseEnterAction()
    {
        ApplicationViewIsMouseOver = true;
    }

    public ICommand ApplicationListMouseLeaveCommand => new RelayCommand(_ => ApplicationListMouseLeaveAction());

    private void ApplicationListMouseLeaveAction()
    {
        // Don't minimize the list, if the user has accidentally moved the mouse while searching
        if (!TextBoxApplicationSearchIsFocused)
            ApplicationViewIsOpen = false;

        ApplicationViewIsMouseOver = false;
    }

    public ICommand TextBoxSearchGotFocusCommand => new RelayCommand(_ => TextBoxSearchGotFocusAction());

    private void TextBoxSearchGotFocusAction()
    {
        TextBoxApplicationSearchIsFocused = true;
    }

    public ICommand TextBoxSearchLostFocusCommand => new RelayCommand(_ => TextBoxSearchLostFocusAction());

    private void TextBoxSearchLostFocusAction()
    {
        if (!ApplicationViewIsMouseOver)
            ApplicationViewIsOpen = false;

        TextBoxApplicationSearchIsFocused = false;
    }

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        ApplicationSearch = string.Empty;
    }

    #endregion

    #region Methods

    private void ShowWindow()
    {
        if (_isInTray)
            ShowWindowFromTray();

        if (!IsActive)
            BringWindowToFront();
    }

    private void HideWindowToTray()
    {
        if (_notifyIcon == null)
            InitNotifyIcon();

        _isInTray = true;

        if (_notifyIcon != null)
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

    private void ConfigureDNS()
    {
        Log.Info("Configure application DNS...");

        DNSClientSettings dnsSettings = new();

        if (SettingsManager.Current.Network_UseCustomDNSServer)
        {
            if (!string.IsNullOrEmpty(SettingsManager.Current.Network_CustomDNSServer))
            {
                Log.Info($"Use custom DNS servers ({SettingsManager.Current.Network_CustomDNSServer})...");

                List<(string Server, int Port)> dnsServers = new();

                foreach (var dnsServer in SettingsManager.Current.Network_CustomDNSServer.Split(";"))
                    dnsServers.Add((dnsServer, 53));

                dnsSettings.UseCustomDNSServers = true;
                dnsSettings.DNSServers = dnsServers;
            }
            else
            {
                Log.Info(
                    $"Custom DNS servers could not be set (Setting \"{nameof(SettingsManager.Current.Network_CustomDNSServer)}\" has value \"{SettingsManager.Current.Network_CustomDNSServer}\")! Fallback to Windows DNS servers...");
            }
        }
        else
        {
            Log.Info("Use Windows DNS servers...");
        }

        DNSClient.GetInstance().Configure(dnsSettings);
    }

    private void UpdateDNS()
    {
        Log.Info("Update Windows DNS servers...");

        DNSClient.GetInstance().UpdateFromWindows();
    }

    private void WriteDefaultPowerShellProfileToRegistry()
    {
        if (!SettingsManager.Current.Appearance_PowerShellModifyGlobalProfile)
            return;

        HashSet<string> paths = new();

        // PowerShell
        if (!string.IsNullOrEmpty(SettingsManager.Current.PowerShell_ApplicationFilePath) &&
            File.Exists(SettingsManager.Current.PowerShell_ApplicationFilePath))
            paths.Add(SettingsManager.Current.PowerShell_ApplicationFilePath);

        // AWS Session Manager
        if (!string.IsNullOrEmpty(SettingsManager.Current.AWSSessionManager_ApplicationFilePath) &&
            File.Exists(SettingsManager.Current.AWSSessionManager_ApplicationFilePath))
            paths.Add(SettingsManager.Current.AWSSessionManager_ApplicationFilePath);

        foreach (var path in paths)
            PowerShell.WriteDefaultProfileToRegistry(SettingsManager.Current.Appearance_Theme, path);
    }

    #endregion

    #region Status window

    private void OpenStatusWindow(bool enableCloseTimer = false)
    {
        _statusWindow.ShowWindow(enableCloseTimer);
    }

    private async void OnNetworkHasChanged()
    {
        if (_isNetworkChanging)
            return;

        _isNetworkChanging = true;

        // Wait, because the event may be triggered several times.
        await Task.Delay(GlobalStaticConfiguration.StatusWindowDelayBeforeOpen);

        Log.Info("Network availability or address has changed!");

        // Update DNS server if network changed
        if (!SettingsManager.Current.Network_UseCustomDNSServer)
            UpdateDNS();

        // Show status window on network change
        if (SettingsManager.Current.Status_ShowWindowOnNetworkChange)
            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate { OpenStatusWindow(true); }));

        _isNetworkChanging = false;
    }

    #endregion

    #region Focus embedded window

    private void MetroMainWindow_Activated(object sender, EventArgs e)
    {
        FocusEmbeddedWindow();
    }

    private async void FocusEmbeddedWindow()
    {
        // Delay the focus to prevent blocking the ui
        // Detect if window is resizing
        do
        {
            await Task.Delay(250);
        } while (Control.MouseButtons == MouseButtons.Left);

        /* Don't continue if
           - Application is not set
           - Settings are opened
           - Profile file DropDown is opened
           - Application search TextBox is opened
           - Dialog over an embedded window is opened (FixAirspace)
        */
        if (SelectedApplication == null || SettingsViewIsOpen || IsProfileFileDropDownOpened ||
            TextBoxApplicationSearchIsFocused ||
            ConfigurationManager.Current.FixAirspace)
            return;

        // Switch by name
        switch (SelectedApplication.Name)
        {
            case ApplicationName.PowerShell:
                _powerShellHostView?.FocusEmbeddedWindow();
                break;
            case ApplicationName.PuTTY:
                _puttyHostView?.FocusEmbeddedWindow();
                break;
            case ApplicationName.AWSSessionManager:
                _awsSessionManagerHostView?.FocusEmbeddedWindow();
                break;
        }
    }

    #endregion
}