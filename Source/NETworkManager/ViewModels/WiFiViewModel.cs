using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public ICommand ScanNetworksCommand => new RelayCommand(p => ScanNetworksAction());

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

            try
            {
                IEnumerable<WiFiNetworkInfo> networks = await WiFi.GetNetworksAsync(adapter);

                Networks.Clear();

                foreach (var network in networks)
                    Networks.Add(network);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error...", ex.Message);
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

