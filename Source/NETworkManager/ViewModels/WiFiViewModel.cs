using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.WiFi;

namespace NETworkManager.ViewModels
{
    public class WiFiViewModel : ViewModelBase
    {
        #region  Variables 
        private readonly bool _isLoading;

        private bool _isWiFiAdaptersLoading;
        public bool IsWiFiAdaptersLoading
        {
            get => _isWiFiAdaptersLoading;
            set
            {
                if (value == _isWiFiAdaptersLoading)
                    return;

                _isWiFiAdaptersLoading = value;
                OnPropertyChanged();
            }
        }

        private List<WiFiAdapterInfo> _wiFiAdapters;
        public List<WiFiAdapterInfo> WiFiAdapters
        {
            get => _wiFiAdapters;
            set
            {
                if (value == _wiFiAdapters)
                    return;

                _wiFiAdapters = value;
                OnPropertyChanged();
            }
        }

        private WiFiAdapterInfo _selectedWiFiAdapters;
        public WiFiAdapterInfo SelectedWiFiAdapter
        {
            get => _selectedWiFiAdapters;
            set
            {
                if (value == _selectedWiFiAdapters)
                    return;

                if (value != null)
                {
                    //if (!_isLoading)
                    //    SettingsManager.Current.NetworkInterface_SelectedInterfaceId = value.Id;

                    ScanNetworks(value.WiFiAdapter);
                }

                _selectedWiFiAdapters = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings

        public WiFiViewModel()
        {
            _isLoading = true;

            LoadSettings();

            LoadAdapters();
                       
            _isLoading = false;
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadWiFiAdaptersCommand => new RelayCommand(p => ReloadAdapterAction(), ReloadWiFiAdapters_CanExecute);

        private bool ReloadWiFiAdapters_CanExecute(object obj) => !IsWiFiAdaptersLoading;

        private void ReloadAdapterAction()
        {
            ReloadAdapter();
        }
        #endregion

        #region Methods
        private async void LoadAdapters()
        {
            IsWiFiAdaptersLoading = true;

            WiFiAdapters = await WiFi.GetAdapterAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (WiFiAdapters.Count > 0)
            {
                var info = WiFiAdapters.FirstOrDefault(s => s.NetworkInterfaceInfo.Id.ToString() == SettingsManager.Current.NetworkInterface_SelectedInterfaceId);

                SelectedWiFiAdapter = info ?? WiFiAdapters[0];
            }

            IsWiFiAdaptersLoading = false;                        
        }

        private async void ReloadAdapter()
        {
            IsWiFiAdaptersLoading = true;

            await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the reload command)

            string id = string.Empty;

            if (SelectedWiFiAdapter != null)
                id = SelectedWiFiAdapter.NetworkInterfaceInfo.Id;

            WiFiAdapters = await WiFi.GetAdapterAsync();

            // Change interface...
            SelectedWiFiAdapter = string.IsNullOrEmpty(id) ? WiFiAdapters.FirstOrDefault() : WiFiAdapters.FirstOrDefault(x => x.NetworkInterfaceInfo.Id == id);

            IsWiFiAdaptersLoading = false;
        }
               
        private async void ScanNetworks(WiFiAdapter adapter)
        {
            var x = await WiFi.GetNetworksAsync(adapter);

            foreach(var i in x)
            {
                Debug.WriteLine(i.BSSID);
                Debug.WriteLine(i.SSID);
            }
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

