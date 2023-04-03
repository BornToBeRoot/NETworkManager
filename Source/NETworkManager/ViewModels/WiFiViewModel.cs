﻿using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Windows.Devices.WiFi;

namespace NETworkManager.ViewModels;

public class WiFiViewModel : ViewModelBase
{
    #region  Variables 
    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly bool _isLoading;    
    private readonly DispatcherTimer _autoRefreshTimer = new();

    private bool _sdkContractsFailedToLoad;
    public bool SDKContractsFailedToLoad
    {
        get => _sdkContractsFailedToLoad;
        set
        {
            if (value == _sdkContractsFailedToLoad)
                return;

            _sdkContractsFailedToLoad = value;
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
        set
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

                Scan(value.WiFiAdapter);
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

    public SeriesCollection Radio1Series { get; set; } = new SeriesCollection();
    public string[] Radio1Labels { get; set; } = new string[] { " ", " ", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", " ", " " };
    public SeriesCollection Radio2Series { get; set; } = new SeriesCollection();
    public string[] Radio2Labels { get; set; } = new string[] { " ", " ", "36", "40", "44", "48", "52", "56", "60", "64", "", "", "", "", "100", "104", "108", "112", "116", "120", "124", "128", "132", "136", "140", "144", "149", "153", "157", "161", "165", " ", " " };
    public Func<double, string> FormatterdBm { get; set; } = value => $"- {100 - value} dBm"; // Reverse y-axis 0 to -100

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
        set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Constructor, load settings
    public WiFiViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        // Result view + search
        NetworksView = CollectionViewSource.GetDefaultView(Networks);
        NetworksView.SortDescriptions.Add(new SortDescription($"{nameof(WiFiNetworkInfo.AvailableNetwork)}.{nameof(WiFiNetworkInfo.AvailableNetwork.Ssid)}", ListSortDirection.Ascending));
        NetworksView.Filter = o =>
        {
            if (o is WiFiNetworkInfo info)
            {
                if (WiFi.Is2dot4GHzNetwork(info.AvailableNetwork.ChannelCenterFrequencyInKilohertz) && !Show2dot4GHzNetworks)
                    return false;

                if (WiFi.Is5GHzNetwork(info.AvailableNetwork.ChannelCenterFrequencyInKilohertz) && !Show5GHzNetworks)
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return true;


                // Search by: SSID, Security, Channel, BSSID (MAC address), Vendor, Phy kind
                return info.AvailableNetwork.Ssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                WiFi.GetHumanReadableNetworkAuthenticationType(info.AvailableNetwork.SecuritySettings.NetworkAuthenticationType).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                $"{WiFi.GetChannelFromChannelFrequency(info.AvailableNetwork.ChannelCenterFrequencyInKilohertz)}".IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                info.AvailableNetwork.Bssid.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                OUILookup.Lookup(info.AvailableNetwork.Bssid).FirstOrDefault()?.Vendor.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                WiFi.GetHumandReadablePhyKind(info.AvailableNetwork.PhyKind).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
            }
            else
            {
                return false;
            }
        };

        // Load network adapters
        LoadAdapters(SettingsManager.Current.WiFi_InterfaceId);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x => x.Value == SettingsManager.Current.WiFi_AutoRefreshTime.Value && x.TimeUnit == SettingsManager.Current.WiFi_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.WiFi_AutoRefreshEnabled;

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
    public ICommand ReloadAdaptersCommand => new RelayCommand(p => ReloadAdapterAction(), ReloadAdapters_CanExecute);

    private bool ReloadAdapters_CanExecute(object obj) => !IsAdaptersLoading;

    private void ReloadAdapterAction()
    {
        LoadAdapters(SelectedAdapter?.NetworkInterfaceInfo.Id);
    }

    public ICommand ScanNetworksCommand => new RelayCommand(p => ScanNetworksAction(), ScanNetworks_CanExecute);

    private bool ScanNetworks_CanExecute(object obj) => !IsAdaptersLoading && !IsNetworksLoading;

    private async Task ScanNetworksAction()
    {
        await Scan(SelectedAdapter.WiFiAdapter, true);
    }

    public ICommand ExportCommand => new RelayCommand(p => ExportAction());

    private void ExportAction()
    {
        Export();
    }
    #endregion

    #region Methods
    private async Task LoadAdapters(string adapterId = null)
    {
        IsAdaptersLoading = true;

        try
        {            
            Adapters = await WiFi.GetAdapterAsync();

            // Check if we found any adapters
            if (Adapters.Count > 0)
            {
                // Check for existing adapter id or select the first one
                if (string.IsNullOrEmpty(adapterId))
                    SelectedAdapter = Adapters.FirstOrDefault();
                else
                    SelectedAdapter = Adapters.FirstOrDefault(s => s.NetworkInterfaceInfo.Id.ToString() == adapterId) ?? Adapters.FirstOrDefault();
            }
        }
        catch (FileNotFoundException) // This exception is thrown, when the Microsoft.Windows.SDK.Contracts is not available...
        {
            SDKContractsFailedToLoad = true;
        }

        IsAdaptersLoading = false;
    }

    private async Task Scan(WiFiAdapter adapter, bool refreshing = false)
    {
        if (refreshing)
        {
            StatusMessage = Localization.Resources.Strings.SearchingForNetworksDots;
            IsBackgroundSearchRunning = true;
        }
        else
        {
            IsStatusMessageDisplayed = false;
            IsNetworksLoading = true;
        }

        IEnumerable<WiFiNetworkInfo> networks = await WiFi.GetNetworksAsync(adapter);

        Networks.Clear();

        Radio1Series.Clear();
        Radio2Series.Clear();

        foreach (var network in networks)
        {
            // Identify hidden networks
            //if (string.IsNullOrEmpty(network.AvailableNetwork.Ssid))
            //    network.AvailableNetwork.Ssid = Localization.Resources.Strings.HiddenNetwork;

            Networks.Add(network);

            if (WiFi.ConvertChannelFrequencyToGigahertz(network.AvailableNetwork.ChannelCenterFrequencyInKilohertz) < 5) // 2.4 GHz
                AddNetworkToRadio1Chart(network);
            else
                AddNetworkToRadio2Chart(network);
        }

        IsStatusMessageDisplayed = true;
        StatusMessage = string.Format(Localization.Resources.Strings.LastScanAtX, DateTime.Now.ToLongTimeString());

        if (refreshing)
            IsBackgroundSearchRunning = false;
        else
            IsNetworksLoading = false;
    }

    private ChartValues<double> GetDefaultChartValues(WiFiRadio radio)
    {
        ChartValues<double> values = new();

        for (int i = 0; i < (radio == WiFiRadio.One ? Radio1Labels.Length : Radio2Labels.Length); i++)
            values.Add(-1);

        return values;
    }

    private ChartValues<double> SetChartValues(WiFiNetworkInfo network, WiFiRadio radio, int index)
    {
        ChartValues<double> values = GetDefaultChartValues(radio);

        double reverseMilliwatts = 100 - (network.AvailableNetwork.NetworkRssiInDecibelMilliwatts * -1);

        values[index - 2] = -1;
        values[index - 1] = reverseMilliwatts;
        values[index] = reverseMilliwatts;
        values[index + 1] = reverseMilliwatts;
        values[index + 2] = -1;

        return values;
    }

    private void AddNetworkToRadio1Chart(WiFiNetworkInfo network)
    {
        int index = Array.IndexOf(Radio1Labels, $"{WiFi.GetChannelFromChannelFrequency(network.AvailableNetwork.ChannelCenterFrequencyInKilohertz)}");

        Radio1Series.Add(new LineSeries
        {
            Title = network.AvailableNetwork.Ssid,
            Values = SetChartValues(network, WiFiRadio.One, index),
            PointGeometry = null,
            LineSmoothness = 0
        });
    }

    private void AddNetworkToRadio2Chart(WiFiNetworkInfo network)
    {
        int index = Array.IndexOf(Radio2Labels, $"{WiFi.GetChannelFromChannelFrequency(network.AvailableNetwork.ChannelCenterFrequencyInKilohertz)}");

        Radio2Series.Add(new LineSeries
        {
            Title = network.AvailableNetwork.Ssid,
            Values = SetChartValues(network, WiFiRadio.Two, index),
            PointGeometry = null,
            LineSmoothness = 0
        });
    }

    private async Task Export()
    {
        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? Networks : new ObservableCollection<WiFiNetworkInfo>(SelectedNetworks.Cast<WiFiNetworkInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.WiFi_ExportFileType = instance.FileType;
            SettingsManager.Current.WiFi_ExportFilePath = instance.FilePath;
        }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportFileType[] { ExportFileType.CSV, ExportFileType.XML, ExportFileType.JSON }, true, SettingsManager.Current.WiFi_ExportFileType, SettingsManager.Current.WiFi_ExportFilePath);

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
        // Stop timer...
        _autoRefreshTimer.Stop();

        // Scan networks
        await Scan(SelectedAdapter.WiFiAdapter, true);

        // Restart timer...
        _autoRefreshTimer.Start();
    }
    #endregion
}
