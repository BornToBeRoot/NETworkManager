using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
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

    private readonly bool _isLoading;

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
    /// Gets or sets the currently selected network interface.
    /// </summary>
    public NetworkInterfaceInfo SelectedNetworkInterface
    {
        get;
        set
        {
            if (value == field)
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

            ConfigEnableStaticDNS = true;

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
    /// Gets or sets the selected profile. Pre-fills the <c>Config*</c> fields from the profile.
    /// </summary>
    public override ProfileInfo SelectedProfile
    {
        get => base.SelectedProfile;
        set
        {
            if (value == base.SelectedProfile)
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
        _isLoading = true;

        // Initialize the bandwidth chart before loading interfaces, so the chart collections
        // exist by the time the (possibly synchronous) interface load selects an interface and
        // starts the bandwidth meter.
        InitialBandwidthChart();

        _ = LoadNetworkInterfaces();

        InitializeProfileHost();

        // Detect if network address or status changed...
        NetworkChange.NetworkAvailabilityChanged += (_, _) => ReloadNetworkInterfaces();
        NetworkChange.NetworkAddressChanged += (_, _) => ReloadNetworkInterfaces();

        // React to settings changes (e.g. the configurable bandwidth chart time window)
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        _isLoading = false;
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

        var downloadColor = SKColor.Parse("#1ba1e2");
        var uploadColor = SKColor.Parse("#7fba00");

        var labelColor = Application.Current?.TryFindResource("MahApps.Brushes.Gray5") is System.Windows.Media.SolidColorBrush gray5
            ? new SKColor(gray5.Color.R, gray5.Color.G, gray5.Color.B, gray5.Color.A)
            : new SKColor(0x68, 0x68, 0x68);

        var separatorColor = Application.Current?.TryFindResource("MahApps.Brushes.Gray8") is System.Windows.Media.SolidColorBrush gray8
            ? new SKColor(gray8.Color.R, gray8.Color.G, gray8.Color.B, gray8.Color.A)
            : new SKColor(0x80, 0x80, 0x80);

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
                Stroke = new SolidColorPaint(downloadColor) { StrokeThickness = 1.5f },
                Fill = new SolidColorPaint(downloadColor.WithAlpha(0x33))
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
                Stroke = new SolidColorPaint(uploadColor) { StrokeThickness = 1.5f },
                Fill = new SolidColorPaint(uploadColor.WithAlpha(0x33))
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
                LabelsPaint = new SolidColorPaint(labelColor),
                SeparatorsPaint = new SolidColorPaint(separatorColor)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect([10f, 10f])
                },
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
                LabelsPaint = new SolidColorPaint(labelColor),
                SeparatorsPaint = new SolidColorPaint(separatorColor)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect([10f, 10f])
                }
            }
        ];

        BandwidthLegendTextPaint = new SolidColorPaint(labelColor) { SKTypeface = SKTypeface.Default };

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
    public SolidColorPaint BandwidthLegendTextPaint { get; private set; }

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

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.NetworkInterface;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.NetworkInterface_Enabled;

    // Unlike other tools, profile search here only matches the profile name (no secondary field).
    protected override string GetSearchableField(ProfileInfo profile) => string.Empty;

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
        new RelayCommand(_ => ApplyConfigurationAction(), ConfigureCommands_CanExecute);

    /// <summary>
    /// Action to apply the network configuration.
    /// </summary>
    private void ApplyConfigurationAction()
    {
        _ = ApplyConfiguration();
    }

    /// <summary>
    /// Gets the command to apply the profile configuration.
    /// </summary>
    public ICommand ApplyProfileCommand => new RelayCommand(_ => ApplyProfileAction(), ConfigureCommands_CanExecute);

    private void ApplyProfileAction()
    {
        _ = ApplyConfigurationFromProfile();
    }

    #region Additional commands

    private bool AdditionalCommands_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private bool ConfigureCommands_CanExecute(object parameter) => ConfigurationManager.Current.IsAdmin &&
                                                                   Application.Current.MainWindow != null &&
                                                                   !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
                                                                   !ConfigurationManager.Current.IsChildWindowOpen;

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
    public ICommand FlushDNSCommand => new RelayCommand(_ => FlushDNSAction(), ConfigureCommands_CanExecute);

    private void FlushDNSAction()
    {
        _ = FlushDNSAsync();
    }

    /// <summary>
    /// Gets the command to release and renew IP address.
    /// </summary>
    public ICommand ReleaseRenewCommand => new RelayCommand(_ => ReleaseRenewAction(), ConfigureCommands_CanExecute);

    private void ReleaseRenewAction()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew);
    }

    /// <summary>
    /// Gets the command to release IP address.
    /// </summary>
    public ICommand ReleaseCommand => new RelayCommand(_ => ReleaseAction(), ConfigureCommands_CanExecute);

    private void ReleaseAction()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Release);
    }

    /// <summary>
    /// Gets the command to renew IP address.
    /// </summary>
    public ICommand RenewCommand => new RelayCommand(_ => RenewAction(), ConfigureCommands_CanExecute);

    private void RenewAction()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew);
    }

    /// <summary>
    /// Gets the command to release and renew IPv6 address.
    /// </summary>
    public ICommand ReleaseRenew6Command => new RelayCommand(_ => ReleaseRenew6Action(), ConfigureCommands_CanExecute);

    private void ReleaseRenew6Action()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.ReleaseRenew6);
    }

    /// <summary>
    /// Gets the command to release IPv6 address.
    /// </summary>
    public ICommand Release6Command => new RelayCommand(_ => Release6Action(), ConfigureCommands_CanExecute);

    private void Release6Action()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Release6);
    }

    /// <summary>
    /// Gets the command to renew IPv6 address.
    /// </summary>
    public ICommand Renew6Command => new RelayCommand(_ => Renew6Action(), ConfigureCommands_CanExecute);

    private void Renew6Action()
    {
        _ = ReleaseRenewAsync(IPConfigReleaseRenewMode.Renew6);
    }

    /// <summary>
    /// Gets the command to add an IPv4 address.
    /// </summary>
    public ICommand AddIPv4AddressCommand => new RelayCommand(parameter => { _ = AddIPv4AddressAction(); },
        ConfigureCommands_CanExecute);

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
    public ICommand RemoveIPv4AddressCommand => new RelayCommand(parameter => { _ = RemoveIPv4AddressAction(); },
        ConfigureCommands_CanExecute);

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
            await NetworkInterface.ConfigureNetworkInterfaceAsync(config);

            ReloadNetworkInterfaces();

            StatusMessage = Strings.NetworkInterfaceConfigurationAppliedSuccessfully;
            IsStatusMessageDisplayed = true;
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
            await NetworkInterface.ConfigureNetworkInterfaceAsync(config);

            ReloadNetworkInterfaces();

            StatusMessage = Strings.NetworkInterfaceConfigurationAppliedSuccessfully;
            IsStatusMessageDisplayed = true;
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
            ExternalProcessStarter.RunProcess("NCPA.cpl");
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

        try
        {
            await NetworkInterface.FlushDnsAsync();

            StatusMessage = Strings.FlushDNSCacheSuccessfully;
            IsStatusMessageDisplayed = true;
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

    private async Task ReleaseRenewAsync(IPConfigReleaseRenewMode releaseRenewMode)
    {
        IsConfigurationRunning = true;
        IsStatusMessageDisplayed = false;

        try
        {
            await NetworkInterface.ReleaseRenewAsync(releaseRenewMode, SelectedNetworkInterface.Name);

            ReloadNetworkInterfaces();

            StatusMessage = releaseRenewMode switch
            {
                IPConfigReleaseRenewMode.ReleaseRenew => Strings.IPv4AddressReleasedAndRenewedSuccessfully,
                IPConfigReleaseRenewMode.Release => Strings.IPv4AddressReleasedSuccessfully,
                IPConfigReleaseRenewMode.Renew => Strings.IPv4AddressRenewedSuccessfully,
                IPConfigReleaseRenewMode.ReleaseRenew6 => Strings.IPv6AddressReleasedAndRenewedSuccessfully,
                IPConfigReleaseRenewMode.Release6 => Strings.IPv6AddressReleasedSuccessfully,
                IPConfigReleaseRenewMode.Renew6 => Strings.IPv6AddressRenewedSuccessfully,
                _ => string.Empty
            };
            IsStatusMessageDisplayed = true;
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

            StatusMessage = Strings.IPv4AddressAddedSuccessfully;
            IsStatusMessageDisplayed = true;
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

            StatusMessage = Strings.IPv4AddressRemovedSuccessfully;
            IsStatusMessageDisplayed = true;
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

        // The meter is only paused while this view is hidden (i.e. another application is shown).
        // Returning to it starts a fresh measurement: reset the chart and statistics, then start.
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
    public override void OnViewVisible()
    {
        base.OnViewVisible();

        ResumeBandwidthMeter();
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public override void OnViewHide()
    {
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
