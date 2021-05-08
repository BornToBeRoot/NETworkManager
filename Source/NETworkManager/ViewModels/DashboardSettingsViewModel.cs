using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class DashboardSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private string _publicIPv4Address;
        public string PublicIPv4Address
        {
            get => _publicIPv4Address;
            set
            {
                if (value == _publicIPv4Address)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_PublicIPv4Address = value;

                _publicIPv4Address = value;
                OnPropertyChanged();
            }
        }

        private string _publicIPv6Address;
        public string PublicIPv6Address
        {
            get => _publicIPv6Address;
            set
            {
                if (value == _publicIPv6Address)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_PublicIPv6Address = value;

                _publicIPv6Address = value;
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

        private bool _usePublicIPv4AddressCustomAPI;
        public bool UsePublicIPv4AddressCustomAPI
        {
            get => _usePublicIPv4AddressCustomAPI;
            set
            {
                if (value == _usePublicIPv4AddressCustomAPI)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI = value;

                _usePublicIPv4AddressCustomAPI = value;
                OnPropertyChanged();
            }
        }

        private string _customPublicIPv4AddressAPI;
        public string CustomPublicIPv4AddressAPI
        {
            get => _customPublicIPv4AddressAPI;
            set
            {
                if (value == _customPublicIPv4AddressAPI)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI = value;

                _customPublicIPv4AddressAPI = value;
                OnPropertyChanged();
            }
        }

        private bool _usePublicIPv6AddressCustomAPI;
        public bool UsePublicIPv6AddressCustomAPI
        {
            get => _usePublicIPv6AddressCustomAPI;
            set
            {
                if (value == _usePublicIPv6AddressCustomAPI)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI = value;

                _usePublicIPv6AddressCustomAPI = value;
                OnPropertyChanged();
            }
        }

        private string _customPublicIPv6AddressAPI;
        public string CustomPublicIPv6AddressAPI
        {
            get => _customPublicIPv6AddressAPI;
            set
            {
                if (value == _customPublicIPv6AddressAPI)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI = value;

                _customPublicIPv6AddressAPI = value;
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
            PublicIPv4Address = SettingsManager.Current.Dashboard_PublicIPv4Address;
            PublicIPv6Address = SettingsManager.Current.Dashboard_PublicIPv6Address;
            CheckPublicIPAddress = SettingsManager.Current.Dashboard_CheckPublicIPAddress;
            UsePublicIPv4AddressCustomAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI;
            CustomPublicIPv4AddressAPI = SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI;
            UsePublicIPv6AddressCustomAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI;
            CustomPublicIPv6AddressAPI = SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI;
        }
        #endregion
    }
}