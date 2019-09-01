using LiveCharts;
using LiveCharts.Wpf;
using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Windows.Devices.WiFi;

namespace NETworkManager.ViewModels
{
    public class WiFiViewModel : ViewModelBase
    {
        #region  Variables 
        private readonly bool _isLoading;

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

        private List<WiFiAdapterInfo> _adapters = new List<WiFiAdapterInfo>();
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
                    //if (!_isLoading)
                    //    SettingsManager.Current.NetworkInterface_SelectedInterfaceId = value.Id;

                    ScanNetworks(value.WiFiAdapter);
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
        private ObservableCollection<WiFiNetworkInfo> _networks = new ObservableCollection<WiFiNetworkInfo>();
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

        public SeriesCollection Radio1Series { get; set; } = new SeriesCollection();
        public string[] Radio1Labels { get; set; } = new string[] { " ", " ", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", " ", " " };
        public SeriesCollection Radio2Series { get; set; } = new SeriesCollection();
        public string[] Radio2Labels { get; set; }
        public Func<double, string> FormatterdBm { get; set; } = value => $"- {value} dBm";

        #endregion

        #region Constructor, load settings
        public WiFiViewModel()
        {
            _isLoading = true;

            // Result view
            NetworksView = CollectionViewSource.GetDefaultView(Networks);

            LoadSettings();

            LoadAdapters();

            _isLoading = false;
        }

        private ChartValues<double> GetDefaultValues()
        {
            ChartValues<double> values = new ChartValues<double>();

            for (int i = 0; i < 17; i++)
                values.Add(-1);

            return values;
        }

        private void AddNetworkToRadio1Chart(WiFiNetworkInfo network)
        {
            int channel = WiFi.GetChannelFromChannelFrequency(network.ChannelCenterFrequencyInKilohertz);

            int index = channel + 1;

            ChartValues<double> values = GetDefaultValues();
            
            values[index - 2] = 0;
            values[index - 1] = network.NetworkRssiInDecibelMilliwatts / 2 * -1;
            values[index] = network.NetworkRssiInDecibelMilliwatts * -1;
            values[index + 1] = network.NetworkRssiInDecibelMilliwatts / 2 * -1;
            values[index + 2] = 0;

            Radio1Series.Add(new LineSeries
            {
                Title = network.SSID,
                Values = values,
                PointGeometry = null
            });
        }

        private void AddNetworkToRadio2Chart(WiFiNetworkInfo network)
        {

        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadAdaptersCommand => new RelayCommand(p => ReloadAdapterAction(), ReloadAdapters_CanExecute);

        private bool ReloadAdapters_CanExecute(object obj) => !IsAdaptersLoading;

        private void ReloadAdapterAction()
        {
            ReloadAdapter();
        }

        public ICommand ScanNetworksCommand => new RelayCommand(p => ScanNetworksAction(), ScanNetworks_CanExecute);

        private bool ScanNetworks_CanExecute(object obj) => !IsNetworksLoading;


        private void ScanNetworksAction()
        {
            ScanNetworks(SelectedAdapter.WiFiAdapter);
        }

        #endregion

        #region Methods
        private async void LoadAdapters()
        {
            IsAdaptersLoading = true;

            Adapters = await WiFi.GetAdapterAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (Adapters.Count > 0)
            {
                var info = Adapters.FirstOrDefault(s => s.NetworkInterfaceInfo.Id.ToString() == SettingsManager.Current.NetworkInterface_SelectedInterfaceId);

                SelectedAdapter = info ?? Adapters[0];
            }

            IsAdaptersLoading = false;
        }

        private async void ReloadAdapter()
        {
            IsAdaptersLoading = true;

            await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the reload command)

            string id = string.Empty;

            if (SelectedAdapter != null)
                id = SelectedAdapter.NetworkInterfaceInfo.Id;

            Adapters = await WiFi.GetAdapterAsync();

            if (Adapters.Count > 0)
            {
                // Change interface...
                SelectedAdapter = string.IsNullOrEmpty(id) ? Adapters.FirstOrDefault() : Adapters.FirstOrDefault(x => x.NetworkInterfaceInfo.Id == id);
            }

            IsAdaptersLoading = false;
        }

        private async void ScanNetworks(WiFiAdapter adapter)
        {
            IsNetworksLoading = true;

            Networks.Clear();

            Radio1Series.Clear();
            Radio2Series.Clear();

            IEnumerable<WiFiNetworkInfo> networks = await WiFi.GetNetworksAsync(adapter);

            foreach (var network in networks)
            {
                Networks.Add(network);

                if (WiFi.ConvertChannelFrequencyToGigahertz(network.ChannelCenterFrequencyInKilohertz) < 5) // 2.4 GHz
                    AddNetworkToRadio1Chart(network);
                else
                    AddNetworkToRadio2Chart(network);

            }

            IsNetworksLoading = false;
        }

        public void OnViewVisible()
        {

        }

        public void OnViewHide()
        {

        }
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
        #endregion
    }
}

