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

        private int _dnsPort;
        public int DNSPort
        {
            get => _dnsPort;
            set
            {
                if (value == _dnsPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_CustomDNSPort = value;

                _dnsPort = value;
                OnPropertyChanged();
            }
        }

        private bool _useCustomDNSSuffix;
        public bool UseCustomDNSSuffix
        {
            get => _useCustomDNSSuffix;
            set
            {
                if (value == _useCustomDNSSuffix)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_UseCustomDNSSuffix = value;

                _useCustomDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private string _customDNSSuffix;
        public string CustomDNSSuffix
        {
            get => _customDNSSuffix;
            set
            {
                if (value == _customDNSSuffix)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_CustomDNSSuffix = value;

                _customDNSSuffix = value;
                OnPropertyChanged();
            }
        }
        
        private bool _dnsRecursion;
        public bool DNSRecursion
        {
            get => _dnsRecursion;
            set
            {
                if (value == _dnsRecursion)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_DNSRecursion = value;

                _dnsRecursion = value;
                OnPropertyChanged();
            }
        }

        private bool _dnsUseCache;
        public bool DNSUseCache
        {
            get => _dnsUseCache;
            set
            {
                if (value == _dnsUseCache)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_DNSUseCache = value;

                _dnsUseCache = value;
                OnPropertyChanged();
            }
        }

        private bool _dnsUseTCPOnly;
        public bool DNSUseTCPOnly
        {
            get => _dnsUseTCPOnly;
            set
            {
                if (value == _dnsUseTCPOnly)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_DNSUseTCPOnly = value;

                _dnsUseTCPOnly = value;
                OnPropertyChanged();
            }
        }

        private int _dnsRetries;
        public int DNSRetries
        {
            get => _dnsRetries;
            set
            {
                if (value == _dnsRetries)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_DNSRetries = value;

                _dnsRetries = value;
                OnPropertyChanged();
            }
        }

        private int _dnsTimeout;
        public int DNSTimeout
        {
            get => _dnsTimeout;
            set
            {
                if (value == _dnsTimeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Network_DNSTimeout = value;

                _dnsTimeout = value;
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

            DNSPort = SettingsManager.Current.Network_CustomDNSPort;
            UseCustomDNSSuffix = SettingsManager.Current.Network_UseCustomDNSSuffix;
            CustomDNSSuffix = SettingsManager.Current.Network_CustomDNSSuffix;
            DNSRecursion = SettingsManager.Current.Network_DNSRecursion;
            DNSUseCache = SettingsManager.Current.Network_DNSUseCache;
            DNSUseTCPOnly = SettingsManager.Current.Network_DNSUseTCPOnly;
            DNSRetries = SettingsManager.Current.Network_DNSRetries;
            DNSTimeout = SettingsManager.Current.Network_DNSTimeout;
        }
        #endregion
    }
}