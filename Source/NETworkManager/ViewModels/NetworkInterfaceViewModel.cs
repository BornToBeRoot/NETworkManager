using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NetworkInterface = NETworkManager.Models.Network.NetworkInterface;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Network Interface feature, allowing management of network adapters.
/// </summary>
public class NetworkInterfaceViewModel : ProfileHostViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkInterfaceViewModel));

    private BandwidthMeter _bandwidthMeter;

    /// <summary>
    /// The network interface is selected by the (re)load of the network interfaces, not by the user: the selection is
    /// not saved, so a fallback (e.g. while a VPN or USB adapter is missing) doesn't replace the remembered one.
    /// </summary>
    private bool _isSelectingNetworkInterfaceByCode;

    /// <summary>
    /// The network interface to select after a reload (also if it is back after it was missing). Selected by the user
    /// (saved in the settings), by a profile (only for this session: looking at a profile doesn't change the network
    /// interface shown after a restart) or configured.
    /// </summary>
    private string _preferredNetworkInterfaceId = SettingsManager.Current.NetworkInterface_InterfaceId;

    /// <summary>
    /// Gets or sets a value indicating whether network interfaces are currently loading.
    /// </summary>
    public bool IsNetworkInterfaceLoading
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the application has the permission to change the network configuration
    /// (running as administrator or as member of the "Network Configuration Operators" group).
    /// </summary>
    public bool HasPermissionToConfigure { get; } = NetworkInterface.HasPermissionToConfigure();

    /// <summary>
    /// Gets or sets a value indicating whether configuration is allowed.
    /// </summary>
    public bool CanConfigure
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a configuration operation is running.
    /// </summary>
    public bool IsConfigurationRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the status message.
    /// </summary>
    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Level of the status message (icon and color, see <see cref="StatusMessageText" />).
    /// </summary>
    public StatusLevel StatusMessageLevel
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #region NetworkInterfaces, SelectedNetworkInterface

    /// <summary>
    /// Gets the collection of network interfaces.
    /// </summary>
    public ObservableCollection<NetworkInterfaceInfo> NetworkInterfaces
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    /// <summary>
    /// ID of the interface the bandwidth meter and the configuration were set up for.
    /// </summary>
    private string _selectedNetworkInterfaceId;

    /// <summary>
    /// Gets or sets the currently selected network interface.
    /// </summary>
    public NetworkInterfaceInfo SelectedNetworkInterface
    {
        get;
        set
        {
            if (value == field)
                return;

            // A reload creates new objects for the same interfaces (and the ComboBox selects null while the list is
            // replaced). Only start a new measurement and reset the configuration if another interface is selected,
            // not when the selected one is reloaded (e.g. after a network change or Apply).
            if (value != null && value.Id != _selectedNetworkInterfaceId)
            {
                _selectedNetworkInterfaceId = value.Id;

                if (!_isSelectingNetworkInterfaceByCode)
                    RememberNetworkInterface(value);

                // Bandwidth
                StartBandwidthMeter(value.Id);

                // Configuration
                SetConfigurationDefaults(value);
            }

            CanConfigure = value?.IsOperational == true;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Bandwidth

    private long _bandwidthTotalBytesSentTemp;

    /// <summary>
    /// Gets or sets the total bytes sent.
    /// </summary>
    public long BandwidthTotalBytesSent
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private long _bandwidthTotalBytesReceivedTemp;

    /// <summary>
    /// Gets or sets the total bytes received.
    /// </summary>
    public long BandwidthTotalBytesReceived
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the difference in bytes sent.
    /// </summary>
    public long BandwidthDiffBytesSent
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the difference in bytes received.
    /// </summary>
    public long BandwidthDiffBytesReceived
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the speed of bytes received.
    /// </summary>
    public long BandwidthBytesReceivedSpeed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the speed of bytes sent.
    /// </summary>
    public long BandwidthBytesSentSpeed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the start time of the bandwidth measurement.
    /// </summary>
    public DateTime BandwidthStartTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the duration of the bandwidth measurement.
    /// </summary>
    public TimeSpan BandwidthMeasuredTime
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Config

    /// <summary>
    /// Gets or sets a value indicating whether to enable dynamic IP address (DHCP).
    /// </summary>
    public bool ConfigEnableDynamicIPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable static IP address.
    /// </summary>
    public bool ConfigEnableStaticIPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            // DNS servers can only be obtained automatically with DHCP (same as the Windows dialog)
            if (value)
            {
                ConfigEnableDynamicDNS = false;
                ConfigEnableStaticDNS = true;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the static IP address.
    /// </summary>
    public string ConfigIPAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the subnet mask.
    /// </summary>
    public string ConfigSubnetmask
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the default gateway.
    /// </summary>
    public string ConfigGateway
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to enable dynamic DNS (DHCP).
    /// </summary>
    public bool ConfigEnableDynamicDNS
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable static DNS.
    /// </summary>
    public bool ConfigEnableStaticDNS
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the primary DNS server.
    /// </summary>
    public string ConfigPrimaryDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the secondary DNS server.
    /// </summary>
    public string ConfigSecondaryDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets the selected profile. Selecting a profile selects the network interface it is bound to and fills
    /// in its values on the Configure tab.
    /// </summary>
    public override ProfileInfo SelectedProfile
    {
        get => base.SelectedProfile;
        set
        {
            if (value == base.SelectedProfile)
                return;

            // Only if the user selects the profile. The profile list is rebuilt (and the previous or the first profile
            // selected again) when the view becomes visible, on search / filter and when a profile changes: this
            // must not overwrite the values entered on the Configure tab or switch the network interface.
            // Not while a configuration is applied (like Reset): the result would be shown for another interface
            if (value != null && !IsProfileSelectedByCode && !IsConfigurationRunning)
                LoadProfile(value);

            base.SelectedProfile = value;
        }
    }

    #endregion

    #region Constructor, LoadSettings, OnShutdown

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkInterfaceViewModel"/> class.
    /// </summary>
    public NetworkInterfaceViewModel()
    {
        // Initialize the bandwidth chart before loading interfaces, so the chart collections
        // exist by the time the (possibly synchronous) interface load selects an interface and
        // starts the bandwidth meter.
        InitialBandwidthChart();

        _ = LoadNetworkInterfaces();

        InitializeProfileHost();

        // Detect if network address or status changed... The events are raised on a thread pool thread: reload on the
        // UI thread, otherwise the bandwidth meter (DispatcherTimer) of the selected interface is created on a thread
        // without a running dispatcher and never updates.
        NetworkChange.NetworkAvailabilityChanged += (_, _) => ReloadNetworkInterfacesOnUIThread();
        NetworkChange.NetworkAddressChanged += (_, _) => ReloadNetworkInterfacesOnUIThread();

        // React to settings changes (e.g. the configurable bandwidth chart time window)
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        // Colors of the bandwidth chart for the new theme (the paints don't follow the theme)
        AppearanceManager.ThemeResourcesChanged += (_, _) => ApplyBandwidthChartColors();
    }

    /// <summary>
    /// Sets the colors of the bandwidth chart (series, axes, legend) for the current theme.
    /// </summary>
    private void ApplyBandwidthChartColors()
    {
        var colors = new[] { ChartColors.Primary, ChartColors.Secondary };

        for (var i = 0; i < Series.Length && i < colors.Length; i++)
        {
            if (Series[i] is not LineSeries<LvlChartsDefaultInfo> series)
                continue;

            series.Stroke = ChartColors.LineStroke(colors[i]);
            series.Fill = ChartColors.AreaFill(colors[i]);
        }

        ChartColors.ApplyAxisColors(BandwidthXAxes[0]);
        ChartColors.ApplyAxisColors(BandwidthYAxes[0]);

        BandwidthLegendTextPaint = new SolidColorPaint(ChartColors.LabelColor) { SKTypeface = SKTypeface.Default };
    }

    /// <summary>
    /// The visible chart time window in seconds, configurable via the settings.
    /// </summary>
    private static double BandwidthChartWindowSeconds => SettingsManager.Current.NetworkInterface_BandwidthChartTime;

    /// <summary>
    /// Extra share of samples kept beyond the visible window so the chart has a little
    /// off-screen history (smoother left edge while live + minor pan-back headroom).
    /// </summary>
    private const double BandwidthValuesHeadroom = 1.1;

    private int _maxBandwidthValues;
    private bool _updatingBandwidthAxisFromCode;
    private DateTime _bandwidthSessionStartTime;

    private ObservableCollection<LvlChartsDefaultInfo> _bandwidthReceivedValues;
    private ObservableCollection<LvlChartsDefaultInfo> _bandwidthSentValues;

    // Capped rolling history kept off-screen so the chart can be rebuilt when the user
    // returns to live mode after panning/zooming (see BandwidthGoLiveAction).
    private readonly List<LvlChartsDefaultInfo> _bandwidthReceivedHistory = [];
    private readonly List<LvlChartsDefaultInfo> _bandwidthSentHistory = [];

    private void InitialBandwidthChart()
    {
        _bandwidthReceivedValues = [];
        _bandwidthSentValues = [];
        _bandwidthSessionStartTime = DateTime.Now;

        // Start in live mode so samples are plotted immediately, independent of when the meter
        // starts relative to this initialization.
        IsBandwidthLiveMode = true;

        UpdateMaxBandwidthValues();

        var downloadColor = ChartColors.Primary;
        var uploadColor = ChartColors.Secondary;

        Series =
        [
            new LineSeries<LvlChartsDefaultInfo>
            {
                Name = Strings.Download,
                Values = _bandwidthReceivedValues,
                Mapping = (info, _) => double.IsNaN(info.Value)
                    ? Coordinate.Empty
                    : new((info.DateTime - _bandwidthSessionStartTime).TotalSeconds, info.Value),
                GeometrySize = 0,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = ChartColors.LineStroke(downloadColor),
                Fill = ChartColors.AreaFill(downloadColor)
            },
            new LineSeries<LvlChartsDefaultInfo>
            {
                Name = Strings.Upload,
                Values = _bandwidthSentValues,
                Mapping = (info, _) => double.IsNaN(info.Value)
                    ? Coordinate.Empty
                    : new((info.DateTime - _bandwidthSessionStartTime).TotalSeconds, info.Value),
                GeometrySize = 0,
                LineSmoothness = 0.3,
                DataPadding = new LvcPoint(0, 0),
                Stroke = ChartColors.LineStroke(uploadColor),
                Fill = ChartColors.AreaFill(uploadColor)
            }
        ];

        BandwidthXAxes =
        [
            new Axis
            {
                Labeler = value => DateTimeHelper.DateTimeToTimeString(
                    _bandwidthSessionStartTime.AddSeconds(value)),
                TextSize = 10,
                Padding = new Padding(0, 4, 0, 0),
                LabelsPaint = ChartColors.LabelPaint(),
                SeparatorsPaint = ChartColors.SeparatorPaint(),
                MinStep = BandwidthChartWindowSeconds / 4.0,
                ForceStepToMin = true
            }
        ];

        BandwidthYAxes =
        [
            new Axis
            {
                MinLimit = 0,
                Labeler = value => $"{FileSizeConverter.GetBytesReadable((long)value * 8)}it/s",
                TextSize = 11,
                Padding = new Padding(4, 0),
                LabelsPaint = ChartColors.LabelPaint(),
                SeparatorsPaint = ChartColors.SeparatorPaint()
            }
        ];

        BandwidthLegendTextPaint = new SolidColorPaint(ChartColors.LabelColor) { SKTypeface = SKTypeface.Default };

        BandwidthXAxes[0].PropertyChanged += (_, args) =>
        {
            if (_updatingBandwidthAxisFromCode)
                return;

            if (args.PropertyName is not (nameof(Axis.MinLimit) or nameof(Axis.MaxLimit)))
                return;

            IsBandwidthLiveMode = false;

            var axis = BandwidthXAxes[0];
            if (axis.MinLimit.HasValue && axis.MaxLimit.HasValue)
            {
                _updatingBandwidthAxisFromCode = true;
                axis.MinStep = (axis.MaxLimit.Value - axis.MinLimit.Value) / 4.0;
                _updatingBandwidthAxisFromCode = false;
            }
        };

        UpdateBandwidthXAxisWindow(DateTime.Now);
    }

    /// <summary>
    /// Gets the series collection for the bandwidth chart (download/upload).
    /// </summary>
    public ISeries[] Series { get; private set; }

    /// <summary>
    /// Gets the X-axes configuration for the bandwidth chart.
    /// </summary>
    public Axis[] BandwidthXAxes { get; private set; }

    /// <summary>
    /// Gets the Y-axes configuration for the bandwidth chart.
    /// </summary>
    public Axis[] BandwidthYAxes { get; private set; }

    /// <summary>
    /// Gets the themed paint used for the chart legend text.
    /// </summary>
    public SolidColorPaint BandwidthLegendTextPaint
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the bandwidth chart is in live (auto-scrolling) mode.
    /// </summary>
    public bool IsBandwidthLiveMode
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private void UpdateMaxBandwidthValues()
    {
        // Number of samples that fit in the visible window (derived from the meter's sample
        // interval), plus a little off-screen headroom.
        var samplesPerWindow = BandwidthChartWindowSeconds * 1000 / BandwidthMeter.DefaultUpdateInterval;
        _maxBandwidthValues = (int)Math.Ceiling(samplesPerWindow * BandwidthValuesHeadroom);
    }

    /// <summary>
    /// Loads the network interfaces.
    /// </summary>
    private async Task LoadNetworkInterfaces()
    {
        IsNetworkInterfaceLoading = true;

        try
        {
            // Get all network interfaces...
            (await NetworkInterface.GetNetworkInterfacesAsync()).ForEach(NetworkInterfaces.Add);

            // Get the last selected interface, if it is still available on this machine...
            SelectNetworkInterfaceAfterLoad(null);
        }
        catch (Exception ex)
        {
            Log.Error("Error while loading the network interfaces.", ex);
        }
        finally
        {
            IsNetworkInterfaceLoading = false;
        }

        OpenPendingRequest();
        ReloadIfPending();
    }

    #endregion

    #region Profile host

    /// <summary>
    /// Request received while the network interfaces were loaded, opened after the load (see <see cref="Open" />).
    /// </summary>
    private ApplicationRequest _pendingRequest;

    private void OpenPendingRequest()
    {
        if (_pendingRequest == null)
            return;

        var request = _pendingRequest;
        _pendingRequest = null;

        Open(request);
    }

    protected override ApplicationName ApplicationName => ApplicationName.NetworkInterface;

    /// <summary>
    /// Opens a request of the run command (e.g. "networkinterface @home"): selects the profile and fills in its values
    /// like a double-click on the profile. The profile is never applied (also not with Enter): the configuration is
    /// changed with elevated rights and can disconnect the computer, it is applied with Apply after checking the values.
    /// </summary>
    /// <param name="request">Request with the profile (a request without a profile only opens the tool).</param>
    public void Open(ApplicationRequest request)
    {
        // Not while a configuration is applied (like selecting / double-clicking a profile)
        if (request.Profile == null || IsConfigurationRunning)
            return;

        // The network interfaces are (re)loaded, e.g. the run command opened the tool for the first time (the view and
        // its view model are created right before): the bound network interface can only be selected afterwards, and
        // the load would replace the values of the profile with the ones of the selected network interface.
        if (IsNetworkInterfaceLoading)
        {
            _pendingRequest = request;
            return;
        }

        // Select it in the profile list if it is shown (not hidden by the search / filter): selecting loads it
        if (Profiles.Contains(request.Profile) && !Equals(SelectedProfile, request.Profile))
        {
            SelectedProfile = request.Profile;
            return;
        }

        LoadProfile(request.Profile);
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
               !DialogHelper.IsAnyDialogOpen();
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
    public ICommand ExportCommand => new RelayCommand(parameter => { _ = ExportAction(); });

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
    /// Action to apply the network configuration.
    /// </summary>
    private void ApplyConfigurationAction()
    {
        _ = ApplyConfiguration();
    }

    /// <summary>
    /// Gets the command to load the selected profile again (double-click / Enter in the profile list): selects its
    /// network interface and fills in its values, e.g. after Reset or if the profile was selected automatically
    /// (clicking the selected profile doesn't change the selection).
    /// </summary>
    public ICommand LoadProfileCommand => new RelayCommand(_ => LoadProfile(SelectedProfile), LoadProfile_CanExecute);

    private bool LoadProfile_CanExecute(object parameter) => SelectedProfile != null && !IsConfigurationRunning;

    /// <summary>
    /// Gets the command to apply the profile configuration.
    /// </summary>
    public ICommand ApplyProfileCommand => new RelayCommand(_ => ApplyProfileAction(), ApplyProfile_CanExecute);

    private void ApplyProfileAction()
    {
        _ = ApplyConfigurationFromProfile();
    }

    /// <summary>
    /// Gets the command to reset the Configure tab to the current configuration of the selected network interface
    /// (e.g. after a profile was selected or values were changed).
    /// </summary>
    public ICommand ResetConfigurationCommand => new RelayCommand(_ => ResetConfigurationAction(),
        ResetConfiguration_CanExecute);

    private bool ResetConfiguration_CanExecute(object parameter) =>
        SelectedNetworkInterface != null && !IsConfigurationRunning;

    private void ResetConfigurationAction()
    {
        SetConfigurationDefaults(SelectedNetworkInterface);
    }

    /// <summary>
    /// Gets the command to add a profile with the configuration of the Configure tab, bound to the selected network
    /// interface.
    /// </summary>
    public ICommand AddProfileFromConfigurationCommand => new RelayCommand(_ => AddProfileFromConfigurationAction(),
        AddProfileFromConfiguration_CanExecute);

    private bool AddProfileFromConfiguration_CanExecute(object parameter) =>
        AdditionalCommands_CanExecute(parameter) && SelectedNetworkInterface != null;

    private void AddProfileFromConfigurationAction()
    {
        var networkInterface = SelectedNetworkInterface;

        ProfileInfo profileInfo = new()
        {
            Name = networkInterface.Name,
            // Required by the profile, not used by the Network Interface
            Host = ConfigEnableStaticIPAddress
                ? ConfigIPAddress?.Trim()
                : networkInterface.IPv4Address?.FirstOrDefault()?.Item1.ToString(),

            NetworkInterface_BindToInterface = true,
            NetworkInterface_InterfaceId = networkInterface.Id,
            NetworkInterface_InterfaceName = networkInterface.Name,
            NetworkInterface_EnableStaticIPAddress = ConfigEnableStaticIPAddress,
            NetworkInterface_IPAddress = ConfigIPAddress,
            NetworkInterface_Subnetmask = ConfigSubnetmask,
            NetworkInterface_Gateway = ConfigGateway,
            NetworkInterface_EnableStaticDNS = ConfigEnableStaticDNS,
            NetworkInterface_PrimaryDNSServer = ConfigPrimaryDNSServer,
            NetworkInterface_SecondaryDNSServer = ConfigSecondaryDNSServer
        };

        _ = ProfileDialogManager.ShowAddProfileDialog(Application.Current.MainWindow, this, profileInfo, null,
            ApplicationName.NetworkInterface);
    }

    #region Additional commands

    private bool AdditionalCommands_CanExecute(object parameter)
    {
        return !DialogHelper.IsAnyDialogOpen();
    }

    /// <summary>
    /// Configuration actions that don't need a network interface (e.g. flush DNS cache). Only one runs at a time.
    /// </summary>
    private bool ConfigureCommands_CanExecute(object parameter) => HasPermissionToConfigure &&
                                                                   !IsConfigurationRunning &&
                                                                   !DialogHelper.IsAnyDialogOpen();

    /// <summary>
    /// Configuration actions on the selected network interface (release/renew, add/remove IP address).
    /// </summary>
    private bool ConfigureNetworkInterface_CanExecute(object parameter) =>
        ConfigureCommands_CanExecute(parameter) && SelectedNetworkInterface != null;

    /// <summary>
    /// Apply the configuration (Configure tab): the network interface must be connected.
    /// </summary>
    private bool ApplyConfiguration_CanExecute(object parameter) =>
        ConfigureCommands_CanExecute(parameter) && CanConfigure;

    /// <summary>
    /// Apply the selected profile (profile context menu): same conditions as the Apply button. A profile bound to a
    /// network interface doesn't depend on the selected one (checked when it is applied).
    /// </summary>
    private bool ApplyProfile_CanExecute(object parameter) =>
        ConfigureCommands_CanExecute(parameter) && SelectedProfile != null &&
        (SelectedProfile.NetworkInterface_BindToInterface || CanConfigure);

    /// <summary>
    /// Gets the command to restart the application with administrator privileges.
    /// </summary>
    public ICommand RestartAsAdminCommand => new RelayCommand(parameter => { _ = RestartAsAdminAction(); });

    private async Task RestartAsAdminAction()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.RestartApplication(true);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    /// <summary>
    /// Gets the command to open network connections.
    /// </summary>
    public ICommand OpenNetworkConnectionsCommand =>
        new RelayCommand(_ => OpenNetworkConnectionsAction(), AdditionalCommands_CanExecute);

    private void OpenNetworkConnectionsAction()
    {
        _ = OpenNetworkConnectionsAsync();
    }

    /// <summary>
    /// Gets the command to open IP Scanner.
    /// </summary>
    public ICommand IPScannerCommand => new RelayCommand(_ => IPScannerAction(), IPScanner_CanExecute);

    /// <summary>
    /// The IP Scanner needs an IPv4 address with subnet mask (e.g. not for a disconnected adapter).
    /// </summary>
    private bool IPScanner_CanExecute(object parameter) => AdditionalCommands_CanExecute(parameter) &&
                                                          GetFirstIPv4AddressWithSubnetmask() != null;

    private Tuple<System.Net.IPAddress, System.Net.IPAddress> GetFirstIPv4AddressWithSubnetmask() =>
        SelectedNetworkInterface?.IPv4Address?.FirstOrDefault(x => x.Item2 != null);

    private void IPScannerAction()
    {
        var ipTuple = GetFirstIPv4AddressWithSubnetmask();

        if (ipTuple == null)
            return;

        NavigationManager.Open(new ApplicationRequest(ApplicationName.IPScanner)
        {
            Target = $"{ipTuple.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(ipTuple.Item2)}"
        });
    }

    /// <summary>
    /// Gets the command to flush DNS.
    /// </summary>
    public ICommand FlushDNSCommand => new RelayCommand(_ => FlushDNSAction(), ConfigureCommands_CanExecute);

    private void FlushDNSAction()
    {
        _ = FlushDNSAsync();
    }

    /// <summary>
    /// Gets the command to release and renew IP address.
    /// </summary>
    public ICommand ReleaseRenewCommand => new RelayCommand(_ => ReleaseRenewAction(), ConfigureNetworkInterface_CanExecute);

    private void ReleaseRenewAction()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew);
    }

    /// <summary>
    /// Gets the command to release IP address.
    /// </summary>
    public ICommand ReleaseCommand => new RelayCommand(_ => ReleaseAction(), ConfigureNetworkInterface_CanExecute);

    private void ReleaseAction()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Release);
    }

    /// <summary>
    /// Gets the command to renew IP address.
    /// </summary>
    public ICommand RenewCommand => new RelayCommand(_ => RenewAction(), ConfigureNetworkInterface_CanExecute);

    private void RenewAction()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew);
    }

    /// <summary>
    /// Gets the command to release and renew IPv6 address.
    /// </summary>
    public ICommand ReleaseRenew6Command => new RelayCommand(_ => ReleaseRenew6Action(), ConfigureNetworkInterface_CanExecute);

    private void ReleaseRenew6Action()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew6);
    }

    /// <summary>
    /// Gets the command to release IPv6 address.
    /// </summary>
    public ICommand Release6Command => new RelayCommand(_ => Release6Action(), ConfigureNetworkInterface_CanExecute);

    private void Release6Action()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Release6);
    }

    /// <summary>
    /// Gets the command to renew IPv6 address.
    /// </summary>
    public ICommand Renew6Command => new RelayCommand(_ => Renew6Action(), ConfigureNetworkInterface_CanExecute);

    private void Renew6Action()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew6);
    }

    /// <summary>
    /// Gets the command to add an IPv4 address.
    /// </summary>
    public ICommand AddIPv4AddressCommand => new RelayCommand(parameter => { _ = AddIPv4AddressAction(); },
        ConfigureNetworkInterface_CanExecute);

    private async Task AddIPv4AddressAction()
    {
        // The network interface the dialog is opened for (the selection can change while it is open)
        var networkInterface = SelectedNetworkInterface;

        var childWindow = new IPAddressAndSubnetmaskChildWindow();

        var childWindowViewModel = new IPAddressAndSubnetmaskViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            await AddIPv4Address(networkInterface, instance.IPAddress, instance.Subnetmask);
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
    public ICommand RemoveIPv4AddressCommand => new RelayCommand(parameter => { _ = RemoveIPv4AddressAction(); },
        ConfigureNetworkInterface_CanExecute);

    private async Task RemoveIPv4AddressAction()
    {
        // The network interface the dialog is opened for (the selection can change while it is open)
        var networkInterface = SelectedNetworkInterface;

        var childWindow = new DropDownChildWindow();

        var childWindowViewModel = new DropDownViewModel(async instance =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;

                await RemoveIPv4Address(networkInterface, instance.SelectedValue.Split("/")[0]);
            }, _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            },
                [.. networkInterface.IPv4Address.Select(x => $"{x.Item1}/{Subnetmask.ConvertSubnetmaskToCidr(x.Item2)}")],
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

    private void ReloadNetworkInterfacesOnUIThread()
    {
        Application.Current?.Dispatcher.BeginInvoke(new Action(ReloadNetworkInterfaces));
    }

    /// <summary>
    /// A reload was requested while the network interfaces were loaded (e.g. after Apply while a reload started by
    /// a network change is running, which may have read the interfaces in the middle of the change).
    /// </summary>
    private bool _reloadPending;

    private async void ReloadNetworkInterfaces()
    {
        // Avoid multiple reloads at the same time, but reload once more afterwards
        if (IsNetworkInterfaceLoading)
        {
            _reloadPending = true;
            return;
        }

        IsNetworkInterfaceLoading = true;

        try
        {
            // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

            // Store the last selected id
            var id = SelectedNetworkInterface?.Id;

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

            SelectNetworkInterfaceAfterLoad(id);
        }
        catch (Exception ex)
        {
            // async void: an unhandled exception would crash the application
            Log.Error("Error while reloading the network interfaces.", ex);
        }
        finally
        {
            IsNetworkInterfaceLoading = false;
        }

        OpenPendingRequest();
        ReloadIfPending();
    }

    /// <summary>
    /// Selects a network interface after the list was (re)loaded: the preferred one (selected by the user or a profile,
    /// also if it is back after it was missing, e.g. a VPN or USB adapter), then the one selected before, then the
    /// first one. The selection is not saved, so a fallback doesn't replace the preferred network interface.
    /// </summary>
    /// <param name="previousId">ID of the network interface selected before the reload.</param>
    private void SelectNetworkInterfaceAfterLoad(string previousId)
    {
        _isSelectingNetworkInterfaceByCode = true;

        try
        {
            SelectedNetworkInterface = NetworkInterface.GetNetworkInterfaceToSelect(NetworkInterfaces,
                _preferredNetworkInterfaceId, previousId);
        }
        finally
        {
            _isSelectingNetworkInterfaceByCode = false;
        }

        // No network interface left: remove the meter of the removed one (it would look it up every second, also
        // after it was resumed when the view becomes visible again)
        if (SelectedNetworkInterface != null)
            return;

        RemoveBandwidthMeter();
        _selectedNetworkInterfaceId = null;
    }

    /// <summary>
    /// Remembers a network interface selected by the user or configured: saved in the settings (selected again after
    /// a restart) and preferred after a reload.
    /// </summary>
    private void RememberNetworkInterface(NetworkInterfaceInfo networkInterface)
    {
        SettingsManager.Current.NetworkInterface_InterfaceId = networkInterface.Id;
        _preferredNetworkInterfaceId = networkInterface.Id;
    }

    /// <summary>
    /// Selects the network interface a profile is bound to: preferred after a reload in this session, but not saved
    /// (only looking at a profile doesn't change the network interface selected after a restart, see
    /// <see cref="RememberNetworkInterface" /> when it is configured).
    /// </summary>
    private void SelectNetworkInterfaceOfProfile(NetworkInterfaceInfo networkInterface)
    {
        _preferredNetworkInterfaceId = networkInterface.Id;
        _isSelectingNetworkInterfaceByCode = true;

        try
        {
            SelectedNetworkInterface = networkInterface;
        }
        finally
        {
            _isSelectingNetworkInterfaceByCode = false;
        }
    }

    private void ReloadIfPending()
    {
        if (!_reloadPending)
            return;

        _reloadPending = false;

        ReloadNetworkInterfaces();
    }

    private void SetConfigurationDefaults(NetworkInterfaceInfo info)
    {
        // Both options of a radio button pair are set: the RadioButton group only unchecks the other one while the
        // Configure tab is loaded (not if another tab is shown).
        if (info.DhcpEnabled)
        {
            ConfigEnableDynamicIPAddress = true;
            ConfigEnableStaticIPAddress = false;

            // Values of another interface or profile don't belong to this one (same as the Windows dialog: empty)
            ConfigIPAddress = string.Empty;
            ConfigSubnetmask = string.Empty;
            ConfigGateway = string.Empty;
        }
        else
        {
            ConfigEnableDynamicIPAddress = false;
            ConfigEnableStaticIPAddress = true;
            ConfigIPAddress = info.IPv4Address.FirstOrDefault()?.Item1.ToString();
            ConfigSubnetmask = info.IPv4Address.FirstOrDefault()?.Item2.ToString();
            ConfigGateway = info.IPv4Gateway?.Any() == true
                ? info.IPv4Gateway.FirstOrDefault()?.ToString()
                : string.Empty;
        }

        if (info.DNSAutoconfigurationEnabled && info.DhcpEnabled)
        {
            ConfigEnableDynamicDNS = true;
            ConfigEnableStaticDNS = false;

            ConfigPrimaryDNSServer = string.Empty;
            ConfigSecondaryDNSServer = string.Empty;
        }
        else
        {
            ConfigEnableDynamicDNS = false;
            ConfigEnableStaticDNS = true;

            var dnsServers = info.DNSServer.Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToList();
            ConfigPrimaryDNSServer = dnsServers.Count > 0 ? dnsServers[0].ToString() : string.Empty;
            ConfigSecondaryDNSServer = dnsServers.Count > 1 ? dnsServers[1].ToString() : string.Empty;
        }
    }

    /// <summary>
    /// Selects the network interface the profile is bound to (if it exists on this computer) and fills in the values
    /// of the profile on the Configure tab.
    /// </summary>
    private void LoadProfile(ProfileInfo profile)
    {
        // The message of a previous action or profile doesn't belong to this profile
        IsStatusMessageDisplayed = false;

        var boundNetworkInterface = GetBoundNetworkInterface(profile);

        if (boundNetworkInterface != null)
        {
            SelectNetworkInterfaceOfProfile(boundNetworkInterface);
        }
        else if (profile.NetworkInterface_BindToInterface)
        {
            // The values would be applied to the selected network interface by the Apply button (the context menu
            // asks first, see ConfirmApplyToSelectedNetworkInterface)
            StatusMessage = string.Format(Strings.NetworkInterfaceXOfProfileNotFoundCheckSelected,
                profile.NetworkInterface_InterfaceName);
            StatusMessageLevel = StatusLevel.Warning;
            IsStatusMessageDisplayed = true;
        }

        SetConfigurationFromProfile(profile);
    }

    /// <summary>
    /// Fills in the values of a profile on the Configure tab.
    /// </summary>
    private void SetConfigurationFromProfile(ProfileInfo profile)
    {
        ConfigEnableDynamicIPAddress = !profile.NetworkInterface_EnableStaticIPAddress;
        ConfigEnableStaticIPAddress = profile.NetworkInterface_EnableStaticIPAddress;
        ConfigIPAddress = profile.NetworkInterface_IPAddress;
        ConfigGateway = profile.NetworkInterface_Gateway;
        ConfigSubnetmask = profile.NetworkInterface_Subnetmask;

        // DNS servers can only be obtained automatically with DHCP (older profiles may contain both)
        var enableStaticDNS = profile.NetworkInterface_EnableStaticIPAddress ||
                              profile.NetworkInterface_EnableStaticDNS;
        ConfigEnableDynamicDNS = !enableStaticDNS;
        ConfigEnableStaticDNS = enableStaticDNS;
        ConfigPrimaryDNSServer = profile.NetworkInterface_PrimaryDNSServer;
        ConfigSecondaryDNSServer = profile.NetworkInterface_SecondaryDNSServer;
    }

    private Task ApplyConfiguration()
    {
        return ApplyConfigurationAsync(SelectedNetworkInterface, NetworkInterfaceConfig.Create(
            SelectedNetworkInterface.Name, ConfigEnableStaticIPAddress, ConfigIPAddress, ConfigSubnetmask, ConfigGateway,
            ConfigEnableStaticDNS, ConfigPrimaryDNSServer, ConfigSecondaryDNSServer));
    }

    private async Task ApplyConfigurationFromProfile()
    {
        var profile = SelectedProfile;
        var networkInterface = SelectedNetworkInterface;

        // Profile bound to a network interface (matched by ID only, the name is not unique)
        if (profile.NetworkInterface_BindToInterface)
        {
            var boundNetworkInterface = GetBoundNetworkInterface(profile);

            if (boundNetworkInterface == null)
            {
                networkInterface = await ConfirmApplyToSelectedNetworkInterface(profile);

                if (networkInterface == null)
                    return;
            }
            else
            {
                // Select it, so the result can be checked on the Information tab
                SelectNetworkInterfaceOfProfile(boundNetworkInterface);
                networkInterface = boundNetworkInterface;
            }
        }

        if (networkInterface is not { IsOperational: true })
        {
            StatusMessage = Strings.ConnectTheNetworkCardToConfigureIt;
            StatusMessageLevel = StatusLevel.Warning;
            IsStatusMessageDisplayed = true;
            return;
        }

        // Show the applied values on the Configure tab (selecting the bound network interface above filled in its
        // current configuration)
        SetConfigurationFromProfile(profile);

        await ApplyConfigurationAsync(networkInterface, NetworkInterfaceConfig.Create(networkInterface.Name,
            profile.NetworkInterface_EnableStaticIPAddress, profile.NetworkInterface_IPAddress,
            profile.NetworkInterface_Subnetmask, profile.NetworkInterface_Gateway,
            profile.NetworkInterface_EnableStaticDNS, profile.NetworkInterface_PrimaryDNSServer,
            profile.NetworkInterface_SecondaryDNSServer));
    }

    /// <summary>
    /// Gets the network interface a profile is bound to, or <see langword="null" /> if the profile is not bound or the
    /// network interface is not found on this computer.
    /// </summary>
    private NetworkInterfaceInfo GetBoundNetworkInterface(ProfileInfo profile)
    {
        return profile.NetworkInterface_BindToInterface
            ? NetworkInterface.FindById(NetworkInterfaces, profile.NetworkInterface_InterfaceId)
            : null;
    }

    /// <summary>
    /// The network interface of the profile is not found on this computer: ask whether the profile should be applied
    /// to the selected network interface instead (only if it can be configured).
    /// </summary>
    /// <returns>The selected network interface (current object after the dialog, the list may have been reloaded
    /// meanwhile), or <see langword="null" /> if canceled or it is no longer selected.</returns>
    private async Task<NetworkInterfaceInfo> ConfirmApplyToSelectedNetworkInterface(ProfileInfo profile)
    {
        var message = string.Format(Strings.NetworkInterfaceOfProfileXNotFoundOnThisComputer, profile.Name) +
                      Environment.NewLine + Environment.NewLine +
                      $"{Strings.Name}: {profile.NetworkInterface_InterfaceName}" + Environment.NewLine +
                      $"{Strings.ID}: {profile.NetworkInterface_InterfaceId}" + Environment.NewLine +
                      Environment.NewLine;

        var selectedNetworkInterface = SelectedNetworkInterface;

        if (selectedNetworkInterface is not { IsOperational: true })
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.NetworkInterfaceNotFound,
                message + Strings.FixNetworkInterfaceOfProfileHint, ChildWindowIcon.Warn);

            return null;
        }

        if (!await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
                Strings.NetworkInterfaceNotFound,
                message + string.Format(Strings.ApplyProfileToSelectedNetworkInterfaceXInstead,
                    selectedNetworkInterface.Name) + Environment.NewLine + Environment.NewLine +
                Strings.FixNetworkInterfaceOfProfileHint, ChildWindowIcon.Warn, Strings.Apply))
            return null;

        // Only the network interface named in the dialog (a reload may have selected another one meanwhile)
        return SelectedNetworkInterface?.Id == selectedNetworkInterface.Id ? SelectedNetworkInterface : null;
    }

    /// <summary>
    /// Shows a (localized) message if a value of the configuration is not a valid IPv4 address or subnet mask. The
    /// model checks it again before running netsh, but its exception is not localized.
    /// </summary>
    /// <returns><see langword="true" /> if the configuration is valid.</returns>
    private bool ValidateConfiguration(NetworkInterfaceConfig config)
    {
        // Profiles can contain values that were not validated by the UI (e.g. edited or shared profile files).
        var invalidValue = config.GetInvalidValue();

        if (invalidValue == null)
            return true;

        // An empty value means the IP address or subnet mask of a static IP address is missing
        StatusMessage = invalidValue.Length == 0
            ? Strings.NetworkInterfaceConfigIPAddressAndSubnetmaskRequired
            : string.Format(Strings.NetworkInterfaceConfigInvalidValueX, invalidValue);
        StatusMessageLevel = StatusLevel.Error;
        IsStatusMessageDisplayed = true;

        return false;
    }

    private Task ApplyConfigurationAsync(NetworkInterfaceInfo networkInterface, NetworkInterfaceConfig config)
    {
        if (!ValidateConfiguration(config))
            return Task.CompletedTask;

        return RunConfigurationAsync(() => NetworkInterface.ConfigureNetworkInterfaceAsync(config),
            Strings.NetworkInterfaceConfigurationAppliedSuccessfully, networkInterface);
    }

    /// <summary>
    /// Runs a configuration action (netsh / ipconfig): only one at a time, the success message or the error (of
    /// Windows) is shown below the configuration. If a network interface is changed, it is remembered (like a
    /// selection by the user: it stays selected after the reload, also if it was only selected as a fallback for a
    /// missing one) and the network interfaces are reloaded afterwards (also after an error).
    /// </summary>
    /// <param name="action">Action to run.</param>
    /// <param name="successMessage">Message shown if the action succeeded.</param>
    /// <param name="networkInterface">Network interface that is changed, or <see langword="null" /> if none is
    /// changed (e.g. flush DNS cache).</param>
    private async Task RunConfigurationAsync(Func<Task> action, string successMessage,
        NetworkInterfaceInfo networkInterface = null)
    {
        if (networkInterface != null)
            RememberNetworkInterface(networkInterface);

        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        try
        {
            await action();

            StatusMessage = successMessage;
            StatusMessageLevel = StatusLevel.Success;
            IsStatusMessageDisplayed = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            StatusMessageLevel = StatusLevel.Error;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsConfigurationRunning = false;

            // Also after an error: the commands run one after another and stop at the first error, the ones before
            // may have changed the configuration (e.g. IP address set, DNS servers failed).
            if (networkInterface != null)
                ReloadNetworkInterfaces();
        }
    }

    private async Task OpenNetworkConnectionsAsync()
    {
        try
        {
            ExternalProcessStarter.RunProcess("NCPA.cpl");
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    private Task FlushDNSAsync()
    {
        return RunConfigurationAsync(NetworkInterface.FlushDnsAsync, Strings.FlushDNSCacheSuccessfully);
    }

    private Task ReleaseRenewAsync(IPConfigReleaseRenewMode releaseRenewMode)
    {
        var networkInterface = SelectedNetworkInterface;

        return RunConfigurationAsync(() => NetworkInterface.ReleaseRenewAsync(releaseRenewMode, networkInterface.Name),
            releaseRenewMode switch
            {
                IPConfigReleaseRenewMode.ReleaseRenew => Strings.IPv4AddressReleasedAndRenewedSuccessfully,
                IPConfigReleaseRenewMode.Release => Strings.IPv4AddressReleasedSuccessfully,
                IPConfigReleaseRenewMode.Renew => Strings.IPv4AddressRenewedSuccessfully,
                IPConfigReleaseRenewMode.ReleaseRenew6 => Strings.IPv6AddressReleasedAndRenewedSuccessfully,
                IPConfigReleaseRenewMode.Release6 => Strings.IPv6AddressReleasedSuccessfully,
                IPConfigReleaseRenewMode.Renew6 => Strings.IPv6AddressRenewedSuccessfully,
                _ => string.Empty
            }, networkInterface);
    }

    /// <summary>
    /// Gets the current object of a network interface a dialog was opened for. The list may have been reloaded while
    /// the dialog was open (new objects) or the network interface may have been removed (e.g. a USB or VPN adapter):
    /// then nothing is changed, instead of changing another network interface that was selected meanwhile.
    /// </summary>
    /// <param name="networkInterface">Network interface the dialog was opened for.</param>
    /// <returns>The current object, or <see langword="null" /> if it no longer exists (a message is shown).</returns>
    private NetworkInterfaceInfo GetCurrentNetworkInterface(NetworkInterfaceInfo networkInterface)
    {
        var currentNetworkInterface = NetworkInterface.FindById(NetworkInterfaces, networkInterface.Id);

        if (currentNetworkInterface != null)
            return currentNetworkInterface;

        StatusMessage = string.Format(Strings.NetworkInterfaceXNotFoundNothingChanged, networkInterface.Name);
        StatusMessageLevel = StatusLevel.Warning;
        IsStatusMessageDisplayed = true;

        return null;
    }

    private Task AddIPv4Address(NetworkInterfaceInfo networkInterface, string ipAddress, string subnetmaskOrCidr)
    {
        networkInterface = GetCurrentNetworkInterface(networkInterface);

        if (networkInterface == null)
            return Task.CompletedTask;

        var config = new NetworkInterfaceConfig
        {
            Name = networkInterface.Name,
            EnableDhcpStaticIpCoexistence = networkInterface.DhcpEnabled,
            IPAddress = ipAddress.Trim(),
            Subnetmask = NetworkInterfaceConfig.ConvertSubnetmaskOrCidr(subnetmaskOrCidr)
        };

        if (!ValidateConfiguration(config))
            return Task.CompletedTask;

        return RunConfigurationAsync(() => NetworkInterface.AddIPAddressToNetworkInterfaceAsync(config),
            Strings.IPv4AddressAddedSuccessfully, networkInterface);
    }

    private Task RemoveIPv4Address(NetworkInterfaceInfo networkInterface, string ipAddress)
    {
        networkInterface = GetCurrentNetworkInterface(networkInterface);

        if (networkInterface == null)
            return Task.CompletedTask;

        var config = new NetworkInterfaceConfig
        {
            Name = networkInterface.Name,
            IPAddress = ipAddress
        };

        if (!ValidateConfiguration(config))
            return Task.CompletedTask;

        return RunConfigurationAsync(() => NetworkInterface.RemoveIPAddressFromNetworkInterfaceAsync(config),
            Strings.IPv4AddressRemovedSuccessfully, networkInterface);
    }

    private void ResetBandwidthChart()
    {
        if (_bandwidthReceivedValues == null)
            return;

        _bandwidthReceivedValues.Clear();
        _bandwidthSentValues.Clear();
        _bandwidthReceivedHistory.Clear();
        _bandwidthSentHistory.Clear();

        _bandwidthSessionStartTime = DateTime.Now;
        IsBandwidthLiveMode = true;

        UpdateBandwidthXAxisWindow(DateTime.Now);
    }

    private void UpdateBandwidthXAxisWindow(DateTime now)
    {
        _updatingBandwidthAxisFromCode = true;
        var axis = BandwidthXAxes[0];
        var elapsed = (now - _bandwidthSessionStartTime).TotalSeconds;
        axis.MinStep = BandwidthChartWindowSeconds / 4.0;
        axis.MinLimit = elapsed - BandwidthChartWindowSeconds;
        axis.MaxLimit = elapsed;
        _updatingBandwidthAxisFromCode = false;
    }

    /// <summary>
    /// Rescales the Y axis to the current values. The step is derived from a 20% padded
    /// max and MaxLimit is set to step * 3, so the top label lands exactly on MaxLimit.
    /// </summary>
    private void UpdateBandwidthYAxis()
    {
        var maxVal = _bandwidthReceivedValues.Concat(_bandwidthSentValues)
            .Where(p => !double.IsNaN(p.Value))
            .Select(p => p.Value)
            .DefaultIfEmpty(0)
            .Max();

        if (!(maxVal > 0))
            return;

        var yAxis = BandwidthYAxes[0];
        var step = Math.Ceiling(maxVal * 1.2 / 3.0);
        yAxis.MinStep = step;
        yAxis.MaxLimit = step * 3;
    }

    private void TrimBandwidthHistory(List<LvlChartsDefaultInfo> history)
    {
        var excess = history.Count - _maxBandwidthValues;
        if (excess > 0)
            history.RemoveRange(0, excess);
    }

    /// <summary>
    /// Gets the command to return the bandwidth chart to live (auto-scrolling) mode.
    /// </summary>
    public ICommand BandwidthGoLiveCommand => new RelayCommand(_ => BandwidthGoLiveAction());

    private void BandwidthGoLiveAction()
    {
        IsBandwidthLiveMode = true;

        // Samples received while inspecting were not added to the chart, so rebuild the
        // rolling buffers from the most recent history to resume at the current time.
        var recentReceived = _bandwidthReceivedHistory
            .Skip(Math.Max(0, _bandwidthReceivedHistory.Count - _maxBandwidthValues));
        var recentSent = _bandwidthSentHistory
            .Skip(Math.Max(0, _bandwidthSentHistory.Count - _maxBandwidthValues));

        _bandwidthReceivedValues = new ObservableCollection<LvlChartsDefaultInfo>(recentReceived);
        _bandwidthSentValues = new ObservableCollection<LvlChartsDefaultInfo>(recentSent);

        ((LineSeries<LvlChartsDefaultInfo>)Series[0]).Values = _bandwidthReceivedValues;
        ((LineSeries<LvlChartsDefaultInfo>)Series[1]).Values = _bandwidthSentValues;

        UpdateBandwidthXAxisWindow(DateTime.Now);
        UpdateBandwidthYAxis();
    }

    private bool _resetBandwidthStatisticOnNextUpdate;

    /// <summary>
    /// The view is created when it is shown for the first time (without OnViewVisible).
    /// </summary>
    private bool _isViewVisible = true;

    private void StartBandwidthMeter(string networkInterfaceId)
    {
        RemoveBandwidthMeter();

        // Reset chart
        ResetBandwidthChart();

        // Reset statistic
        _resetBandwidthStatisticOnNextUpdate = true;

        _bandwidthMeter = new BandwidthMeter(networkInterfaceId);
        _bandwidthMeter.UpdateSpeed += BandwidthMeter_UpdateSpeed;

        // Another adapter can be selected while the view is hidden (e.g. by a reload after a network change): start
        // the measurement when the view becomes visible (ResumeBandwidthMeter).
        if (_isViewVisible)
            _bandwidthMeter.Start();
    }

    private void ResumeBandwidthMeter()
    {
        if (_bandwidthMeter is not { IsRunning: false })
            return;

        // The meter is only paused while this view is hidden (i.e. another application is shown).
        // Returning to it starts a fresh measurement: reset the chart and statistics, then start.
        ResetBandwidthChart();

        _resetBandwidthStatisticOnNextUpdate = true;

        _bandwidthMeter.Start();
    }

    /// <summary>
    /// Stops the meter and removes it (with its event handler), e.g. before a meter for another network interface is
    /// started or if no network interface is left.
    /// </summary>
    private void RemoveBandwidthMeter()
    {
        if (_bandwidthMeter == null)
            return;

        _bandwidthMeter.Stop();
        _bandwidthMeter.UpdateSpeed -= BandwidthMeter_UpdateSpeed;
        _bandwidthMeter = null;
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
    public override void OnViewVisible()
    {
        _isViewVisible = true;

        base.OnViewVisible();

        ResumeBandwidthMeter();
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public override void OnViewHide()
    {
        _isViewVisible = false;

        StopBandwidthMeter();

        base.OnViewHide();
    }

    #endregion

    #region Events

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.NetworkInterface_BandwidthChartTime):
                UpdateMaxBandwidthValues();

                // Re-apply the visible window immediately so the change is reflected while running.
                if (IsBandwidthLiveMode && _bandwidthMeter is { IsRunning: true })
                    UpdateBandwidthXAxisWindow(DateTime.Now);
                break;
        }
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

        // Interface totals (cumulative, since boot) + current speed
        BandwidthTotalBytesReceived = e.TotalBytesReceived;
        BandwidthTotalBytesSent = e.TotalBytesSent;
        BandwidthBytesReceivedSpeed = e.ByteReceivedSpeed;
        BandwidthBytesSentSpeed = e.ByteSentSpeed;

        // A counter reset (e.g. adapter disable/enable, driver reset, sleep/resume) can drop the
        // cumulative totals below the session baseline. Re-baseline in that case so the session
        // amounts below never go negative; they then resume counting from the reset point.
        if (BandwidthTotalBytesReceived < _bandwidthTotalBytesReceivedTemp)
            _bandwidthTotalBytesReceivedTemp = BandwidthTotalBytesReceived;

        if (BandwidthTotalBytesSent < _bandwidthTotalBytesSentTemp)
            _bandwidthTotalBytesSentTemp = BandwidthTotalBytesSent;

        // Amount transferred since the measurement started (this session)
        BandwidthDiffBytesReceived = BandwidthTotalBytesReceived - _bandwidthTotalBytesReceivedTemp;
        BandwidthDiffBytesSent = BandwidthTotalBytesSent - _bandwidthTotalBytesSentTemp;

        // Add chart entry
        var receivedInfo = new LvlChartsDefaultInfo(e.DateTime, e.ByteReceivedSpeed);
        var sentInfo = new LvlChartsDefaultInfo(e.DateTime, e.ByteSentSpeed);

        // Always record history (capped) so the chart can be rebuilt when returning to live mode.
        _bandwidthReceivedHistory.Add(receivedInfo);
        _bandwidthSentHistory.Add(sentInfo);
        TrimBandwidthHistory(_bandwidthReceivedHistory);
        TrimBandwidthHistory(_bandwidthSentHistory);

        // While the user inspects the chart (panned/zoomed, i.e. not live), keep the view
        // frozen: skip updating the visible buffer and axes. New samples are still recorded
        // above and become visible again via BandwidthGoLiveCommand.
        if (!IsBandwidthLiveMode)
            return;

        _bandwidthReceivedValues.Add(receivedInfo);
        _bandwidthSentValues.Add(sentInfo);

        if (_bandwidthReceivedValues.Count > _maxBandwidthValues)
            _bandwidthReceivedValues.RemoveAt(0);

        if (_bandwidthSentValues.Count > _maxBandwidthValues)
            _bandwidthSentValues.RemoveAt(0);

        UpdateBandwidthXAxisWindow(e.DateTime);
        UpdateBandwidthYAxis();
    }

    #endregion
}
