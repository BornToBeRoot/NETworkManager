using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Converters;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Utilities.WPF;
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
    /// <summary>
    /// Refreshes the view automatically (see <see cref="AutoRefreshEnabled"/>).
    /// </summary>
    private readonly AutoRefreshTimer _autoRefreshTimer;
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

                // The same adapter after the adapters were reloaded: refresh in the background (the networks stay
                // visible), another adapter: show the loading indicator instead of the networks of the previous one
                var isSameAdapter = field != null && field.NetworkInterfaceInfo.Id == value.NetworkInterfaceInfo.Id;

                _ = ScanAsync(value, isSameAdapter);
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets whether another adapter can be selected: not while the networks are scanned (the result of the previous
    ///     adapter could arrive after the one of the new adapter) or a connection is made.
    /// </summary>
    public bool CanChangeAdapter => !IsNetworksLoading && !IsBackgroundSearchRunning && !IsConnecting;

    public bool IsNetworksLoading
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanChangeAdapter));
        }
    }

    /// <summary>
    /// Gets the time of the last scan of the networks, null before the first one (shown in the header).
    /// </summary>
    public DateTime? LastRefreshTime
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
    /// Gets the auto refresh, for the countdown in the header (see <see cref="AutoRefreshTimer.RemainingTime"/>).
    /// </summary>
    public AutoRefreshTimer AutoRefreshTimer => _autoRefreshTimer;

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

            _autoRefreshTimer.IsEnabled = value;

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

            // Not in the list (e.g. a removed interval of an old settings file): the previous interval is kept
            if (value != null)
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Delays filtering while the user is typing (see <see cref="Search"/>), so a large list is not filtered on
    /// every key press.
    /// </summary>
    private readonly SearchDelay _searchDelay;

    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            // Filter after a short delay while typing (see _searchDelay)
            _searchDelay.Restart();

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

    /// <summary>
    ///     Networks of the last successful scan and the id of its adapter: the next scan of the same adapter is compared
    ///     with it (see <see cref="CompareWithPreviousScan" />). Only in memory.
    /// </summary>
    private IReadOnlyList<WiFiNetworkInfo> _previousScan;

    private string _previousScanAdapterId;

    /// <summary>
    ///     Gets whether the shown networks are compared with a previous scan (shows the column and the counters).
    /// </summary>
    public bool HasComparison
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
    ///     Gets the number of networks that are visible now and were not in the previous scan.
    /// </summary>
    public int NetworksNew
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
    ///     Gets the number of networks that were visible in the previous scan and are not now.
    /// </summary>
    public int NetworksGone
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
    ///     Gets the number of networks whose channel, channel width or security changed since the previous scan.
    /// </summary>
    public int NetworksChanged
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

    public ObservableRangeCollection<WiFiNetworkInfo> Networks
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
            OnPropertyChanged(nameof(CanChangeAdapter));
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
            OnPropertyChanged(nameof(CanChangeAdapter));
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
        _searchDelay = new SearchDelay(GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan, ApplySearch);

        _isLoading = true;

        // Set up the channel charts (axes, sections, paints), the list and the timers unconditionally, so the
        // bindings are never null - also if the WiFi API is not available or the access is denied (yet)
        InitializeCharts();

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

            // Search by SSID ("Hidden Network" as shown), authentication type, channel frequency, channel, BSSID,
            // vendor and PHY kind
            return GetDisplaySsid(info).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.NetworkAuthenticationType.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{info.ChannelCenterFrequencyInGigahertz}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{info.Channel}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Bssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Vendor.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.PhyKind.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   (HasComparison && WiFiNetworkChangeToStringConverter.ToLocalizedString(info.Change)
                       .IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1);
        };

        // Auto refresh (enabled when the access is allowed, see Start)
        // Not without an adapter, while a dialog is open (e.g. connect), or while networks are loading or a connection
        // is made (tried again after the interval)
        _autoRefreshTimer = new AutoRefreshTimer(() => ScanAsync(SelectedAdapter, true),
            () => SelectedAdapter != null && !IsDialogOpen() && !IsNetworksLoading && !IsBackgroundSearchRunning &&
                  !IsConnecting);

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.WiFi_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.WiFi_AutoRefreshTime.TimeUnit);

        // Hide ConnectionStatusMessage automatically
        _hideConnectionStatusMessageTimer.Interval = new TimeSpan(0, 0, 15);
        _hideConnectionStatusMessageTimer.Tick += HideConnectionStatusMessageTimer_Tick;

        // Load settings
        LoadSettings();

        // Check if Microsoft.Windows.SDK.Contracts is available
        SdkContractAvailable = ApiInformation.IsTypePresent("Windows.Devices.WiFi.WiFiAdapter");

        if (SdkContractAvailable)
        {
            // Check if the access is denied and show a message (checked again, see CheckAccess)
            WiFiAdapterAccessEnabled = RequestAccess();

            if (WiFiAdapterAccessEnabled)
                Start();
        }

        _isLoading = false;
    }

    /// <summary>
    ///     Loads the adapters (scans the networks of the selected one) and enables the auto refresh. Called once the
    ///     access to the WiFi adapter is allowed.
    /// </summary>
    private void Start()
    {
        _ = LoadAdaptersAsync(SettingsManager.Current.WiFi_InterfaceId);

        AutoRefreshEnabled = SettingsManager.Current.WiFi_AutoRefreshEnabled;
    }

    /// <summary>
    ///     Checks the access to the WiFi adapter again while it is denied (e.g. it was allowed in the Windows settings
    ///     after the message was shown), and starts if it is allowed now - without a restart of the application.
    /// </summary>
    private void CheckAccess()
    {
        if (!SdkContractAvailable || WiFiAdapterAccessEnabled)
            return;

        WiFiAdapterAccessEnabled = RequestAccess();

        if (WiFiAdapterAccessEnabled)
            Start();
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
        // Not blocked by the auto refresh (it only scans the networks): without an adapter its toggle is hidden
        return !IsDialogOpen() && !IsAdaptersLoading && !IsNetworksLoading && !IsBackgroundSearchRunning &&
               !IsConnecting;
    }

    /// <summary>
    ///     A dialog (e.g. connect, export) is open: no refresh behind it (F5), and no auto refresh.
    /// </summary>
    private static bool IsDialogOpen()
    {
        return Application.Current.MainWindow == null ||
               ((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen ||
               ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void ReloadAdapterAction()
    {
        _ = LoadAdaptersAsync(SelectedAdapter?.NetworkInterfaceInfo.Id);
    }

    public ICommand ScanNetworksCommand =>
        new RelayCommand(parameter => { _ = ScanNetworksAction(); }, ScanNetworks_CanExecute);

    private bool ScanNetworks_CanExecute(object obj)
    {
        return SelectedAdapter != null && !IsDialogOpen() && !IsAdaptersLoading && !IsNetworksLoading &&
               !IsBackgroundSearchRunning && !AutoRefreshEnabled && !IsConnecting;
    }

    private async Task ScanNetworksAction()
    {
        await ScanAsync(SelectedAdapter, true);
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction(), ConnectDisconnect_CanExecute);

    /// <summary>
    ///     A network is selected that is still visible (not gone since the last scan) and no connection is made at the
    ///     moment.
    /// </summary>
    private bool ConnectDisconnect_CanExecute(object obj)
    {
        return SelectedNetwork is { Change: not WiFiNetworkChange.Gone } && !IsConnecting;
    }

    private void ConnectAction()
    {
        Connect();
    }

    public ICommand DisconnectCommand => new RelayCommand(_ => DisconnectAction(), ConnectDisconnect_CanExecute);

    private void DisconnectAction()
    {
        _ = DisconnectAsync();
    }

    /// <summary>
    ///     Gets the command to copy a value of all selected networks (one per line, in the order shown in the grid,
    ///     without duplicates). Parameter: <c>SSID</c> or <c>BSSID</c>.
    /// </summary>
    public ICommand CopySelectedNetworksCommand => new RelayCommand(CopySelectedNetworksAction);

    private void CopySelectedNetworksAction(object parameter)
    {
        Func<WiFiNetworkInfo, string> selector = parameter switch
        {
            // Hidden networks have no SSID
            "SSID" => x => x.AvailableNetwork.Ssid,
            "BSSID" => x => x.Bssid,
            _ => null
        };

        if (selector == null)
            return;

        ClipboardHelper.SetClipboard(string.Join(Environment.NewLine,
            GetSelectedNetworksInGridOrder().Select(selector).Where(x => !string.IsNullOrEmpty(x)).Distinct()));
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    public ICommand CheckAccessCommand => new RelayCommand(_ => CheckAccess());

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
        try
        {
            var accessStatus = WiFiAdapter.RequestAccessAsync().GetAwaiter().GetResult();

            return accessStatus == WiFiAccessStatus.Allowed;
        }
        catch (Exception ex)
        {
            // RequestAccessAsync() can throw instead of returning a status (e.g. on Windows ARM64
            // running the emulated x64 build, see GitHub issue #3110). Log it and treat it as denied
            // so the view shows the "access not available" message with the settings button instead
            // of silently failing (the exception would otherwise be swallowed by the WPF binding that
            // constructs this view model).
            Log.Error("Error while requesting access to the WiFi adapter.", ex);

            return false;
        }
    }

    private async Task LoadAdaptersAsync(string adapterId = null)
    {
        Log.Debug("LoadAdaptersAsync - Trying to get WiFi adapters...");

        IsAdaptersLoading = true;

        try
        {
            // Takes at least the refresh interval, so the reload animation is visible and the reload can't be spammed -
            // loading runs at the same time
            Adapters = await TaskHelper.WithMinimumDurationAsync(() => WiFi.GetAdapterAsync(),
                GlobalStaticConfiguration.ApplicationUIRefreshInterval);
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

        // An error of the last scan is hidden (the search icon spins while searching)
        IsStatusMessageDisplayed = false;

        if (refreshing)
            IsBackgroundSearchRunning = true;
        else
            IsNetworksLoading = true;

        if (delayInMs != 0)
            await Task.Delay(delayInMs);

        var statusMessage = string.Empty;

        try
        {
            var wiFiNetworkScanInfo = await WiFi.GetNetworksAsync(adapterInfo.WiFiAdapter);

            Log.Debug("ScanAsync - Scan completed. Found " + wiFiNetworkScanInfo.WiFiNetworkInfos.Count + " networks.");

            // Remember the selected network, because replacing the networks clears the selection
            var selectedBssid = SelectedNetwork?.Bssid;

            // Replace all networks with a single notification, so the view is filtered / sorted only once. With the
            // networks that are gone since the previous scan of this adapter.
            Networks.ReplaceAll(CompareWithPreviousScan(adapterInfo, wiFiNetworkScanInfo.WiFiNetworkInfos));

            // Select the network again (e.g. after an auto refresh), so the selection and the scroll position are kept.
            // If several networks were selected, only this one is selected again. Not if it is hidden by the filter.
            if (selectedBssid != null)
            {
                var network = Networks.FirstOrDefault(x => x.Bssid == selectedBssid);

                if (network != null && NetworksView.Contains(network))
                    SelectedNetwork = network;
            }

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
                            network.ChannelBandwidthCenterFrequencyInGigahertz * 1000, WiFiRadio.GHz6);

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

            LastRefreshTime = wiFiNetworkScanInfo.Timestamp;

            // Connected / disconnected (also outside of NETworkManager) - the icon of the adapter selection
            WiFi.UpdateAdapterStates(Adapters);
        }
        catch (Exception ex)
        {
            Log.Error($"Error while scanning WiFi adapter \"{adapterInfo.NetworkInterfaceInfo.Name}\".", ex);

            statusMessage = string.Format(Strings.ErrorWhileScanningWiFiAdapterXXXWithErrorXXX,
                adapterInfo.NetworkInterfaceInfo.Name, ex.Message);

            // A refresh (manual, automatic, after connecting) keeps the networks of the last scan (e.g. the adapter is
            // busy shortly after connecting); the header still shows the time of the last scan. The networks of
            // another adapter (first scan) are cleared.
            if (!refreshing)
            {
                // No time of the last scan and no changes of the previous adapter in the header
                LastRefreshTime = null;
                HasComparison = false;
                NetworksNew = 0;
                NetworksGone = 0;
                NetworksChanged = 0;

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
        }
        finally
        {
            // Only an error, the time of the scan is shown in the header
            StatusMessage = statusMessage;
            IsStatusMessageDisplayed = !string.IsNullOrEmpty(statusMessage);

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

    // 5 GHz operating channels are 36 - 64 and 100 - 144 (every 4 in channel-number space) and 149 - 177 (UNII-3 / -4,
    // odd channel numbers), labeled with the channel
    private static readonly HashSet<int> ValidChannels5GHz =
    [
        .. BuildChannelSet(36, 64, 4),
        .. BuildChannelSet(100, 144, 4),
        .. BuildChannelSet(149, 177, 4)
    ];

    // 6 GHz operating channels are 1, 5, 9, ... 233 (every 4 in channel-number space), labeled with the channel.
    // Channel 2 (5935 MHz) is below channel 1, at -3 on the axis - not labeled (the tooltip shows it).
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
        Radio5GHzXAxes = BuildXAxes(30, 160, ValidChannels5GHz, true, labelColor);
        Radio6GHzLowerXAxes = BuildXAxes(-6, 128, ValidChannels6GHz, false, labelColor);
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
    ///     trapezoid centered on the center frequency of the whole channel (not of the primary channel).
    /// </summary>
    private static (LineSeries<WiFiChannelPoint> Series, WiFiChannelLegendEntry Legend) BuildNetworkSeries(WiFiNetworkInfo network, int colorIndex)
    {
        var color = ChartPalette[colorIndex % ChartPalette.Length];

        double dbm = network.AvailableNetwork.NetworkRssiInDecibelMilliwatts;
        var center = ChannelNumberToDisplay(
            FrequencyToChannelAxis(network.ChannelBandwidthCenterFrequencyInGigahertz * 1000, network.Radio),
            network.Radio);

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

        var ssid = GetDisplaySsid(network);

        var series = new LineSeries<WiFiChannelPoint>
        {
            Name = $"{ssid} ({network.Bssid})",
            Values = points,
            // Plot in 0..100 space (dBm + 100) so the area fill drops to the bottom baseline.
            Mapping = (point, _) => new Coordinate(point.ChannelAxis, point.Dbm + SignalOffset),
            GeometrySize = 0,
            LineSmoothness = 0,
            Stroke = new SolidColorPaint(color) { StrokeThickness = 1.5f },
            Fill = new SolidColorPaint(color.WithAlpha(0x33))
        };

        var wpfColor = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        var legend = new WiFiChannelLegendEntry(new SolidColorBrush(wpfColor), ssid, network.Bssid);

        return (series, legend);
    }

    /// <summary>
    ///     Compares the networks of a scan with the previous scan of the same adapter: marks new and changed networks,
    ///     adds the networks that are gone (shown until the next scan). The scan becomes the one the next scan is
    ///     compared with. Another adapter (or the first scan) starts without a comparison.
    /// </summary>
    /// <param name="adapterInfo">Adapter of the scan.</param>
    /// <param name="networks">Networks of the scan.</param>
    /// <returns>The networks to show (the scanned ones and the gone ones).</returns>
    private List<WiFiNetworkInfo> CompareWithPreviousScan(WiFiAdapterInfo adapterInfo,
        IReadOnlyList<WiFiNetworkInfo> networks)
    {
        var rows = new List<WiFiNetworkInfo>(networks);
        var adapterId = adapterInfo.NetworkInterfaceInfo.Id;

        if (_previousScan != null && _previousScanAdapterId == adapterId)
        {
            // The networks of the previous scan are not shown anymore (except the gone ones): no chain of previous
            // networks over the scans
            foreach (var network in _previousScan)
                network.Previous = null;

            var comparison = WiFiScanComparer.Compare(_previousScan, networks);

            foreach (var (network, change, previous) in comparison.Changes)
            {
                network.Change = change;
                network.Previous = previous;
            }

            // Shown as they were in the previous scan (not connected, not drawn in the charts)
            foreach (var network in comparison.Gone)
            {
                network.Change = WiFiNetworkChange.Gone;
                network.IsConnected = false;

                rows.Add(network);
            }

            NetworksNew = comparison.Changes.Count(x => x.Change == WiFiNetworkChange.New);
            NetworksGone = comparison.Gone.Count;
            NetworksChanged = comparison.Changes.Count(x => x.Change == WiFiNetworkChange.Changed);
            HasComparison = true;
        }
        else
        {
            NetworksNew = 0;
            NetworksGone = 0;
            NetworksChanged = 0;
            HasComparison = false;
        }

        _previousScan = networks;
        _previousScanAdapterId = adapterId;

        return rows;
    }

    /// <summary>
    ///     Gets the SSID of a network as shown in the grid ("Hidden Network" for a hidden network).
    /// </summary>
    private static string GetDisplaySsid(WiFiNetworkInfo network)
    {
        return network.IsHidden ? Strings.HiddenNetwork : network.AvailableNetwork.Ssid;
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
            // Channel 2 is at 5935 MHz, below channel 1 (5955 MHz) - drawn at its real position (-3)
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

            await ConnectAsync(ssid, () =>
            {
                PasswordCredential credential = new();

                switch (instance.ConnectMode)
                {
                    case WiFiConnectMode.Psk:
                        credential.Password = SecureStringHelper.ConvertToString(instance.PreSharedKey);
                        break;
                    // Without credentials, Windows uses the logged-on user or a certificate (like an open network,
                    // an empty credential is passed)
                    case WiFiConnectMode.Eap when instance.UseCredentials:
                        credential.UserName = instance.Username;

                        if (!string.IsNullOrEmpty(instance.Domain))
                            credential.Resource = instance.Domain;

                        credential.Password = SecureStringHelper.ConvertToString(instance.Password);
                        break;
                }

                return selectedNetwork.IsHidden
                    ? WiFi.ConnectAsync(instance.Options.AdapterInfo.WiFiAdapter,
                        instance.Options.NetworkInfo.AvailableNetwork, GetReconnectionKind(instance), credential,
                        instance.Ssid)
                    : WiFi.ConnectAsync(instance.Options.AdapterInfo.WiFiAdapter,
                        instance.Options.NetworkInfo.AvailableNetwork, GetReconnectionKind(instance), credential);
            });
        }, async instance =>
        {
            // Connect WPS
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            var ssid = selectedNetwork.IsHidden ? instance.Ssid : selectedNetwork.AvailableNetwork.Ssid;

            await ConnectAsync(ssid, () => WiFi.ConnectWpsAsync(instance.Options.AdapterInfo.WiFiAdapter,
                instance.Options.NetworkInfo.AvailableNetwork, GetReconnectionKind(instance)));
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

    /// <summary>
    ///     Gets the reconnection kind of the connect dialog ("Connect automatically").
    /// </summary>
    private static WiFiReconnectionKind GetReconnectionKind(WiFiConnectViewModel instance)
    {
        return instance.ConnectAutomatically ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual;
    }

    /// <summary>
    ///     Connects to a network and shows the status (connecting, result) below the networks. The networks are
    ///     scanned again afterwards.
    /// </summary>
    /// <param name="ssid">SSID of the network, shown in the status message.</param>
    /// <param name="connect">Connects to the network (Open / PSK / EAP or WPS) and returns the result.</param>
    private async Task ConnectAsync(string ssid, Func<Task<WiFiConnectionStatus>> connect)
    {
        // Show status message (not hidden by the timer of a previous message)
        _hideConnectionStatusMessageTimer.Stop();
        IsConnecting = true;
        ConnectionStatusMessage = string.Format(Strings.ConnectingToXXX, ssid);
        IsConnectionStatusMessageDisplayed = true;

        try
        {
            // Connect to the network
            var connectionResult = await connect();

            // Get result
            ConnectionStatusMessage = connectionResult == WiFiConnectionStatus.Success
                ? string.Format(Strings.SuccessfullyConnectedToXXX, ssid)
                : string.Format(Strings.CouldNotConnectToXXXReasonXXX, ssid,
                    ResourceTranslator.Translate(ResourceIdentifier.WiFiConnectionStatus, connectionResult));
        }
        catch (Exception ex)
        {
            // E.g. the adapter was disabled while connecting
            Log.Error($"Error while connecting to the WiFi network \"{ssid}\".", ex);

            ConnectionStatusMessage = string.Format(Strings.CouldNotConnectToXXXReasonXXX, ssid, ex.Message);
        }
        finally
        {
            // Done connecting
            IsConnecting = false;
        }

        // Hide message automatically
        _hideConnectionStatusMessageTimer.Stop();
        _hideConnectionStatusMessageTimer.Start();

        // Update the Wi-Fi networks.
        // Wait because an error may occur if a refresh is done directly after connecting.
        await ScanAsync(SelectedAdapter, true, 5000);
    }

    private async Task DisconnectAsync()
    {
        var connectedNetwork = Networks.FirstOrDefault(x => x.IsConnected);

        try
        {
            WiFi.Disconnect(SelectedAdapter.WiFiAdapter);

            if (connectedNetwork != null)
            {
                ConnectionStatusMessage = string.Format(Strings.XXXDisconnected, GetDisplaySsid(connectedNetwork));
                IsConnectionStatusMessageDisplayed = true;
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error while disconnecting the WiFi adapter.", ex);

            ConnectionStatusMessage = string.Format(Strings.CouldNotDisconnectFromXXXReasonXXX,
                connectedNetwork == null ? null : GetDisplaySsid(connectedNetwork), ex.Message);
            IsConnectionStatusMessageDisplayed = true;
        }

        // Hide message automatically
        _hideConnectionStatusMessageTimer.Stop();
        _hideConnectionStatusMessageTimer.Start();

        // Refresh
        await ScanAsync(SelectedAdapter, true, GlobalStaticConfiguration.ApplicationUIRefreshInterval);
    }

    /// <summary>
    ///     Gets the selected networks in the order they are shown in the grid (not in the order they were selected).
    /// </summary>
    private List<WiFiNetworkInfo> GetSelectedNetworksInGridOrder()
    {
        var selectedNetworks = new HashSet<WiFiNetworkInfo>(SelectedNetworks.Cast<WiFiNetworkInfo>());

        return NetworksView.Cast<WiFiNetworkInfo>().Where(selectedNetworks.Contains).ToList();
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
                // The networks shown in the grid (band filter, search) in their order. Copied on the UI thread, exported
                // in the background
                var networks = instance.ExportAll
                    ? NetworksView.Cast<WiFiNetworkInfo>().ToList()
                    : GetSelectedNetworksInGridOrder();

                await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, networks));
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
        // The access may have been allowed in the Windows settings in the meantime
        CheckAccess();

        _autoRefreshTimer.Resume();
    }

    public void OnViewHide()
    {
        _autoRefreshTimer.Pause();
    }

    #endregion

    #region Events

    private void HideConnectionStatusMessageTimer_Tick(object sender, EventArgs e)
    {
        _hideConnectionStatusMessageTimer.Stop();
        IsConnectionStatusMessageDisplayed = false;
    }

    /// <summary>
    /// Gets the command to filter right away (Enter in the search box, see
    /// <see cref="NETworkManager.Utilities.WPF.SearchTextBoxHelper"/>).
    /// </summary>
    public ICommand ApplySearchCommand => _searchDelay.ApplyCommand;

    /// <summary>
    /// Filters with the current <see cref="Search"/> right away.
    /// </summary>
    private void ApplySearch()
    {
        NetworksView.Refresh();
    }

    #endregion
}

public record WiFiChannelLegendEntry(SolidColorBrush Color, string Ssid, string Bssid);