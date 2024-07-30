using System;
using System.Collections.Generic;
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
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using NetworkInterface = NETworkManager.Models.Network.NetworkInterface;

namespace NETworkManager.ViewModels;

public class NetworkInterfaceViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private BandwidthMeter _bandwidthMeter;

    private readonly bool _isLoading;
    private bool _isViewActive = true;

    private bool _isNetworkInterfaceLoading;

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

    private List<NetworkInterfaceInfo> _networkInterfaces;

    public List<NetworkInterfaceInfo> NetworkInterfaces
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

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            // Start searching...
            IsSearching = true;
            _searchDispatcherTimer.Start();

            OnPropertyChanged();
        }
    }

    private bool _isSearching;

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

    private bool _canProfileWidthChange = true;
    private double _tempProfileWidth;

    private bool _expandProfileView;

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

    public NetworkInterfaceViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        LoadNetworkInterfaces().ConfigureAwait(false);

        InitialBandwidthChart();

        // Profiles
        SetProfilesView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        // Detect if network address or status changed...
        NetworkChange.NetworkAvailabilityChanged += (_, _) => ReloadNetworkInterfaces();
        NetworkChange.NetworkAddressChanged += (_, _) => ReloadNetworkInterfaces();

        LoadSettings();

        _isLoading = false;
    }

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

    private async Task LoadNetworkInterfaces()
    {
        IsNetworkInterfaceLoading = true;

        NetworkInterfaces = await NetworkInterface.GetNetworkInterfacesAsync();

        // Get the last selected interface, if it is still available on this machine...
        if (NetworkInterfaces.Count > 0)
        {
            var info = NetworkInterfaces.FirstOrDefault(s =>
                s.Id == SettingsManager.Current.NetworkInterface_InterfaceId);

            SelectedNetworkInterface = info ?? NetworkInterfaces[0];
        }

        IsNetworkInterfaceLoading = false;
    }

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

    public ICommand ReloadNetworkInterfacesCommand =>
        new RelayCommand(_ => ReloadNetworkInterfacesAction(), ReloadNetworkInterfaces_CanExecute);

    private bool ReloadNetworkInterfaces_CanExecute(object obj)
    {
        return !IsNetworkInterfaceLoading &&
               Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow)
                   .IsAnyDialogOpen;
    }

    private void ReloadNetworkInterfacesAction()
    {
        ReloadNetworkInterfaces();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        instance.ExportAll ? NetworkInterfaces : [SelectedNetworkInterface]);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.NetworkInterface_ExportFileType = instance.FileType;
                SettingsManager.Current.NetworkInterface_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], true,
            SettingsManager.Current.NetworkInterface_ExportFileType,
            SettingsManager.Current.NetworkInterface_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public ICommand ApplyConfigurationCommand =>
        new RelayCommand(_ => ApplyConfigurationAction(), ApplyConfiguration_CanExecute);

    private bool ApplyConfiguration_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow)
                   .IsAnyDialogOpen;
    }

    private void ApplyConfigurationAction()
    {
        ApplyConfiguration().ConfigureAwait(false);
    }

    public ICommand ApplyProfileConfigCommand => new RelayCommand(_ => ApplyProfileProfileAction());

    private void ApplyProfileProfileAction()
    {
        ApplyConfigurationFromProfile().ConfigureAwait(false);
    }

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(this, this, _dialogCoordinator, null, null, ApplicationName.NetworkInterface)
            .ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(this, _dialogCoordinator, new List<ProfileInfo> { SelectedProfile })
            .ConfigureAwait(false);
    }

    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, ProfileManager.GetGroup(group.ToString()))
            .ConfigureAwait(false);
    }

    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }

    #region Additional commands

    private bool AdditionalCommands_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow)
                   .IsAnyDialogOpen;
    }

    public ICommand OpenNetworkConnectionsCommand =>
        new RelayCommand(_ => OpenNetworkConnectionsAction(), AdditionalCommands_CanExecute);

    private void OpenNetworkConnectionsAction()
    {
        OpenNetworkConnectionsAsync().ConfigureAwait(false);
    }

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

    public ICommand FlushDNSCommand => new RelayCommand(_ => FlushDNSAction(), AdditionalCommands_CanExecute);

    private void FlushDNSAction()
    {
        FlushDNSAsync().ConfigureAwait(false);
    }

    public ICommand ReleaseRenewCommand => new RelayCommand(_ => ReleaseRenewAction(), AdditionalCommands_CanExecute);

    private void ReleaseRenewAction()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew).ConfigureAwait(false);
    }

    public ICommand ReleaseCommand => new RelayCommand(_ => ReleaseAction(), AdditionalCommands_CanExecute);

    private void ReleaseAction()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Release).ConfigureAwait(false);
    }

    public ICommand RenewCommand => new RelayCommand(_ => RenewAction(), AdditionalCommands_CanExecute);

    private void RenewAction()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew).ConfigureAwait(false);
    }

    public ICommand ReleaseRenew6Command => new RelayCommand(_ => ReleaseRenew6Action(), AdditionalCommands_CanExecute);

    private void ReleaseRenew6Action()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew6).ConfigureAwait(false);
    }

    public ICommand Release6Command => new RelayCommand(_ => Release6Action(), AdditionalCommands_CanExecute);

    private void Release6Action()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Release6).ConfigureAwait(false);
    }

    public ICommand Renew6Command => new RelayCommand(_ => Renew6Action(), AdditionalCommands_CanExecute);

    private void Renew6Action()
    {
        ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew).ConfigureAwait(false);
    }

    public ICommand AddIPv4AddressCommand => new RelayCommand(_ => AddIPv4AddressAction().ConfigureAwait(false),
        AdditionalCommands_CanExecute);

    private async Task AddIPv4AddressAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddIPv4Address
        };

        var ipAddressAndSubnetmaskViewModel = new IPAddressAndSubnetmaskViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            await AddIPv4Address(instance.IPAddress, instance.Subnetmask);
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); });

        customDialog.Content = new IPAddressAndSubnetmaskDialog
        {
            DataContext = ipAddressAndSubnetmaskViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public ICommand RemoveIPv4AddressCommand => new RelayCommand(_ => RemoveIPv4AddressAction().ConfigureAwait(false),
        AdditionalCommands_CanExecute);

    private async Task RemoveIPv4AddressAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.RemoveIPv4Address
        };

        var dropdownViewModel = new DropdownViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                await RemoveIPv4Address(instance.SelectedValue.Split("/")[0]);
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            SelectedNetworkInterface.IPv4Address.Select(x => $"{x.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(x.Item2)}")
                .ToList(), Strings.IPv4Address);

        customDialog.Content = new DropdownDialog
        {
            DataContext = dropdownViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion

    #endregion

    #region Methods

    private async void ReloadNetworkInterfaces()
    {
        IsNetworkInterfaceLoading = true;

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
        await Task.Delay(2000);

        var id = string.Empty;

        if (SelectedNetworkInterface != null)
            id = SelectedNetworkInterface.Id;

        NetworkInterfaces = await NetworkInterface.GetNetworkInterfacesAsync();

        // Change interface...
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
            await _dialogCoordinator.ShowMessageAsync(this, Strings.Error, ex.Message,
                MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
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

    public void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();

        ResumeBandwidthMeter();
    }

    public void OnViewHide()
    {
        StopBandwidthMeter();

        _isViewActive = false;
    }

    private void SetProfilesView(ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.NetworkInterface_Enabled)
                .OrderBy(x => x.Group).ThenBy(x => x.Name)
        }.View;

        Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));

        Profiles.Filter = o =>
        {
            if (o is not ProfileInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            var search = Search.Trim();

            // Search by: Tag=xxx (exact match, ignore case)
            /*
            if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                return !string.IsNullOrEmpty(info.Tags) && info.PingMonitor_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
            */

            // Search by: Name
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

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

        SetProfilesView(SelectedProfile);
    }

    #endregion

    #region Events

    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
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