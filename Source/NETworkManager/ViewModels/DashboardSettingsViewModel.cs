using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class DashboardSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private string _publicICMPTestIPAddress;
        public string PublicICMPTestIPAddress
        {
            get => _publicICMPTestIPAddress;
            set
            {
                if (value == _publicICMPTestIPAddress)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_PublicICMPTestIPAddress = value;

                _publicICMPTestIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _publicDNSTestDomain;
        public string PublicDNSTestDomain
        {
            get => _publicDNSTestDomain;
            set
            {
                if (value == _publicDNSTestDomain)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_PublicDNSTestDomain = value;

                _publicDNSTestDomain = value;
                OnPropertyChanged();
            }
        }

        private string _publicDNSTestIPAddress;
        public string PublicDNSTestIPAddress
        {
            get => _publicDNSTestIPAddress;
            set
            {
                if (value == _publicDNSTestIPAddress)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_PublicDNSTestIPAddress = value;

                _publicDNSTestIPAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _checkPublicIPAddress;
        public bool CheckPublicIPAddress
        {
            get => _checkPublicIPAddress;
            set
            {
                if (value == _checkPublicIPAddress)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_CheckPublicIPAddress = value;

                _checkPublicIPAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _usePublicIPAddressCustomAPI;
        public bool UsePublicIPAddressCustomAPI
        {
            get => _usePublicIPAddressCustomAPI;
            set
            {
                if (value == _usePublicIPAddressCustomAPI)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_UseCustomPublicIPAddressAPI = value;

                _usePublicIPAddressCustomAPI = value;
                OnPropertyChanged();
            }
        }

        private string _customPublicIPAddressAPI;
        public string CustomPublicIPAddressAPI
        {
            get => _customPublicIPAddressAPI;
            set
            {
                if (value == _customPublicIPAddressAPI)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_CustomPublicIPAddressAPI = value;

                _customPublicIPAddressAPI = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public DashboardSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            PublicICMPTestIPAddress = SettingsManager.Current.Dashboard_PublicICMPTestIPAddress;
            PublicDNSTestDomain = SettingsManager.Current.Dashboard_PublicDNSTestDomain;
            PublicDNSTestIPAddress = SettingsManager.Current.Dashboard_PublicDNSTestIPAddress;
            CheckPublicIPAddress = SettingsManager.Current.Dashboard_CheckPublicIPAddress;
            UsePublicIPAddressCustomAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPAddressAPI;
            CustomPublicIPAddressAPI = SettingsManager.Current.Dashboard_CustomPublicIPAddressAPI;
        }
        #endregion
    }
}