using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

            WiFiAdapters = await WiFi.GetWiFiAdapterAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (WiFiAdapters.Count > 0)
            {
                var info = WiFiAdapters.FirstOrDefault(s => s.Id.ToString() == SettingsManager.Current.NetworkInterface_SelectedInterfaceId);

                SelectedWiFiAdapter = info ?? WiFiAdapters[0];
            }

            IsWiFiAdaptersLoading = false;
        }

        private async void ReloadAdapter()
        {
            IsWiFiAdaptersLoading = true;

            await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the reload command)

            Guid id = Guid.Empty;

            if (SelectedWiFiAdapter != null)
                id = SelectedWiFiAdapter.Id;

            WiFiAdapters = await WiFi.GetWiFiAdapterAsync();

            // Change interface...
            SelectedWiFiAdapter = id == Guid.Empty ? WiFiAdapters.FirstOrDefault() : WiFiAdapters.FirstOrDefault(x => x.Id == id);

            IsWiFiAdaptersLoading = false;
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

