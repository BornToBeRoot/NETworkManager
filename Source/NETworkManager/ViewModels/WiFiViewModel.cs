using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using Windows.Devices.WiFi;
using Windows.Foundation.Metadata;
using Windows.Security.Credentials;
using Windows.System;

namespace NETworkManager.ViewModels;

public class WiFiViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(WiFiViewModel));

    private readonly bool _isLoading;
    private readonly DispatcherTimer _autoRefreshTimer = new();
    private readonly DispatcherTimer _hideConnectionStatusMessageTimer = new();

    public bool SdkContractAvailable
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

    public bool WiFiAdapterAccessEnabled
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

    public bool IsAdaptersLoading
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

    public List<WiFiAdapterInfo> Adapters
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

    public WiFiAdapterInfo SelectedAdapter
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value != null)
            {
                if (!_isLoading)
                    SettingsManager.Current.WiFi_InterfaceId = value.NetworkInterfaceInfo.Id;

                _ = ScanAsync(value);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsNetworksLoading
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

    public bool AutoRefreshEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_AutoRefreshEnabled = value;

            field = value;

            // Start timer to refresh automatically
            if (value)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(SelectedAutoRefreshTime);
                _autoRefreshTimer.Start();
            }
            else
            {
                _autoRefreshTimer.Stop();
            }

            OnPropertyChanged();
        }
    }

    public ICollectionView AutoRefreshTimes { get; }

    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_AutoRefreshTime = value;

            field = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    public bool Show2Dot4GHzNetworks
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_Show2dot4GHzNetworks = value;

            field = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    public bool Show5GHzNetworks
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_Show5GHzNetworks = value;

            field = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    public bool Show6GHzNetworks
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_Show6GHzNetworks = value;

            field = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    public ObservableCollection<WiFiNetworkInfo> Networks
    {
        get;
        init
        {
            if (value != null && value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ICollectionView NetworksView { get; }

    public WiFiNetworkInfo SelectedNetwork
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

    public IList SelectedNetworks
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    public ISeries[] Radio2Dot4GHzSeries
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ISeries[] Radio5GHzSeries
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    // 6 GHz spans a very wide range (channels 1-233). It is split into a lower (1-125) and an upper
    // (129-233) chart for readability, similar to the UniFi channel view.
    public ISeries[] Radio6GHzLowerSeries
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ISeries[] Radio6GHzUpperSeries
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public Axis[] Radio2Dot4GHzXAxes { get; private set; }
    public Axis[] Radio5GHzXAxes { get; private set; }
    public Axis[] Radio6GHzLowerXAxes { get; private set; }
    public Axis[] Radio6GHzUpperXAxes { get; private set; }

    public Axis[] Radio2Dot4GHzYAxes { get; private set; }
    public Axis[] Radio5GHzYAxes { get; private set; }
    public Axis[] Radio6GHzLowerYAxes { get; private set; }
    public Axis[] Radio6GHzUpperYAxes { get; private set; }

    public RectangularSection[] Radio2Dot4GHzSections { get; private set; }
    public RectangularSection[] Radio5GHzSections { get; private set; }
    public RectangularSection[] Radio6GHzLowerSections { get; private set; }
    public RectangularSection[] Radio6GHzUpperSections { get; private set; }

    public WiFiChannelLegendEntry[] Radio2Dot4GHzLegend
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public WiFiChannelLegendEntry[] Radio5GHzLegend
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public WiFiChannelLegendEntry[] Radio6GHzLowerLegend
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public WiFiChannelLegendEntry[] Radio6GHzUpperLegend
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

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

    public bool IsBackgroundSearchRunning
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

    public bool IsConnecting
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

    public bool IsConnectionStatusMessageDisplayed
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

    public string ConnectionStatusMessage
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

    #endregion

    #region Constructor, load settings

    public WiFiViewModel()
    {
        _isLoading = true;

        // Set up the channel charts (axes, sections, paints) unconditionally so the chart bindings
        // are never null, even on the code paths that return early below.
        InitializeCharts();

        // Check if Microsoft.Windows.SDK.Contracts is available
        SdkContractAvailable = ApiInformation.IsTypePresent("Windows.Devices.WiFi.WiFiAdapter");

        if (!SdkContractAvailable)
        {
            _isLoading = false;

            return;
        }

        // Check if the access is denied and show a message
        WiFiAdapterAccessEnabled = RequestAccess();

        if (!WiFiAdapterAccessEnabled)
        {
            _isLoading = false;

            return;
        }

        // Result view + search
        NetworksView = CollectionViewSource.GetDefaultView(Networks);
        NetworksView.SortDescriptions.Add(new SortDescription(
            $"{nameof(WiFiNetworkInfo.AvailableNetwork)}.{nameof(WiFiNetworkInfo.AvailableNetwork.Ssid)}",
            ListSortDirection.Ascending));
        NetworksView.Filter = o =>
        {
            if (o is not WiFiNetworkInfo info)
                return false;

            // Filter by frequency
            if ((info.Radio == WiFiRadio.GHz2dot4 && !Show2Dot4GHzNetworks) ||
                (info.Radio == WiFiRadio.GHz5 && !Show5GHzNetworks) ||
                (info.Radio == WiFiRadio.GHz6 && !Show6GHzNetworks))
            {
                return false;
            }

            // Return true if no search term is set
            if (string.IsNullOrEmpty(Search))
                return true;

            // Search by SSID, authentication type, channel frequency, channel, BSSID, vendor and PHY kind
            return info.AvailableNetwork.Ssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.NetworkAuthenticationType.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{info.ChannelCenterFrequencyInGigahertz}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{info.Channel}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.AvailableNetwork.Bssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Vendor.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.PhyKind.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Load network adapters
        _ = LoadAdaptersAsync(SettingsManager.Current.WiFi_InterfaceId);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.WiFi_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.WiFi_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.WiFi_AutoRefreshEnabled;

        // Hide ConnectionStatusMessage automatically
        _hideConnectionStatusMessageTimer.Interval = new TimeSpan(0, 0, 15);
        _hideConnectionStatusMessageTimer.Tick += HideConnectionStatusMessageTimer_Tick;

        // Load settings
        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        Show2Dot4GHzNetworks = SettingsManager.Current.WiFi_Show2dot4GHzNetworks;
        Show5GHzNetworks = SettingsManager.Current.WiFi_Show5GHzNetworks;
        Show6GHzNetworks = SettingsManager.Current.WiFi_Show6GHzNetworks;
    }

    #endregion

    #region ICommands & Actions

    public ICommand ReloadAdaptersCommand => new RelayCommand(_ => ReloadAdapterAction(), ReloadAdapter_CanExecute);

    private bool ReloadAdapter_CanExecute(object obj)
    {
        return !IsAdaptersLoading && !IsNetworksLoading && !IsBackgroundSearchRunning && !AutoRefreshEnabled && !IsConnecting;
    }

    private void ReloadAdapterAction()
    {
        _ = LoadAdaptersAsync(SelectedAdapter?.NetworkInterfaceInfo.Id);
    }

    public ICommand ScanNetworksCommand =>
        new RelayCommand(parameter => { _ = ScanNetworksAction(); }, ScanNetworks_CanExecute);

    private bool ScanNetworks_CanExecute(object obj)
    {
        return !IsAdaptersLoading && !IsNetworksLoading && !IsBackgroundSearchRunning && !AutoRefreshEnabled && !IsConnecting;
    }

    private async Task ScanNetworksAction()
    {
        await ScanAsync(SelectedAdapter, true);
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction());

    private void ConnectAction()
    {
        Connect();
    }

    public ICommand DisconnectCommand => new RelayCommand(_ => DisconnectAction());

    private void DisconnectAction()
    {
        Disconnect();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Request access to the Wi-Fi adapter.
    /// </summary>
    /// <returns>Fails if the access is denied.</returns>
    private static bool RequestAccess()
    {
        var accessStatus = WiFiAdapter.RequestAccessAsync().GetAwaiter().GetResult();

        return accessStatus == WiFiAccessStatus.Allowed;
    }

    private async Task LoadAdaptersAsync(string adapterId = null)
    {
        Log.Debug("LoadAdaptersAsync - Trying to get WiFi adapters...");

        IsAdaptersLoading = true;

        // Show a loading animation for the user
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        try
        {
            Adapters = await WiFi.GetAdapterAsync();
        }
        catch (Exception ex)
        {
            Log.Error("Error trying to get WiFi adapters.", ex);

            Adapters.Clear();
        }

        // Check if we found any adapters
        if (Adapters.Count > 0)
        {
            Log.Debug("LoadAdaptersAsync - Found " + Adapters.Count + " WiFi adapters.");

            // Check for previous selected adapter
            if (string.IsNullOrEmpty(adapterId))
            {
                Log.Debug("LoadAdaptersAsync - No previous adapter ID found. Selecting the first adapter.");

                SelectedAdapter = Adapters.FirstOrDefault();
            }
            else
            {
                Log.Debug("LoadAdaptersAsync - Previous adapter ID found. Trying to select the adapter with the ID: " + adapterId);

                SelectedAdapter = Adapters.FirstOrDefault(s => s.NetworkInterfaceInfo.Id.ToString() == adapterId) ??
                                  Adapters.FirstOrDefault();
            }

            Log.Debug("LoadAdaptersAsync - Selected adapter: " + SelectedAdapter?.NetworkInterfaceInfo.Name + " (" +
                      SelectedAdapter?.NetworkInterfaceInfo.Id + ")");
        }
        else
        {
            Log.Debug("LoadAdaptersAsync - No WiFi adapters found.");
        }

        IsAdaptersLoading = false;

        Log.Debug("LoadAdaptersAsync - Done.");
    }

    private async Task ScanAsync(WiFiAdapterInfo adapterInfo, bool refreshing = false, int delayInMs = 0)
    {
        Log.Debug($"ScanAsync - Scanning WiFi adapter \"{adapterInfo.NetworkInterfaceInfo.Name}\" with delay of {delayInMs} ms...");

        if (refreshing)
        {
            StatusMessage = Strings.SearchingForNetworksDots;
            IsBackgroundSearchRunning = true;
        }
        else
        {
            IsStatusMessageDisplayed = false;
            IsNetworksLoading = true;
        }

        if (delayInMs != 0)
            await Task.Delay(delayInMs);

        var statusMessage = string.Empty;

        try
        {
            var wiFiNetworkScanInfo = await WiFi.GetNetworksAsync(adapterInfo.WiFiAdapter);

            Log.Debug("ScanAsync - Scan completed. Found " + wiFiNetworkScanInfo.WiFiNetworkInfos.Count + " networks.");

            // Clear the values after the scan to make the UI smoother
            Log.Debug("ScanAsync - Clearing old values...");
            Networks.Clear();

            Log.Debug("ScanAsync - Adding new values...");
            List<ISeries> series2Dot4GHz = [];
            List<ISeries> series5GHz = [];
            List<ISeries> series6GHzLower = [];
            List<ISeries> series6GHzUpper = [];
            List<WiFiChannelLegendEntry> legend2Dot4GHz = [];
            List<WiFiChannelLegendEntry> legend5GHz = [];
            List<WiFiChannelLegendEntry> legend6GHzLower = [];
            List<WiFiChannelLegendEntry> legend6GHzUpper = [];

            foreach (var network in wiFiNetworkScanInfo.WiFiNetworkInfos)
            {
                Log.Debug("ScanAsync - Add network: " + network.AvailableNetwork.Ssid + " with channel frequency: " +
                          network.AvailableNetwork.ChannelCenterFrequencyInKilohertz);

                Networks.Add(network);

                switch (network.Radio)
                {
                    case WiFiRadio.GHz2dot4:
                        var (s24, l24) = BuildNetworkSeries(network, series2Dot4GHz.Count);
                        series2Dot4GHz.Add(s24);
                        legend2Dot4GHz.Add(l24);
                        break;

                    case WiFiRadio.GHz5:
                        var (s5, l5) = BuildNetworkSeries(network, series5GHz.Count);
                        series5GHz.Add(s5);
                        legend5GHz.Add(l5);
                        break;

                    case WiFiRadio.GHz6:
                        // Split by channel center into the lower (1-125) and upper (129-233) chart.
                        var centerChannel = FrequencyToChannelAxis(
                            network.ChannelCenterFrequencyInGigahertz * 1000, WiFiRadio.GHz6);

                        if (centerChannel < SixGHzSplitChannel)
                        {
                            var (s6L, l6L) = BuildNetworkSeries(network, series6GHzLower.Count);
                            series6GHzLower.Add(s6L);
                            legend6GHzLower.Add(l6L);
                        }
                        else
                        {
                            var (s6U, l6U) = BuildNetworkSeries(network, series6GHzUpper.Count);
                            series6GHzUpper.Add(s6U);
                            legend6GHzUpper.Add(l6U);
                        }

                        break;
                }
            }

            Radio2Dot4GHzSeries = [.. series2Dot4GHz];
            Radio5GHzSeries = [.. series5GHz];
            Radio6GHzLowerSeries = [.. series6GHzLower];
            Radio6GHzUpperSeries = [.. series6GHzUpper];
            Radio2Dot4GHzLegend = [.. legend2Dot4GHz];
            Radio5GHzLegend = [.. legend5GHz];
            Radio6GHzLowerLegend = [.. legend6GHzLower];
            Radio6GHzUpperLegend = [.. legend6GHzUpper];

            statusMessage = string.Format(Strings.LastScanAtX,
                wiFiNetworkScanInfo.Timestamp.ToLongTimeString());
        }
        catch (Exception ex)
        {
            Log.Error($"Error while scanning WiFi adapter \"{adapterInfo.NetworkInterfaceInfo.Name}\".", ex);

            statusMessage = string.Format(Strings.ErrorWhileScanningWiFiAdapterXXXWithErrorXXX,
                adapterInfo.NetworkInterfaceInfo.Name, ex.Message);

            // Clear the existing old values if an error occurs
            Networks.Clear();
            Radio2Dot4GHzSeries = [];
            Radio5GHzSeries = [];
            Radio6GHzLowerSeries = [];
            Radio6GHzUpperSeries = [];
            Radio2Dot4GHzLegend = [];
            Radio5GHzLegend = [];
            Radio6GHzLowerLegend = [];
            Radio6GHzUpperLegend = [];
        }
        finally
        {
            IsStatusMessageDisplayed = true;
            StatusMessage = statusMessage;

            IsBackgroundSearchRunning = false;
            IsNetworksLoading = false;

            Log.Debug("ScanAsync - Done.");
        }
    }

    /// <summary>
    ///     Color palette used to assign a distinct, deterministic color to each network series.
    ///     Assigning the stroke explicitly (instead of relying on the LiveCharts2 theme) keeps the
    ///     legend, the chart and the tooltip in sync.
    /// </summary>
    private static readonly SKColor[] ChartPalette =
    [
        SKColor.Parse("#1ba1e2"), SKColor.Parse("#a4c400"), SKColor.Parse("#f0a30a"),
        SKColor.Parse("#e51400"), SKColor.Parse("#6a00ff"), SKColor.Parse("#00aba9"),
        SKColor.Parse("#d80073"), SKColor.Parse("#60a917"), SKColor.Parse("#fa6800"),
        SKColor.Parse("#0050ef"), SKColor.Parse("#aa00ff"), SKColor.Parse("#825a2c")
    ];

    // The Y axis is plotted in 0..100 space (signal strength + 100) so the area fill drops to the
    // bottom baseline; the axis labeler converts back to real dBm for display.
    private const double SignalOffset = 100;

    // 2.4 GHz operating channels are 1, 2, 3, ... 14 (every channel number), labeled with the channel
    private static readonly HashSet<int> ValidChannels2Dot4GHz = BuildChannelSet(1, 14, 1);

    // 5 GHz operating channels are 36, 40, 44, ... 165 (every 4 in channel-number space), labeled with
    private static readonly HashSet<int> ValidChannels5GHz =
    [
        .. BuildChannelSet(36, 64, 4),
        .. BuildChannelSet(100, 165, 4)
    ];

    // 6 GHz operating channels are 1, 5, 9, ... 233 (every 4 in channel-number space), labeled with the
    private static readonly HashSet<int> ValidChannels6GHz = BuildChannelSet(1, 233, 4);

    // Channel that splits the 6 GHz band into the lower (1-125) and upper (129-233) chart. 127 sits
    // cleanly between the operating channels 125 and 129.
    private const int SixGHzSplitChannel = 127;

    // The 5 GHz band has a large unused range between channel 64 (UNII-2) and channel 100
    // (UNII-2 Extended). On a frequency-linear axis this would render as dead space, so it is
    // compressed: channels above 64 are shifted left, leaving only a small visual gap that still
    // provides room for the channel-width trapezoids.
    private const int FiveGHzGapStartChannel = 64;
    private const int FiveGHzGapEndChannel = 100;
    private const int FiveGHzGapDisplayUnits = 16;
    private const int FiveGHzGapShift = FiveGHzGapEndChannel - FiveGHzGapStartChannel - FiveGHzGapDisplayUnits;

    private static HashSet<int> BuildChannelSet(int first, int last, int step)
    {
        var set = new HashSet<int>();

        for (var channel = first; channel <= last; channel += step)
            set.Add(channel);

        return set;
    }

    /// <summary>
    ///     Builds axes, signal-quality sections and paints for all channel charts. Called once from
    ///     the constructor; the per-network series are (re)built on each scan.
    /// </summary>
    private void InitializeCharts()
    {
        var labelColor = Application.Current?.TryFindResource("MahApps.Brushes.Gray5") is SolidColorBrush gray5
            ? new SKColor(gray5.Color.R, gray5.Color.G, gray5.Color.B, gray5.Color.A)
            : new SKColor(0x68, 0x68, 0x68);

        var separatorColor = Application.Current?.TryFindResource("MahApps.Brushes.Gray8") is SolidColorBrush gray8
            ? new SKColor(gray8.Color.R, gray8.Color.G, gray8.Color.B, gray8.Color.A)
            : new SKColor(0x80, 0x80, 0x80);

        // (min, max) in display space. The lower bound is extended below the first channel so the
        // left flank of its trapezoid reaches the baseline instead of being clipped. The 5 GHz
        // upper bound is reduced because the 64..100 dead zone is compressed.
        Radio2Dot4GHzXAxes = BuildXAxes(-2, 16, ValidChannels2Dot4GHz, false, labelColor);
        Radio5GHzXAxes = BuildXAxes(30, 152, ValidChannels5GHz, true, labelColor);
        Radio6GHzLowerXAxes = BuildXAxes(-2, 128, ValidChannels6GHz, false, labelColor);
        Radio6GHzUpperXAxes = BuildXAxes(126, 236, ValidChannels6GHz, false, labelColor);

        Radio2Dot4GHzYAxes = BuildYAxes(labelColor, separatorColor);
        Radio5GHzYAxes = BuildYAxes(labelColor, separatorColor);
        Radio6GHzLowerYAxes = BuildYAxes(labelColor, separatorColor);
        Radio6GHzUpperYAxes = BuildYAxes(labelColor, separatorColor);

        Radio2Dot4GHzSections = BuildSections();
        Radio5GHzSections = BuildSections();
        Radio6GHzLowerSections = BuildSections();
        Radio6GHzUpperSections = BuildSections();
    }

    /// <summary>
    ///     Builds an X-axis in channel-number space (which is linear with the channel frequency).
    ///     Only channels contained in <paramref name="validChannels" /> are labeled; all other tick
    ///     positions stay blank. When <paramref name="applyFiveGHzGap" /> is set, the 5 GHz gap shift
    ///     is reversed before the label lookup.
    /// </summary>
    private static Axis[] BuildXAxes(double min, double max, HashSet<int> validChannels, bool applyFiveGHzGap,
        SKColor labelColor)
    {
        return
        [
            new Axis
            {
                Name = Strings.Channel,
                NamePaint = new SolidColorPaint(labelColor),
                NameTextSize = 11,
                MinLimit = min,
                MaxLimit = max,
                MinStep = 1,
                ForceStepToMin = true,
                Labeler = value =>
                {
                    var channel = (int)Math.Round(value);

                    if (applyFiveGHzGap && channel > FiveGHzGapStartChannel)
                        channel += FiveGHzGapShift;

                    return validChannels.Contains(channel) ? channel.ToString() : string.Empty;
                },
                TextSize = 10,
                Padding = new Padding(0, 4, 0, 0),
                LabelsPaint = new SolidColorPaint(labelColor),
                // No vertical grid lines (matches the legacy chart).
                SeparatorsPaint = null
            }
        ];
    }

    /// <summary>
    ///     Builds the Y-axis (signal strength in dBm, -100..0) shared by all three channel charts.
    /// </summary>
    private static Axis[] BuildYAxes(SKColor labelColor, SKColor separatorColor)
    {
        return
        [
            new Axis
            {
                Name = Strings.SignalStrength,
                NamePaint = new SolidColorPaint(labelColor),
                NameTextSize = 11,
                MinLimit = 0,
                MaxLimit = 100,
                MinStep = 10,
                ForceStepToMin = true,
                // Plotted in 0..100 space; convert back to real dBm for the label.
                Labeler = value => $"{(int)Math.Round(value) - (int)SignalOffset} dBm",
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
    }

    /// <summary>
    ///     Builds the colored signal-quality bands (Cisco/MetaGeek thresholds), identical for all
    ///     bands. Returns fresh instances so they are not shared between charts.
    /// </summary>
    private static RectangularSection[] BuildSections()
    {
        // Thresholds in 0..100 space (dBm + 100): -30 -> 70, -67 -> 33, -70 -> 30, -80 -> 20.
        return
        [
            NewSection(70, 100, "#5EA4BF"), // Excellent (>= -30 dBm)
            NewSection(33, 70, "#badc58"), // Good (-67…-30 dBm)
            NewSection(30, 33, "#f9ca24"), // Reliable (-70…-67 dBm)
            NewSection(20, 30, "#FF970D"), // Weak (-80…-70 dBm)
            NewSection(0, 20, "#A4442B") // Poor (< -80 dBm)
        ];
    }

    private static RectangularSection NewSection(double yi, double yj, string color)
    {
        return new RectangularSection
        {
            Yi = yi,
            Yj = yj,
            Fill = new SolidColorPaint(SKColor.Parse(color).WithAlpha(0x40))
        };
    }

    /// <summary>
    ///     Builds a line series and a matching legend entry for a single network, rendered as a
    ///     trapezoid centered on its channel center frequency.
    /// </summary>
    private static (LineSeries<WiFiChannelPoint> Series, WiFiChannelLegendEntry Legend) BuildNetworkSeries(WiFiNetworkInfo network, int colorIndex)
    {
        var color = ChartPalette[colorIndex % ChartPalette.Length];

        double dbm = network.AvailableNetwork.NetworkRssiInDecibelMilliwatts;
        var center = ChannelNumberToDisplay(
            FrequencyToChannelAxis(network.ChannelCenterFrequencyInGigahertz * 1000, network.Radio), network.Radio);

        // Channel-number space uses 5 MHz per unit, so a bandwidth in MHz spans bandwidth/5 units.
        var bandwidth = network.ChannelBandwidth;
        var half = bandwidth / 10.0;
        var inner = half * 0.7;
        const double floor = -100; // baseline in real dBm; mapped to 0 in the chart's 0..100 space

        // Dense points at 0.5-channel steps so that CompareOnlyX always finds this series when the
        // mouse is anywhere inside the trapezoid, even when a narrower network shares the same area.
        // With only 5 corner points the nearest point of a wide network can be far from the mouse
        // while a narrower network's corner is closer, causing the wide network to be omitted from
        // the tooltip despite the mouse being visually inside it.
        const double step = 0.5;
        var rampWidth = half - inner; // > 0 always (inner = half * 0.7)
        var pointList = new List<WiFiChannelPoint>();

        for (var x = center - half; x <= center + half + step * 0.01; x += step)
        {
            double y;

            if (x <= center - inner)
                y = floor + (dbm - floor) * (x - (center - half)) / rampWidth;
            else if (x >= center + inner)
                y = dbm - (dbm - floor) * (x - (center + inner)) / rampWidth;
            else
                y = dbm;

            pointList.Add(new WiFiChannelPoint(x, y, network));
        }

        var points = pointList.ToArray();

        var name = network.IsHidden
            ? $"{Strings.HiddenNetwork} ({network.AvailableNetwork.Bssid})"
            : $"{network.AvailableNetwork.Ssid} ({network.AvailableNetwork.Bssid})";

        var series = new LineSeries<WiFiChannelPoint>
        {
            Name = name,
            Values = points,
            // Plot in 0..100 space (dBm + 100) so the area fill drops to the bottom baseline.
            Mapping = (point, _) => new Coordinate(point.ChannelAxis, point.Dbm + SignalOffset),
            GeometrySize = 0,
            LineSmoothness = 0,
            Stroke = new SolidColorPaint(color) { StrokeThickness = 1.5f },
            Fill = new SolidColorPaint(color.WithAlpha(0x33))
        };

        var wpfColor = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        var legendSsid = network.IsHidden ? Strings.HiddenNetwork : network.AvailableNetwork.Ssid;
        var legend = new WiFiChannelLegendEntry(new SolidColorBrush(wpfColor), legendSsid, network.AvailableNetwork.Bssid);

        return (series, legend);
    }

    /// <summary>
    ///     Converts a channel center frequency (MHz) to channel-number space for the chart X axis.
    ///     This mapping is linear with frequency (5 MHz per unit).
    /// </summary>
    private static double FrequencyToChannelAxis(double frequencyMHz, WiFiRadio radio)
    {
        return radio switch
        {
            // Channel 14 is at 2484 MHz and does not follow the standard ch×5 + 2407 spacing.
            WiFiRadio.GHz2dot4 => (int)Math.Round(frequencyMHz) == 2484 ? 14.0 : (frequencyMHz - 2407) / 5.0,
            WiFiRadio.GHz5 => (frequencyMHz - 5000) / 5.0,
            WiFiRadio.GHz6 => (frequencyMHz - 5950) / 5.0,
            _ => 0
        };
    }

    /// <summary>
    ///     Maps a channel number to its display position on the X axis. For 5 GHz the unused
    ///     64..100 range is compressed so it does not render as dead space.
    /// </summary>
    private static double ChannelNumberToDisplay(double channelNumber, WiFiRadio radio)
    {
        if (radio == WiFiRadio.GHz5 && channelNumber > FiveGHzGapStartChannel)
            return channelNumber - FiveGHzGapShift;

        return channelNumber;
    }

    private void Connect()
    {
        var selectedAdapter = SelectedAdapter;
        var selectedNetwork = SelectedNetwork;

        var connectMode = WiFi.GetConnectMode(selectedNetwork.AvailableNetwork);

        var childWindow = new WiFiConnectChildWindow();

        var childWindowViewModel = new WiFiConnectViewModel(async instance =>
        {
            // Connect Open/PSK/EAP
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            var ssid = selectedNetwork.IsHidden ? instance.Ssid : selectedNetwork.AvailableNetwork.Ssid;

            // Show status message
            IsConnecting = true;
            ConnectionStatusMessage = string.Format(Strings.ConnectingToXXX, ssid);
            IsConnectionStatusMessageDisplayed = true;

            // Connect to the network
            var reconnectionKind = instance.ConnectAutomatically
                ? WiFiReconnectionKind.Automatic
                : WiFiReconnectionKind.Manual;

            PasswordCredential credential = new();

            switch (instance.ConnectMode)
            {
                case WiFiConnectMode.Psk:
                    credential.Password = SecureStringHelper.ConvertToString(instance.PreSharedKey);
                    break;
                case WiFiConnectMode.Eap:
                    credential.UserName = instance.Username;

                    if (!string.IsNullOrEmpty(instance.Domain))
                        credential.Resource = instance.Domain;

                    credential.Password = SecureStringHelper.ConvertToString(instance.Password);
                    break;
            }

            WiFiConnectionStatus connectionResult;

            if (selectedNetwork.IsHidden)
                connectionResult = await WiFi.ConnectAsync(instance.Options.AdapterInfo.WiFiAdapter,
                    instance.Options.NetworkInfo.AvailableNetwork, reconnectionKind, credential, instance.Ssid);
            else
                connectionResult = await WiFi.ConnectAsync(instance.Options.AdapterInfo.WiFiAdapter,
                    instance.Options.NetworkInfo.AvailableNetwork, reconnectionKind, credential);

            // Done connecting
            IsConnecting = false;

            // Get result
            ConnectionStatusMessage = connectionResult == WiFiConnectionStatus.Success
                ? string.Format(Strings.SuccessfullyConnectedToXXX, ssid)
                : string.Format(Strings.CouldNotConnectToXXXReasonXXX, ssid,
                    ResourceTranslator.Translate(ResourceIdentifier.WiFiConnectionStatus, connectionResult));

            // Hide message automatically
            _hideConnectionStatusMessageTimer.Start();

            // Update the Wi-Fi networks.
            // Wait because an error may occur if a refresh is done directly after connecting.            
            await ScanAsync(SelectedAdapter, true, 5000);
        }, async instance =>
        {
            // Connect WPS
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            var ssid = selectedNetwork.IsHidden ? instance.Ssid : selectedNetwork.AvailableNetwork.Ssid;

            // Show status message
            IsConnecting = true;
            ConnectionStatusMessage = string.Format(Strings.ConnectingToXXX, ssid);
            IsConnectionStatusMessageDisplayed = true;

            // Connect to the network
            var reconnectionKind = instance.ConnectAutomatically
                ? WiFiReconnectionKind.Automatic
                : WiFiReconnectionKind.Manual;

            var connectionResult = await WiFi.ConnectWpsAsync(
                instance.Options.AdapterInfo.WiFiAdapter, instance.Options.NetworkInfo.AvailableNetwork,
                reconnectionKind);

            // Done connecting
            IsConnecting = false;

            // Get result
            ConnectionStatusMessage = connectionResult == WiFiConnectionStatus.Success
                ? string.Format(Strings.SuccessfullyConnectedToXXX, ssid)
                : string.Format(Strings.CouldNotConnectToXXXReasonXXX, ssid,
                    ResourceTranslator.Translate(ResourceIdentifier.WiFiConnectionStatus, connectionResult));

            // Hide message automatically
            _hideConnectionStatusMessageTimer.Start();

            // Update the Wi-Fi networks.
            // Wait because an error may occur if a refresh is done directly after connecting.            
            await ScanAsync(SelectedAdapter, true, 5000);
        },
            _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            }, (selectedAdapter, selectedNetwork),
            connectMode);


        childWindow.Title = selectedNetwork.IsHidden
                ? Strings.HiddenNetwork
                : string.Format(Strings.ConnectToXXX, selectedNetwork.AvailableNetwork.Ssid);

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        _ = Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private async void Disconnect()
    {
        var connectedNetwork = Networks.FirstOrDefault(x => x.IsConnected);

        WiFi.Disconnect(SelectedAdapter.WiFiAdapter);

        if (connectedNetwork != null)
        {
            ConnectionStatusMessage = string.Format(Strings.XXXDisconnected,
                connectedNetwork.AvailableNetwork.Ssid);
            IsConnectionStatusMessageDisplayed = true;

            // Hide message automatically
            _hideConnectionStatusMessageTimer.Start();
        }

        // Refresh
        await ScanAsync(SelectedAdapter, true, GlobalStaticConfiguration.ApplicationUIRefreshInterval);
    }

    private Task Export()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Networks
                        : new ObservableCollection<WiFiNetworkInfo>(SelectedNetworks.Cast<WiFiNetworkInfo>()
                            .ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.WiFi_ExportFileType = instance.FileType;
            SettingsManager.Current.WiFi_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.WiFi_ExportFileType,
        SettingsManager.Current.WiFi_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    public void OnViewVisible()
    {
        // Temporarily stop timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Stop();
    }

    public void OnViewHide()
    {
        // Temporarily stop timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Stop();
    }

    #endregion

    #region Events

    private async void AutoRefreshTimer_Tick(object sender, EventArgs e)
    {
        // Don't refresh if it's already loading or connecting
        if (IsNetworksLoading || IsBackgroundSearchRunning || IsConnecting)
            return;

        // Stop timer...
        _autoRefreshTimer.Stop();

        // Scan networks
        await ScanAsync(SelectedAdapter, true);

        // Restart timer...
        _autoRefreshTimer.Start();
    }

    private void HideConnectionStatusMessageTimer_Tick(object sender, EventArgs e)
    {
        _hideConnectionStatusMessageTimer.Stop();
        IsConnectionStatusMessageDisplayed = false;
    }

    #endregion
}

public record WiFiChannelLegendEntry(SolidColorBrush Color, string Ssid, string Bssid);