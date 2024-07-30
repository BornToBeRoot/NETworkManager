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
using LiveCharts;
using LiveCharts.Wpf;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

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

    private List<WiFiAdapterInfo> _adapters = new();

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

                Scan(value).ConfigureAwait(false);
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

    public SeriesCollection Radio1Series { get; set; } = new();

    public string[] Radio1Labels { get; set; } =
        { " ", " ", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", " ", " " };

    public SeriesCollection Radio2Series { get; set; } = new();

    public string[] Radio2Labels { get; set; } =
    {
        " ", " ", "36", "40", "44", "48", "52", "56", "60", "64", "", "", "", "", "100", "104", "108", "112", "116",
        "120", "124", "128", "132", "136", "140", "144", "149", "153", "157", "161", "165", " ", " "
    };

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

            if (WiFi.Is2dot4GHzNetwork(info.AvailableNetwork.ChannelCenterFrequencyInKilohertz) &&
                !Show2dot4GHzNetworks)
                return false;

            if (WiFi.Is5GHzNetwork(info.AvailableNetwork.ChannelCenterFrequencyInKilohertz) && !Show5GHzNetworks)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            // Search by: SSID, Security, Channel, BSSID (MAC address), Vendor, Phy kind
            return info.AvailableNetwork.Ssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   WiFi.GetHumanReadableNetworkAuthenticationType(info.AvailableNetwork.SecuritySettings
                       .NetworkAuthenticationType).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   $"{WiFi.GetChannelFromChannelFrequency(info.AvailableNetwork.ChannelCenterFrequencyInKilohertz)}"
                       .IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.AvailableNetwork.Bssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   OUILookup.LookupByMacAddress(info.AvailableNetwork.Bssid).FirstOrDefault()?.Vendor
                       .IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   WiFi.GetHumanReadablePhyKind(info.AvailableNetwork.PhyKind)
                       .IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Load network adapters
        LoadAdapters(SettingsManager.Current.WiFi_InterfaceId).ConfigureAwait(false);

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
    }

    #endregion

    #region ICommands & Actions

    public ICommand ReloadAdaptersCommand => new RelayCommand(_ => ReloadAdapterAction());

    private void ReloadAdapterAction()
    {
        LoadAdapters(SelectedAdapter?.NetworkInterfaceInfo.Id).ConfigureAwait(false);
    }

    public ICommand ScanNetworksCommand =>
        new RelayCommand(_ => ScanNetworksAction().ConfigureAwait(false), ScanNetworks_CanExecute);

    private bool ScanNetworks_CanExecute(object obj)
    {
        return !IsAdaptersLoading && !IsNetworksLoading && !IsBackgroundSearchRunning && !IsConnecting;
    }

    private async Task ScanNetworksAction()
    {
        await Scan(SelectedAdapter, true);
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
    ///     Request access to the WiFi adapter.
    /// </summary>
    /// <returns>Fails if the access is denied.</returns>
    private static bool RequestAccess()
    {
        var accessStatus = WiFiAdapter.RequestAccessAsync().GetAwaiter().GetResult();

        return accessStatus == WiFiAccessStatus.Allowed;
    }

    private async Task LoadAdapters(string adapterId = null)
    {
        IsAdaptersLoading = true;

        // Show a loading animation for the user
        await Task.Delay(2500);

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
            // Check for existing adapter id or select the first one
            if (string.IsNullOrEmpty(adapterId))
                SelectedAdapter = Adapters.FirstOrDefault();
            else
                SelectedAdapter = Adapters.FirstOrDefault(s => s.NetworkInterfaceInfo.Id.ToString() == adapterId) ??
                                  Adapters.FirstOrDefault();
        }

        IsAdaptersLoading = false;
    }

    private async Task Scan(WiFiAdapterInfo adapterInfo, bool refreshing = false, uint delayInMs = 0)
    {
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
            await Task.Delay((int)delayInMs);

        var statusMessage = string.Empty;

        try
        {
            var wiFiNetworkScanInfo = await WiFi.GetNetworksAsync(adapterInfo.WiFiAdapter);

            // Clear the values after the scan to make the UI smoother
            Networks.Clear();
            Radio1Series.Clear();
            Radio2Series.Clear();

            foreach (var network in wiFiNetworkScanInfo.WiFiNetworkInfos)
            {
                Networks.Add(network);

                if (WiFi.ConvertChannelFrequencyToGigahertz(network.AvailableNetwork
                        .ChannelCenterFrequencyInKilohertz) < 5) // 2.4 GHz
                    Radio1Series.Add(GetSeriesCollection(network, WiFiRadio.One));
                else
                    Radio2Series.Add(GetSeriesCollection(network, WiFiRadio.Two));
            }

            statusMessage = string.Format(Strings.LastScanAtX,
                wiFiNetworkScanInfo.Timestamp.ToLongTimeString());
        }
        catch (Exception ex)
        {
            // Clear the existing old values if an error occurs
            Networks.Clear();
            Radio1Series.Clear();
            Radio2Series.Clear();

            statusMessage = string.Format(Strings.ErrorWhileScanningWiFiAdapterXXXWithErrorXXX,
                adapterInfo.NetworkInterfaceInfo.Name, ex.Message);
        }
        finally
        {
            IsStatusMessageDisplayed = true;
            StatusMessage = statusMessage;

            IsBackgroundSearchRunning = false;
            IsNetworksLoading = false;
        }
    }

    private ChartValues<double> GetDefaultChartValues(WiFiRadio radio)
    {
        ChartValues<double> values = new();

        for (var i = 0; i < (radio == WiFiRadio.One ? Radio1Labels.Length : Radio2Labels.Length); i++)
            values.Add(-1);

        return values;
    }

    private ChartValues<double> GetChartValues(WiFiNetworkInfo network, WiFiRadio radio, int index)
    {
        var values = GetDefaultChartValues(radio);

        var reverseMilliwatts = 100 - network.AvailableNetwork.NetworkRssiInDecibelMilliwatts * -1;

        values[index - 2] = -1;
        values[index - 1] = reverseMilliwatts;
        values[index] = reverseMilliwatts;
        values[index + 1] = reverseMilliwatts;
        values[index + 2] = -1;

        return values;
    }

    private LineSeries GetSeriesCollection(WiFiNetworkInfo network, WiFiRadio radio)
    {
        var index = Array.IndexOf(radio == WiFiRadio.One ? Radio1Labels : Radio2Labels,
            $"{WiFi.GetChannelFromChannelFrequency(network.AvailableNetwork.ChannelCenterFrequencyInKilohertz)}");

        return new LineSeries
        {
            Title = $"{network.AvailableNetwork.Ssid} ({network.AvailableNetwork.Bssid})",
            Values = GetChartValues(network, radio, index),
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

        var exportViewModel = new WiFiConnectViewModel(async instance =>
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

                // Update the wifi networks.
                // Wait because an error may occur if a refresh is done directly after connecting.            
                await Scan(SelectedAdapter, true, 5000);
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

                // Update the wifi networks.
                // Wait because an error may occur if a refresh is done directly after connecting.            
                await Scan(SelectedAdapter, true, 5000);
            },
            _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, (selectedAdapter, selectedNetwork),
            connectMode);

        customDialog.Content = new WiFiConnectDialog
        {
            DataContext = exportViewModel
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
        await Scan(SelectedAdapter, true, 2500);
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
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.WiFi_ExportFileType = instance.FileType;
                SettingsManager.Current.WiFi_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            new[] { ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json }, true,
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
        await Scan(SelectedAdapter, true);

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