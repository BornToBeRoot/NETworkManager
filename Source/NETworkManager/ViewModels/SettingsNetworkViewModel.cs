using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class SettingsNetworkViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private bool _useCustomDNSServer;
        public bool UseCustomDNSServer
        {
            get => _useCustomDNSServer;
            set
            {
                if (value == _useCustomDNSServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_UseCustomDNSServer = value;

                _useCustomDNSServer = value;
                OnPropertyChanged();
            }
        }

        
        private string _customDNSServer;
        public string CustomDNSServer
        {
            get => _customDNSServer;
            set
            {
                if (value == _customDNSServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_CustomDNSServer = value;

                _customDNSServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsNetworkViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            UseCustomDNSServer = SettingsManager.Current.Network_UseCustomDNSServer;

            if (SettingsManager.Current.Network_CustomDNSServer != null)
                CustomDNSServer = string.Join("; ", SettingsManager.Current.Network_CustomDNSServer);            
        }
        #endregion
    }
}