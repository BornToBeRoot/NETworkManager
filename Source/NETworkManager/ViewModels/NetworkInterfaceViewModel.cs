using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using NetworkInterface = NETworkManager.Models.Network.NetworkInterface;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Network Interface feature, allowing management of network adapters.
/// </summary>
public class NetworkInterfaceViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkInterfaceViewModel));

    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private bool _searchDisabled;
    private BandwidthMeter _bandwidthMeter;

    private readonly bool _isLoading;
    private bool _isViewActive = true;

    private bool _isNetworkInterfaceLoading;

    /// <summary>
    /// Gets or sets a value indicating whether network interfaces are currently loading.
    /// </summary>
    public bool IsNetworkInterfaceLoading
    {
        get => _isNetworkInterfaceLoading;
        set
        {
            if (value == _isNetworkInterfaceLoading)
                return;

            _isNetworkInterfaceLoading = value;
            OnPropertyChanged();
        }
    }

    private bool _canConfigure;

    /// <summary>
    /// Gets or sets a value indicating whether configuration is allowed.
    /// </summary>
    public bool CanConfigure
    {
        get => _canConfigure;
        set
        {
            if (value == _canConfigure)
                return;

            _canConfigure = value;
            OnPropertyChanged();
        }
    }

    private bool _isConfigurationRunning;

    /// <summary>
    /// Gets or sets a value indicating whether a configuration operation is running.
    /// </summary>
    public bool IsConfigurationRunning
    {
        get => _isConfigurationRunning;
        set
        {
            if (value == _isConfigurationRunning)
                return;

            _isConfigurationRunning = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusMessageDisplayed;

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get => _isStatusMessageDisplayed;
        set
        {
            if (value == _isStatusMessageDisplayed)
                return;

            _isStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage;

    /// <summary>
    /// Gets the status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #region NetworkInterfaces, SelectedNetworkInterface

    private ObservableCollection<NetworkInterfaceInfo> _networkInterfaces = [];

    /// <summary>
    /// Gets the collection of network interfaces.
    /// </summary>
    public ObservableCollection<NetworkInterfaceInfo> NetworkInterfaces
    {
        get => _networkInterfaces;
        private set
        {
            if (value == _networkInterfaces)
                return;

            _networkInterfaces = value;
            OnPropertyChanged();
        }
    }

    private NetworkInterfaceInfo _selectedNetworkInterface;

    /// <summary>
    /// Gets or sets the currently selected network interface.
    /// </summary>
    public NetworkInterfaceInfo SelectedNetworkInterface
    {
        get => _selectedNetworkInterface;
        set
        {
            if (value == _selectedNetworkInterface)
                return;

            if (value != null)
            {
                if (!_isLoading)
                    SettingsManager.Current.NetworkInterface_InterfaceId = value.Id;

                // Bandwidth
                StopBandwidthMeter();
                StartBandwidthMeter(value.Id);

                // Configuration
                SetConfigurationDefaults(value);

                CanConfigure = value.IsOperational;
            }

            _selectedNetworkInterface = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Bandwidth

    private long _bandwidthTotalBytesSentTemp;

    private long _bandwidthTotalBytesSent;

    /// <summary>
    /// Gets or sets the total bytes sent.
    /// </summary>
    public long BandwidthTotalBytesSent
    {
        get => _bandwidthTotalBytesSent;
        set
        {
            if (value == _bandwidthTotalBytesSent)
                return;

            _bandwidthTotalBytesSent = value;
            OnPropertyChanged();
        }
    }

    private long _bandwidthTotalBytesReceivedTemp;
    private long _bandwidthTotalBytesReceived;

    /// <summary>
    /// Gets or sets the total bytes received.
    /// </summary>
    public long BandwidthTotalBytesReceived
    {
        get => _bandwidthTotalBytesReceived;
        set
        {
            if (value == _bandwidthTotalBytesReceived)
                return;

            _bandwidthTotalBytesReceived = value;
            OnPropertyChanged();
        }
    }

    private long _bandwidthDiffBytesSent;

    /// <summary>
    /// Gets or sets the difference in bytes sent.
    /// </summary>
    public long BandwidthDiffBytesSent
    {
        get => _bandwidthDiffBytesSent;
        set
        {
            if (value == _bandwidthDiffBytesSent)
                return;

            _bandwidthDiffBytesSent = value;
            OnPropertyChanged();
        }
    }

    private long _bandwidthDiffBytesReceived;

    /// <summary>
    /// Gets or sets the difference in bytes received.
    /// </summary>
    public long BandwidthDiffBytesReceived
    {
        get => _bandwidthDiffBytesReceived;
        set
        {
            if (value == _bandwidthDiffBytesReceived)
                return;

            _bandwidthDiffBytesReceived = value;
            OnPropertyChanged();
        }
    }

    private long _bandwidthBytesReceivedSpeed;

    /// <summary>
    /// Gets or sets the speed of bytes received.
    /// </summary>
    public long BandwidthBytesReceivedSpeed
    {
        get => _bandwidthBytesReceivedSpeed;
        set
        {
            if (value == _bandwidthBytesReceivedSpeed)
                return;

            _bandwidthBytesReceivedSpeed = value;
            OnPropertyChanged();
        }
    }

    private long _bandwidthBytesSentSpeed;

    /// <summary>
    /// Gets or sets the speed of bytes sent.
    /// </summary>
    public long BandwidthBytesSentSpeed
    {
        get => _bandwidthBytesSentSpeed;
        set
        {
            if (value == _bandwidthBytesSentSpeed)
                return;

            _bandwidthBytesSentSpeed = value;
            OnPropertyChanged();
        }
    }

    private DateTime _bandwidthStartTime;

    /// <summary>
    /// Gets or sets the start time of the bandwidth measurement.
    /// </summary>
    public DateTime BandwidthStartTime
    {
        get => _bandwidthStartTime;
        set
        {
            if (value == _bandwidthStartTime)
                return;

            _bandwidthStartTime = value;
            OnPropertyChanged();
        }
    }

    private TimeSpan _bandwidthMeasuredTime;

    /// <summary>
    /// Gets or sets the duration of the bandwidth measurement.
    /// </summary>
    public TimeSpan BandwidthMeasuredTime
    {
        get => _bandwidthMeasuredTime;
        set
        {
            if (value == _bandwidthMeasuredTime)
                return;

            _bandwidthMeasuredTime = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Config

    private bool _configEnableDynamicIPAddress = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable dynamic IP address (DHCP).
    /// </summary>
    public bool ConfigEnableDynamicIPAddress
    {
        get => _configEnableDynamicIPAddress;
        set
        {
            if (value == _configEnableDynamicIPAddress)
                return;

            _configEnableDynamicIPAddress = value;
            OnPropertyChanged();
        }
    }

    private bool _configEnableStaticIPAddress;

    /// <summary>
    /// Gets or sets a value indicating whether to enable static IP address.
    /// </summary>
    public bool ConfigEnableStaticIPAddress
    {
        get => _configEnableStaticIPAddress;
        set
        {
            if (value == _configEnableStaticIPAddress)
                return;

            ConfigEnableStaticDNS = true;

            _configEnableStaticIPAddress = value;
            OnPropertyChanged();
        }
    }

    private string _configIPAddress;

    /// <summary>
    /// Gets or sets the static IP address.
    /// </summary>
    public string ConfigIPAddress
    {
        get => _configIPAddress;
        set
        {
            if (value == _configIPAddress)
                return;

            _configIPAddress = value;
            OnPropertyChanged();
        }
    }

    private string _configSubnetmask;

    /// <summary>
    /// Gets or sets the subnet mask.
    /// </summary>
    public string ConfigSubnetmask
    {
        get => _configSubnetmask;
        set
        {
            if (value == _configSubnetmask)
                return;

            _configSubnetmask = value;
            OnPropertyChanged();
        }
    }

    private string _configGateway;

    /// <summary>
    /// Gets or sets the default gateway.
    /// </summary>
    public string ConfigGateway
    {
        get => _configGateway;
        set
        {
            if (value == _configGateway)
                return;

            _configGateway = value;
            OnPropertyChanged();
        }
    }

    private bool _configEnableDynamicDNS = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable dynamic DNS (DHCP).
    /// </summary>
    public bool ConfigEnableDynamicDNS
    {
        get => _configEnableDynamicDNS;
        set
        {
            if (value == _configEnableDynamicDNS)
                return;

            _configEnableDynamicDNS = value;
            OnPropertyChanged();
        }
    }

    private bool _configEnableStaticDNS;

    /// <summary>
    /// Gets or sets a value indicating whether to enable static DNS.
    /// </summary>
    public bool ConfigEnableStaticDNS
    {
        get => _configEnableStaticDNS;
        set
        {
            if (value == _configEnableStaticDNS)
                return;

            _configEnableStaticDNS = value;
            OnPropertyChanged();
        }
    }

    private string _configPrimaryDNSServer;

    /// <summary>
    /// Gets or sets the primary DNS server.
    /// </summary>
    public string ConfigPrimaryDNSServer
    {
        get => _configPrimaryDNSServer;
        set
        {
            if (value == _configPrimaryDNSServer)
                return;

            _configPrimaryDNSServer = value;
            OnPropertyChanged();
        }
    }

    private string _configSecondaryDNSServer;

    /// <summary>
    /// Gets or sets the secondary DNS server.
    /// </summary>
    public string ConfigSecondaryDNSServer
    {
        get => _configSecondaryDNSServer;
        set
        {
            if (value == _configSecondaryDNSServer)
                return;

            _configSecondaryDNSServer = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Profiles

    private ICollectionView _profiles;

    /// <summary>
    /// Gets the collection of profiles.
    /// </summary>
    public ICollectionView Profiles
    {
        get => _profiles;
        private set
        {
            if (value == _profiles)
                return;

            _profiles = value;
            OnPropertyChanged();
        }
    }

    private ProfileInfo _selectedProfile = new();

    /// <summary>
    /// Gets or sets the selected profile.
    /// </summary>
    public ProfileInfo SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (value == _selectedProfile)
                return;

            if (value != null)
            {
                ConfigEnableDynamicIPAddress = !value.NetworkInterface_EnableStaticIPAddress;
                ConfigEnableStaticIPAddress = value.NetworkInterface_EnableStaticIPAddress;
                ConfigIPAddress = value.NetworkInterface_IPAddress;
                ConfigGateway = value.NetworkInterface_Gateway;
                ConfigSubnetmask = value.NetworkInterface_Subnetmask;
                ConfigEnableDynamicDNS = !value.NetworkInterface_EnableStaticDNS;
                ConfigEnableStaticDNS = value.NetworkInterface_EnableStaticDNS;
                ConfigPrimaryDNSServer = value.NetworkInterface_PrimaryDNSServer;
                ConfigSecondaryDNSServer = value.NetworkInterface_SecondaryDNSServer;
            }

            _selectedProfile = value;
            OnPropertyChanged();
        }
    }

    private string _search;

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            // Start searching...
            if (!_searchDisabled)
            {
                IsSearching = true;
                _searchDispatcherTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    private bool _isSearching;

    /// <summary>
    /// Gets or sets a value indicating whether a search is in progress.
    /// </summary>
    public bool IsSearching
    {
        get => _isSearching;
        set
        {
            if (value == _isSearching)
                return;

            _isSearching = value;
            OnPropertyChanged();
        }
    }

    private bool _profileFilterIsOpen;

    /// <summary>
    /// Gets or sets a value indicating whether the profile filter is open.
    /// </summary>
    public bool ProfileFilterIsOpen
    {
        get => _profileFilterIsOpen;
        set
        {
            if (value == _profileFilterIsOpen)
                return;

            _profileFilterIsOpen = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the view for profile filter tags.
    /// </summary>
    public ICollectionView ProfileFilterTagsView { get; set; }

    /// <summary>
    /// Gets or sets the collection of profile filter tags.
    /// </summary>
    public ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; set; } = [];

    private bool _profileFilterTagsMatchAny = GlobalStaticConfiguration.Profile_TagsMatchAny;

    /// <summary>
    /// Gets or sets a value indicating whether to match any profile filter tag.
    /// </summary>
    public bool ProfileFilterTagsMatchAny
    {
        get => _profileFilterTagsMatchAny;
        set
        {
            if (value == _profileFilterTagsMatchAny)
                return;

            _profileFilterTagsMatchAny = value;
            OnPropertyChanged();
        }
    }

    private bool _profileFilterTagsMatchAll;

    /// <summary>
    /// Gets or sets a value indicating whether to match all profile filter tags.
    /// </summary>
    public bool ProfileFilterTagsMatchAll
    {
        get => _profileFilterTagsMatchAll;
        set
        {
            if (value == _profileFilterTagsMatchAll)
                return;

            _profileFilterTagsMatchAll = value;
            OnPropertyChanged();
        }
    }

    private bool _isProfileFilterSet;

    /// <summary>
    /// Gets or sets a value indicating whether a profile filter is set.
    /// </summary>
    public bool IsProfileFilterSet
    {
        get => _isProfileFilterSet;
        set
        {
            if (value == _isProfileFilterSet)
                return;

            _isProfileFilterSet = value;
            OnPropertyChanged();
        }
    }

    private readonly GroupExpanderStateStore _groupExpanderStateStore = new();

    /// <summary>
    /// Gets the store for group expander states.
    /// </summary>
    public GroupExpanderStateStore GroupExpanderStateStore => _groupExpanderStateStore;

    private bool _canProfileWidthChange = true;
    private double _tempProfileWidth;

    private bool _expandProfileView;

    /// <summary>
    /// Gets or sets a value indicating whether to expand the profile view.
    /// </summary>
    public bool ExpandProfileView
    {
        get => _expandProfileView;
        set
        {
            if (value == _expandProfileView)
                return;

            if (!_isLoading)
                SettingsManager.Current.NetworkInterface_ExpandProfileView = value;

            _expandProfileView = value;

            if (_canProfileWidthChange)
                ResizeProfile(false);

            OnPropertyChanged();
        }
    }

    private GridLength _profileWidth;

    /// <summary>
    /// Gets or sets the width of the profile view.
    /// </summary>
    public GridLength ProfileWidth
    {
        get => _profileWidth;
        set
        {
            if (value == _profileWidth)
                return;

            if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                GlobalStaticConfiguration.Profile_FloatPointFix) // Do not save the size when collapsed
                SettingsManager.Current.NetworkInterface_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor, LoadSettings, OnShutdown

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkInterfaceViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public NetworkInterfaceViewModel()
    {
        _isLoading = true;

        LoadNetworkInterfaces().ConfigureAwait(false);

        InitialBandwidthChart();

        // Profiles
        CreateTags();

        ProfileFilterTagsView = CollectionViewSource.GetDefaultView(ProfileFilterTags);
        ProfileFilterTagsView.SortDescriptions.Add(new SortDescription(nameof(ProfileFilterTagsInfo.Name),
            ListSortDirection.Ascending));

        SetProfilesView(new ProfileFilterInfo());

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        // Detect if network address or status changed...
        NetworkChange.NetworkAvailabilityChanged += (_, _) => ReloadNetworkInterfaces();
        NetworkChange.NetworkAddressChanged += (_, _) => ReloadNetworkInterfaces();

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Initializes the bandwidth chart configuration.
    /// </summary>
    private void InitialBandwidthChart()
    {
        var dayConfig = Mappers.Xy<LvlChartsDefaultInfo>()
            .X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
            .Y(dayModel => dayModel.Value);

        Series = new SeriesCollection(dayConfig)
        {
            new LineSeries
            {
                Title = "Download",
                Values = new ChartValues<LvlChartsDefaultInfo>(),
                PointGeometry = null
            },
            new LineSeries
            {
                Title = "Upload",
                Values = new ChartValues<LvlChartsDefaultInfo>(),
                PointGeometry = null
            }
        };

        FormatterDate = value =>
            DateTimeHelper.DateTimeToTimeString(new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)));
        FormatterSpeed = value => $"{FileSizeConverter.GetBytesReadable((long)value * 8)}it/s";
    }

    public Func<double, string> FormatterDate { get; set; }
    public Func<double, string> FormatterSpeed { get; set; }
    public SeriesCollection Series { get; set; }

    /// <summary>
    /// Loads the network interfaces.
    /// </summary>
    private async Task LoadNetworkInterfaces()
    {
        IsNetworkInterfaceLoading = true;

        // Get all network interfaces...
        (await NetworkInterface.GetNetworkInterfacesAsync()).ForEach(NetworkInterfaces.Add);

        // Get the last selected interface, if it is still available on this machine...
        if (NetworkInterfaces.Count > 0)
        {
            var info = NetworkInterfaces.FirstOrDefault(s =>
                s.Id == SettingsManager.Current.NetworkInterface_InterfaceId);

            SelectedNetworkInterface = info ?? NetworkInterfaces[0];
        }

        IsNetworkInterfaceLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.NetworkInterface_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.NetworkInterface_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.NetworkInterface_ProfileWidth;
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to reload the network interfaces.
    /// </summary>
    public ICommand ReloadNetworkInterfacesCommand =>
        new RelayCommand(_ => ReloadNetworkInterfacesAction(), ReloadNetworkInterfaces_CanExecute);

    /// <summary>
    /// Determines whether the ReloadNetworkInterfaces command can execute.
    /// </summary>
    private bool ReloadNetworkInterfaces_CanExecute(object obj)
    {
        return !IsNetworkInterfaceLoading &&
               Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to reload the network interfaces.
    /// </summary>
    private void ReloadNetworkInterfacesAction()
    {
        ReloadNetworkInterfaces();
    }

    /// <summary>
    /// Gets the command to export the network interfaces.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    /// <summary>
    /// Action to export the network interfaces.
    /// </summary>
    private Task ExportAction()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        instance.ExportAll ? NetworkInterfaces : [SelectedNetworkInterface]);
                }
                catch (Exception ex)
                {
                    Log.Error("Error while exporting data as " + instance.FileType, ex);

                    await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, ChildWindowIcon.Error);
                }

                SettingsManager.Current.NetworkInterface_ExportFileType = instance.FileType;
                SettingsManager.Current.NetworkInterface_ExportFilePath = instance.FilePath;
            }, _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            }, [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], true,
            SettingsManager.Current.NetworkInterface_ExportFileType,
            SettingsManager.Current.NetworkInterface_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to apply the network configuration.
    /// </summary>
    public ICommand ApplyConfigurationCommand =>
        new RelayCommand(_ => ApplyConfigurationAction(), ApplyConfiguration_CanExecute);

    /// <summary>
    /// Determines whether the ApplyConfiguration command can execute.
    /// </summary>
    private bool ApplyConfiguration_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to apply the network configuration.
    /// </summary>
    private void ApplyConfigurationAction()
    {
        ApplyConfiguration().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to apply the profile configuration.
    /// </summary>
    public ICommand ApplyProfileConfigCommand => new RelayCommand(_ => ApplyProfileProfileAction());

    private void ApplyProfileProfileAction()
    {
        ApplyConfigurationFromProfile().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to add a new profile.
    /// </summary>
    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.NetworkInterface)
            .ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    /// <summary>
    /// Gets the command to edit the selected profile.
    /// </summary>
    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to copy the selected profile as a new profile.
    /// </summary>
    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to delete the selected profile.
    /// </summary>
    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(Application.Current.MainWindow, this, new List<ProfileInfo> { SelectedProfile })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to edit a profile group.
    /// </summary>
    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager
            .ShowEditGroupDialog(Application.Current.MainWindow, this, ProfileManager.GetGroupByName($"{group}"))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to open the profile filter.
    /// </summary>
    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    private void OpenProfileFilterAction()
    {
        ProfileFilterIsOpen = true;
    }

    /// <summary>
    /// Gets the command to apply the profile filter.
    /// </summary>
    public ICommand ApplyProfileFilterCommand => new RelayCommand(_ => ApplyProfileFilterAction());

    private void ApplyProfileFilterAction()
    {
        RefreshProfiles();

        ProfileFilterIsOpen = false;
    }

    /// <summary>
    /// Gets the command to clear the profile filter.
    /// </summary>
    public ICommand ClearProfileFilterCommand => new RelayCommand(_ => ClearProfileFilterAction());

    private void ClearProfileFilterAction()
    {
        _searchDisabled = true;
        Search = string.Empty;
        _searchDisabled = false;

        foreach (var tag in ProfileFilterTags)
            tag.IsSelected = false;

        RefreshProfiles();

        IsProfileFilterSet = false;
        ProfileFilterIsOpen = false;
    }

    /// <summary>
    /// Gets the command to expand all profile groups.
    /// </summary>
    public ICommand ExpandAllProfileGroupsCommand => new RelayCommand(_ => ExpandAllProfileGroupsAction());

    private void ExpandAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(true);
    }

    /// <summary>
    /// Gets the command to collapse all profile groups.
    /// </summary>
    public ICommand CollapseAllProfileGroupsCommand => new RelayCommand(_ => CollapseAllProfileGroupsAction());

    private void CollapseAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(false);
    }

    #region Additional commands

    private bool AdditionalCommands_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Gets the command to open network connections.
    /// </summary>
    public ICommand OpenNetworkConnectionsCommand =>
        new RelayCommand(_ => OpenNetworkConnectionsAction(), AdditionalCommands_CanExecute);

    private void OpenNetworkConnectionsAction()
    {
        OpenNetworkConnectionsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to open IP Scanner.
    /// </summary>
    public ICommand IPScannerCommand => new RelayCommand(_ => IPScannerAction(), AdditionalCommands_CanExecute);

    private void IPScannerAction()
    {
        var ipTuple = SelectedNetworkInterface?.IPv4Address.FirstOrDefault();

        // ToDo: Log error in the future
        if (ipTuple == null)
            return;

        EventSystem.RedirectToApplication(ApplicationName.IPScanner,
            $"{ipTuple.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(ipTuple.Item2)}");
    }

    /// <summary>
    /// Gets the command to flush DNS.
    /// </summary>
    public ICommand FlushDNSCommand => new RelayCommand(_ => FlushDNSAction(), AdditionalCommands_CanExecute);

    private void FlushDNSAction()
    {
        FlushDNSAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to release and renew IP address.
    /// </summary>
    public ICommand ReleaseRenewCommand => new RelayCommand(_ => ReleaseRenewAction(), AdditionalCommands_CanExecute);

    private void ReleaseRenewAction()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to release IP address.
    /// </summary>
    public ICommand ReleaseCommand => new RelayCommand(_ => ReleaseAction(), AdditionalCommands_CanExecute);

    private void ReleaseAction()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Release).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to renew IP address.
    /// </summary>
    public ICommand RenewCommand => new RelayCommand(_ => RenewAction(), AdditionalCommands_CanExecute);

    private void RenewAction()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to release and renew IPv6 address.
    /// </summary>
    public ICommand ReleaseRenew6Command => new RelayCommand(_ => ReleaseRenew6Action(), AdditionalCommands_CanExecute);

    private void ReleaseRenew6Action()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew6).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to release IPv6 address.
    /// </summary>
    public ICommand Release6Command => new RelayCommand(_ => Release6Action(), AdditionalCommands_CanExecute);

    private void Release6Action()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Release6).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to renew IPv6 address.
    /// </summary>
    public ICommand Renew6Command => new RelayCommand(_ => Renew6Action(), AdditionalCommands_CanExecute);

    private void Renew6Action()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to add an IPv4 address.
    /// </summary>
    public ICommand AddIPv4AddressCommand => new RelayCommand(_ => AddIPv4AddressAction().ConfigureAwait(false),
        AdditionalCommands_CanExecute);

    private async Task AddIPv4AddressAction()
    {
        var childWindow = new IPAddressAndSubnetmaskChildWindow();

        var childWindowViewModel = new IPAddressAndSubnetmaskViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            await AddIPv4Address(instance.IPAddress, instance.Subnetmask);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddIPv4Address;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to remove an IPv4 address.
    /// </summary>
    public ICommand RemoveIPv4AddressCommand => new RelayCommand(_ => RemoveIPv4AddressAction().ConfigureAwait(false),
        AdditionalCommands_CanExecute);

    private async Task RemoveIPv4AddressAction()
    {
        var childWindow = new DropDownChildWindow();

        var childWindowViewModel = new DropDownViewModel(async instance =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;

                await RemoveIPv4Address(instance.SelectedValue.Split("/")[0]);
            }, _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            },
                [.. SelectedNetworkInterface.IPv4Address.Select(x => $"{x.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(x.Item2)}")],
                Strings.IPv4Address
            );

        childWindow.Title = Strings.RemoveIPv4Address;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #endregion

    #region Methods

    private async void ReloadNetworkInterfaces()
    {
        // Avoid multiple reloads
        if (IsNetworkInterfaceLoading)
            return;

        IsNetworkInterfaceLoading = true;

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        // Store the last selected id
        var id = SelectedNetworkInterface?.Id ?? string.Empty;

        // Get all network interfaces...
        var networkInterfaces = await NetworkInterface.GetNetworkInterfacesAsync();

        // Invoke on UI thread synchronously
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Clear the list            
            NetworkInterfaces.Clear();

            // Add all network interfaces to the list
            networkInterfaces.ForEach(NetworkInterfaces.Add);
        });

        // Set the last selected interface, if it is still available on this machine...
        SelectedNetworkInterface = string.IsNullOrEmpty(id)
            ? NetworkInterfaces.FirstOrDefault()
            : NetworkInterfaces.FirstOrDefault(x => x.Id == id);

        IsNetworkInterfaceLoading = false;
    }

    private void SetConfigurationDefaults(NetworkInterfaceInfo info)
    {
        if (info.DhcpEnabled)
        {
            ConfigEnableDynamicIPAddress = true;
        }
        else
        {
            ConfigEnableStaticIPAddress = true;
            ConfigIPAddress = info.IPv4Address.FirstOrDefault()?.Item1.ToString();
            ConfigSubnetmask = info.IPv4Address.FirstOrDefault()?.Item2.ToString();
            ConfigGateway = info.IPv4Gateway?.Any() == true
                ? info.IPv4Gateway.FirstOrDefault()?.ToString()
                : string.Empty;
        }

        if (info.DNSAutoconfigurationEnabled)
        {
            ConfigEnableDynamicDNS = true;
        }
        else
        {
            ConfigEnableStaticDNS = true;

            var dnsServers = info.DNSServer.Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToList();
            ConfigPrimaryDNSServer = dnsServers.Count > 0 ? dnsServers[0].ToString() : string.Empty;
            ConfigSecondaryDNSServer = dnsServers.Count > 1 ? dnsServers[1].ToString() : string.Empty;
        }
    }

    private async Task ApplyConfiguration()
    {
        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        var subnetmask = ConfigSubnetmask;

        // CIDR to subnetmask
        if (ConfigEnableStaticIPAddress && subnetmask.StartsWith("/"))
            subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

        // If primary and secondary DNS are empty --> autoconfiguration
        if (ConfigEnableStaticDNS && string.IsNullOrEmpty(ConfigPrimaryDNSServer) &&
            string.IsNullOrEmpty(ConfigSecondaryDNSServer))
            ConfigEnableDynamicDNS = true;

        // When primary DNS is empty, swap it with secondary (if not empty)
        if (ConfigEnableStaticDNS && string.IsNullOrEmpty(ConfigPrimaryDNSServer) &&
            !string.IsNullOrEmpty(ConfigSecondaryDNSServer))
        {
            ConfigPrimaryDNSServer = ConfigSecondaryDNSServer;
            ConfigSecondaryDNSServer = string.Empty;
        }

        var config = new NetworkInterfaceConfig
        {
            Name = SelectedNetworkInterface.Name,
            EnableStaticIPAddress = ConfigEnableStaticIPAddress,
            IPAddress = ConfigIPAddress,
            Subnetmask = subnetmask,
            Gateway = ConfigGateway,
            EnableStaticDNS = ConfigEnableStaticDNS,
            PrimaryDNSServer = ConfigPrimaryDNSServer,
            SecondaryDNSServer = ConfigSecondaryDNSServer
        };

        try
        {
            var networkInterface = new NetworkInterface();

            networkInterface.UserHasCanceled += NetworkInterface_UserHasCanceled;

            await networkInterface.ConfigureNetworkInterfaceAsync(config);

            ReloadNetworkInterfaces();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsConfigurationRunning = false;
        }
    }

    private async Task ApplyConfigurationFromProfile()
    {
        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        var subnetmask = SelectedProfile.NetworkInterface_Subnetmask;

        // CIDR to subnetmask
        if (SelectedProfile.NetworkInterface_EnableStaticIPAddress && subnetmask.StartsWith("/"))
            subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

        var enableStaticDNS = SelectedProfile.NetworkInterface_EnableStaticDNS;

        var primaryDNSServer = SelectedProfile.NetworkInterface_PrimaryDNSServer;
        var secondaryDNSServer = SelectedProfile.NetworkInterface_SecondaryDNSServer;

        // If primary and secondary DNS are empty --> autoconfiguration
        if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && string.IsNullOrEmpty(secondaryDNSServer))
            enableStaticDNS = false;

        // When primary DNS is empty, swap it with secondary (if not empty)
        if (SelectedProfile.NetworkInterface_EnableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) &&
            !string.IsNullOrEmpty(secondaryDNSServer))
        {
            primaryDNSServer = secondaryDNSServer;
            secondaryDNSServer = string.Empty;
        }

        var config = new NetworkInterfaceConfig
        {
            Name = SelectedNetworkInterface.Name,
            EnableStaticIPAddress = SelectedProfile.NetworkInterface_EnableStaticIPAddress,
            IPAddress = SelectedProfile.NetworkInterface_IPAddress,
            Subnetmask = subnetmask,
            Gateway = SelectedProfile.NetworkInterface_Gateway,
            EnableStaticDNS = enableStaticDNS,
            PrimaryDNSServer = primaryDNSServer,
            SecondaryDNSServer = secondaryDNSServer
        };

        try
        {
            var networkInterface = new NetworkInterface();

            networkInterface.UserHasCanceled += NetworkInterface_UserHasCanceled;

            await networkInterface.ConfigureNetworkInterfaceAsync(config);

            ReloadNetworkInterfaces();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsConfigurationRunning = false;
        }
    }

    private async Task OpenNetworkConnectionsAsync()
    {
        try
        {
            ProcessStartInfo info = new()
            {
                FileName = "NCPA.cpl",
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    private async Task FlushDNSAsync()
    {
        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        await NetworkInterface.FlushDnsAsync();

        IsConfigurationRunning = false;
    }

    private async Task ReleaseRenewAsync(IPConfigReleaseRenewMode releaseRenewMode)
    {
        IsConfigurationRunning = true;

        await NetworkInterface.ReleaseRenewAsync(releaseRenewMode, SelectedNetworkInterface.Name);

        ReloadNetworkInterfaces();

        IsConfigurationRunning = false;
    }

    private async Task AddIPv4Address(string ipAddress, string subnetmaskOrCidr)
    {
        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        var subnetmask = subnetmaskOrCidr;

        // CIDR to subnetmask
        if (subnetmask.StartsWith("/"))
            subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

        var config = new NetworkInterfaceConfig
        {
            Name = SelectedNetworkInterface.Name,
            EnableDhcpStaticIpCoexistence = SelectedNetworkInterface.DhcpEnabled,
            IPAddress = ipAddress,
            Subnetmask = subnetmask
        };

        try
        {
            await NetworkInterface.AddIPAddressToNetworkInterfaceAsync(config);

            ReloadNetworkInterfaces();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsConfigurationRunning = false;
        }
    }

    private async Task RemoveIPv4Address(string ipAddress)
    {
        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        var config = new NetworkInterfaceConfig
        {
            Name = SelectedNetworkInterface.Name,
            IPAddress = ipAddress
        };

        try
        {
            await NetworkInterface.RemoveIPAddressFromNetworkInterfaceAsync(config);

            ReloadNetworkInterfaces();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsConfigurationRunning = false;
        }
    }

    private void SetIsExpandedForAllProfileGroups(bool isExpanded)
    {
        foreach (var group in Profiles.Groups.Cast<CollectionViewGroup>())
            GroupExpanderStateStore[group.Name.ToString()] = isExpanded;
    }

    private void ResizeProfile(bool dueToChangedSize)
    {
        _canProfileWidthChange = false;

        if (dueToChangedSize)
        {
            ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                                GlobalStaticConfiguration.Profile_FloatPointFix;
        }
        else
        {
            if (ExpandProfileView)
            {
                ProfileWidth =
                    Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) <
                    GlobalStaticConfiguration.Profile_FloatPointFix
                        ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded)
                        : new GridLength(_tempProfileWidth);
            }
            else
            {
                _tempProfileWidth = ProfileWidth.Value;
                ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
            }
        }

        _canProfileWidthChange = true;
    }

    private void ResetBandwidthChart()
    {
        if (Series == null)
            return;

        Series[0].Values.Clear();
        Series[1].Values.Clear();

        var currentDateTime = DateTime.Now;

        for (var i = 60; i > 0; i--)
        {
            var bandwidthInfo = new LvlChartsDefaultInfo(currentDateTime.AddSeconds(-i), double.NaN);

            Series[0].Values.Add(bandwidthInfo);
            Series[1].Values.Add(bandwidthInfo);
        }
    }

    private bool _resetBandwidthStatisticOnNextUpdate;

    private void StartBandwidthMeter(string networkInterfaceId)
    {
        // Reset chart
        ResetBandwidthChart();

        // Reset statistic
        _resetBandwidthStatisticOnNextUpdate = true;

        _bandwidthMeter = new BandwidthMeter(networkInterfaceId);
        _bandwidthMeter.UpdateSpeed += BandwidthMeter_UpdateSpeed;
        _bandwidthMeter.Start();
    }

    private void ResumeBandwidthMeter()
    {
        if (_bandwidthMeter is not { IsRunning: false })
            return;

        ResetBandwidthChart();

        _resetBandwidthStatisticOnNextUpdate = true;

        _bandwidthMeter.Start();
    }

    private void StopBandwidthMeter()
    {
        if (_bandwidthMeter is not { IsRunning: true })
            return;

        _bandwidthMeter.Stop();
    }

    /// <summary>
    /// Called when the view becomes visible.
    /// </summary>
    public void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();

        ResumeBandwidthMeter();
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
        StopBandwidthMeter();

        _isViewActive = false;
    }

    private void CreateTags()
    {
        var tags = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.NetworkInterface_Enabled)
            .SelectMany(x => x.TagsCollection).Distinct().ToList();

        var tagSet = new HashSet<string>(tags);

        for (var i = ProfileFilterTags.Count - 1; i >= 0; i--)
        {
            if (!tagSet.Contains(ProfileFilterTags[i].Name))
                ProfileFilterTags.RemoveAt(i);
        }

        var existingTagNames = new HashSet<string>(ProfileFilterTags.Select(ft => ft.Name));

        foreach (var tag in tags.Where(tag => !existingTagNames.Contains(tag)))
        {
            ProfileFilterTags.Add(new ProfileFilterTagsInfo(false, tag));
        }
    }

    private void SetProfilesView(ProfileFilterInfo filter, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.NetworkInterface_Enabled && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
                    // If no tags are selected, show all profiles
                    (!filter.Tags.Any()) ||
                    // Any tag can match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.Any &&
                     filter.Tags.Any(tag => x.TagsCollection.Contains(tag))) ||
                    // All tags must match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.All &&
                     filter.Tags.All(tag => x.TagsCollection.Contains(tag))))
            ).OrderBy(x => x.Group).ThenBy(x => x.Name)
        }.View;

        Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));

        // Set specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                              Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();
    }

    private void RefreshProfiles()
    {
        if (!_isViewActive)
            return;

        var filter = new ProfileFilterInfo
        {
            Search = Search,
            Tags = [.. ProfileFilterTags.Where(x => x.IsSelected).Select(x => x.Name)],
            TagsFilterMatch = ProfileFilterTagsMatchAny ? ProfileFilterTagsMatch.Any : ProfileFilterTagsMatch.All
        };

        SetProfilesView(filter, SelectedProfile);

        IsProfileFilterSet = !string.IsNullOrEmpty(filter.Search) || filter.Tags.Any();
    }

    #endregion

    #region Events

    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        CreateTags();

        RefreshProfiles();
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    private void BandwidthMeter_UpdateSpeed(object sender, BandwidthMeterSpeedArgs e)
    {
        // Reset statistics
        if (_resetBandwidthStatisticOnNextUpdate)
        {
            BandwidthStartTime = DateTime.Now;
            _bandwidthTotalBytesReceivedTemp = e.TotalBytesReceived;
            _bandwidthTotalBytesSentTemp = e.TotalBytesSent;

            _resetBandwidthStatisticOnNextUpdate = false;
        }

        // Measured time
        BandwidthMeasuredTime = DateTime.Now - BandwidthStartTime;

        // Current download/upload
        BandwidthTotalBytesReceived = e.TotalBytesReceived;
        BandwidthTotalBytesSent = e.TotalBytesSent;
        BandwidthBytesReceivedSpeed = e.ByteReceivedSpeed;
        BandwidthBytesSentSpeed = e.ByteSentSpeed;

        // Total download/upload
        BandwidthDiffBytesReceived = BandwidthTotalBytesReceived - _bandwidthTotalBytesReceivedTemp;
        BandwidthDiffBytesSent = BandwidthTotalBytesSent - _bandwidthTotalBytesSentTemp;

        // Add chart entry
        Series[0].Values.Add(new LvlChartsDefaultInfo(e.DateTime, e.ByteReceivedSpeed));
        Series[1].Values.Add(new LvlChartsDefaultInfo(e.DateTime, e.ByteSentSpeed));

        // Remove data older than 60 seconds
        if (Series[0].Values.Count > 59)
            Series[0].Values.RemoveAt(0);

        if (Series[1].Values.Count > 59)
            Series[1].Values.RemoveAt(0);
    }

    private void NetworkInterface_UserHasCanceled(object sender, EventArgs e)
    {
        StatusMessage = Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;
    }

    #endregion
}