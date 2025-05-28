using LiveCharts;
using LiveCharts.Wpf;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
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

    private readonly IDialogCoordinator _dialogCoordinator;

    private static readonly ILog Log = LogManager.GetLogger(typeof(WiFiViewModel));

    private readonly bool _isLoading;
    private readonly DispatcherTimer _autoRefreshTimer = new();
    private readonly DispatcherTimer _hideConnectionStatusMessageTimer = new();

    private bool _sdkContractAvailable;

    public bool SdkContractAvailable
    {
        get => _sdkContractAvailable;
        set
        {
            if (value == _sdkContractAvailable)
                return;

            _sdkContractAvailable = value;
            OnPropertyChanged();
        }
    }

    private bool _wiFiAdapterAccessEnabled;

    public bool WiFiAdapterAccessEnabled
    {
        get => _wiFiAdapterAccessEnabled;
        set
        {
            if (value == _wiFiAdapterAccessEnabled)
                return;

            _wiFiAdapterAccessEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _isAdaptersLoading;

    public bool IsAdaptersLoading
    {
        get => _isAdaptersLoading;
        set
        {
            if (value == _isAdaptersLoading)
                return;

            _isAdaptersLoading = value;
            OnPropertyChanged();
        }
    }

    private List<WiFiAdapterInfo> _adapters = [];

    public List<WiFiAdapterInfo> Adapters
    {
        get => _adapters;
        private set
        {
            if (value == _adapters)
                return;

            _adapters = value;
            OnPropertyChanged();
        }
    }

    private WiFiAdapterInfo _selectedAdapters;

    public WiFiAdapterInfo SelectedAdapter
    {
        get => _selectedAdapters;
        set
        {
            if (value == _selectedAdapters)
                return;

            if (value != null)
            {
                if (!_isLoading)
                    SettingsManager.Current.WiFi_InterfaceId = value.NetworkInterfaceInfo.Id;

                ScanAsync(value).ConfigureAwait(false);
            }

            _selectedAdapters = value;
            OnPropertyChanged();
        }
    }

    private bool _isNetworksLoading;

    public bool IsNetworksLoading
    {
        get => _isNetworksLoading;
        set
        {
            if (value == _isNetworksLoading)
                return;

            _isNetworksLoading = value;
            OnPropertyChanged();
        }
    }

    private bool _autoRefreshEnabled;

    public bool AutoRefreshEnabled
    {
        get => _autoRefreshEnabled;
        set
        {
            if (value == _autoRefreshEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_AutoRefreshEnabled = value;

            _autoRefreshEnabled = value;

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

    private AutoRefreshTimeInfo _selectedAutoRefreshTime;

    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get => _selectedAutoRefreshTime;
        set
        {
            if (value == _selectedAutoRefreshTime)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_AutoRefreshTime = value;

            _selectedAutoRefreshTime = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

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

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    private bool _show2dot4GHzNetworks;

    public bool Show2dot4GHzNetworks
    {
        get => _show2dot4GHzNetworks;
        set
        {
            if (value == _show2dot4GHzNetworks)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_Show2dot4GHzNetworks = value;

            _show2dot4GHzNetworks = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    private bool _show5GHzNetworks;

    public bool Show5GHzNetworks
    {
        get => _show5GHzNetworks;
        set
        {
            if (value == _show5GHzNetworks)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_Show5GHzNetworks = value;

            _show5GHzNetworks = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    private bool _show6GHzNetworks;

    public bool Show6GHzNetworks
    {
        get => _show6GHzNetworks;
        set
        {
            if (value == _show6GHzNetworks)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_Show6GHzNetworks = value;

            _show6GHzNetworks = value;

            NetworksView.Refresh();

            OnPropertyChanged();
        }
    }

    private ObservableCollection<WiFiNetworkInfo> _networks = new();

    public ObservableCollection<WiFiNetworkInfo> Networks
    {
        get => _networks;
        set
        {
            if (value != null && value == _networks)
                return;

            _networks = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView NetworksView { get; }

    private WiFiNetworkInfo _selectedNetwork;

    public WiFiNetworkInfo SelectedNetwork
    {
        get => _selectedNetwork;
        set
        {
            if (value == _selectedNetwork)
                return;

            _selectedNetwork = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedNetworks = new ArrayList();

    public IList SelectedNetworks
    {
        get => _selectedNetworks;
        set
        {
            if (Equals(value, _selectedNetworks))
                return;

            _selectedNetworks = value;
            OnPropertyChanged();
        }
    }

    public SeriesCollection Radio2dot4GHzSeries { get; set; } = [];

    public string[] Radio2dot4GHzLabels { get; set; } =
        [" ", " ", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", " ", " "];

    public SeriesCollection Radio5GHzSeries { get; set; } = [];

    public string[] Radio5GHzLabels { get; set; } =
    [
        " ", " ", "36", "40", "44", "48", "52", "56", "60", "64", "", "", "", "", "100", "104", "108", "112", "116",
        "120", "124", "128", "132", "136", "140", "144", "149", "153", "157", "161", "165", " ", " "
    ];

    public SeriesCollection Radio6GHzSeries { get; set; } = [];

    public string[] Radio6GHzLabels { get; set; } =
    [

    ];

    public Func<double, string> FormattedDbm { get; set; } =
        value => $"- {100 - value} dBm"; // Reverse y-axis 0 to -100

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

    private bool _isBackgroundSearchRunning;

    public bool IsBackgroundSearchRunning
    {
        get => _isBackgroundSearchRunning;
        set
        {
            if (value == _isBackgroundSearchRunning)
                return;

            _isBackgroundSearchRunning = value;
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

    private bool _isConnecting;

    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            if (value == _isConnecting)
                return;

            _isConnecting = value;
            OnPropertyChanged();
        }
    }

    private bool _isConnectionStatusMessageDisplayed;

    public bool IsConnectionStatusMessageDisplayed
    {
        get => _isConnectionStatusMessageDisplayed;
        set
        {
            if (value == _isConnectionStatusMessageDisplayed)
                return;

            _isConnectionStatusMessageDisplayed = value;
            OnPropertyChanged();
        }
    }

    private string _connectionStatusMessage;

    public string ConnectionStatusMessage
    {
        get => _connectionStatusMessage;
        private set
        {
            if (value == _connectionStatusMessage)
                return;

            _connectionStatusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public WiFiViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

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
        
            // Frequenzfilter immer anwenden
            if ((info.Radio == WiFiRadio.GHz2dot4 && !Show2dot4GHzNetworks) ||
                (info.Radio == WiFiRadio.GHz5 && !Show5GHzNetworks) ||
                (info.Radio == WiFiRadio.GHz6 && !Show6GHzNetworks))
            {
                return false;
            }
        
            // Wenn kein Suchbegriff, Frequenzfilter reicht
            if (string.IsNullOrEmpty(Search))
                return true;
        
            // Suchlogik
            return info.AvailableNetwork.Ssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.NetworkAuthenticationType.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{info.ChannelCenterFrequencyInGigahertz}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{info.Channel}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.AvailableNetwork.Bssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Vendor.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.PhyKind.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Load network adapters
        LoadAdaptersAsync(SettingsManager.Current.WiFi_InterfaceId).ConfigureAwait(false);

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
        Show2dot4GHzNetworks = SettingsManager.Current.WiFi_Show2dot4GHzNetworks;
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
        LoadAdaptersAsync(SelectedAdapter?.NetworkInterfaceInfo.Id).ConfigureAwait(false);
    }

    public ICommand ScanNetworksCommand =>
        new RelayCommand(_ => ScanNetworksAction().ConfigureAwait(false), ScanNetworks_CanExecute);

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
        Export().ConfigureAwait(false);
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
            Radio2dot4GHzSeries.Clear();
            Radio5GHzSeries.Clear();

            Log.Debug("ScanAsync - Adding new values...");
            foreach (var network in wiFiNetworkScanInfo.WiFiNetworkInfos)
            {
                Log.Debug("ScanAsync - Add network: " + network.AvailableNetwork.Ssid + " with channel frequency: " +
                          network.AvailableNetwork.ChannelCenterFrequencyInKilohertz);

                Networks.Add(network);

                switch (network.Radio)
                {
                    case WiFiRadio.GHz2dot4:
                        Radio2dot4GHzSeries.Add(GetSeriesCollection(network));
                        break;

                    case WiFiRadio.GHz5:
                        Radio5GHzSeries.Add(GetSeriesCollection(network));
                        break;

                        // ToDo: Implement 6 GHz
                        /*
                        case WiFiRadio.GHz6:
                            break;
                        */
                }
            }

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
            Radio2dot4GHzSeries.Clear();
            Radio5GHzSeries.Clear();
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

    private ChartValues<double> GetDefaultChartValues(WiFiRadio radio)
    {
        ChartValues<double> values = [];

        var size = radio switch
        {
            WiFiRadio.GHz2dot4 => Radio2dot4GHzLabels.Length,
            WiFiRadio.GHz5 => Radio5GHzLabels.Length,
            WiFiRadio.GHz6 => Radio6GHzLabels.Length,
            _ => 0
        };

        for (var i = 0; i < size; i++)
            values.Add(-1);

        return values;
    }

    private ChartValues<double> GetChartValues(WiFiNetworkInfo network, int index)
    {
        var values = GetDefaultChartValues(network.Radio);

        var reverseMilliwatts = 100 - network.AvailableNetwork.NetworkRssiInDecibelMilliwatts * -1;

        // ToDo: Implement channel width (20, 40, 80, 160)
        values[index - 2] = -1;
        values[index - 1] = reverseMilliwatts;
        values[index] = reverseMilliwatts;
        values[index + 1] = reverseMilliwatts;
        values[index + 2] = -1;

        return values;
    }

    private LineSeries GetSeriesCollection(WiFiNetworkInfo network)
    {
        var radioLabels = network.Radio switch
        {
            WiFiRadio.GHz2dot4 => Radio2dot4GHzLabels,
            WiFiRadio.GHz5 => Radio5GHzLabels,
            WiFiRadio.GHz6 => Radio6GHzLabels,
            _ => []
        };

        var index = Array.IndexOf(radioLabels, $"{network.Channel}");

        return new LineSeries
        {
            Title = $"{network.AvailableNetwork.Ssid} ({network.AvailableNetwork.Bssid})",
            Values = GetChartValues(network, index),
            PointGeometry = null,
            LineSmoothness = 0
        };
    }

    private async void Connect()
    {
        var selectedAdapter = SelectedAdapter;
        var selectedNetwork = SelectedNetwork;

        var connectMode = WiFi.GetConnectMode(selectedNetwork.AvailableNetwork);

        var customDialog = new CustomDialog
        {
            Title = selectedNetwork.IsHidden
                ? Strings.HiddenNetwork
                : string.Format(Strings.ConnectToXXX, selectedNetwork.AvailableNetwork.Ssid)
        };

        var connectViewModel = new WiFiConnectViewModel(async instance =>
            {
                // Connect Open/PSK/EAP
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

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
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

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
            _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, (selectedAdapter, selectedNetwork),
            connectMode);

        customDialog.Content = new WiFiConnectDialog
        {
            DataContext = connectViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
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

    private async Task Export()
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
                        instance.ExportAll
                            ? Networks
                            : new ObservableCollection<WiFiNetworkInfo>(SelectedNetworks.Cast<WiFiNetworkInfo>()
                                .ToArray()));
                }
                catch (Exception ex)
                {
                    Log.Error("Error while exporting data as " + instance.FileType, ex);

                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.WiFi_ExportFileType = instance.FileType;
                SettingsManager.Current.WiFi_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            [ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json], true,
            SettingsManager.Current.WiFi_ExportFileType, SettingsManager.Current.WiFi_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
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