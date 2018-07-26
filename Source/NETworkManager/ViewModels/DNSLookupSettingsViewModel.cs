using Heijden.DNS;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class DNSLookupSettingsViewModel : ViewModelBase
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
                    SettingsManager.Current.DNSLookup_UseCustomDNSServer = value;

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
                    SettingsManager.Current.DNSLookup_CustomDNSServer = value.Split(';').ToList();

                _customDNSServer = value;
                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                if (value == _port)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Port = value;

                _port = value;
                OnPropertyChanged();
            }
        }

        private bool _addDNSSuffix;
        public bool AddDNSSuffix
        {
            get => _addDNSSuffix;
            set
            {
                if (value == _addDNSSuffix)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_AddDNSSuffix = value;

                _addDNSSuffix = value;
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
                    SettingsManager.Current.DNSLookup_UseCustomDNSSuffix = value;

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
                    SettingsManager.Current.DNSLookup_CustomDNSSuffix = value;

                _customDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveCNAME;
        public bool ResolveCNAME
        {
            get => _resolveCNAME;
            set
            {
                if (value == _resolveCNAME)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ResolveCNAME = value;

                _resolveCNAME = value;
                OnPropertyChanged();
            }
        }

        private bool _recursion;
        public bool Recursion
        {
            get => _recursion;
            set
            {
                if (value == _recursion)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Recursion = value;

                _recursion = value;
                OnPropertyChanged();
            }
        }

        private bool _useResolverCache;
        public bool UseResolverCache
        {
            get => _useResolverCache;
            set
            {
                if (value == _useResolverCache)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_UseResolverCache = value;

                _useResolverCache = value;
                OnPropertyChanged();
            }
        }

        public List<QClass> Classes { get; set; }

        private QClass _class;
        public QClass Class
        {
            get => _class;
            set
            {
                if (value == _class)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Class = value;

                _class = value;
                OnPropertyChanged();
            }
        }

        private bool _showMostCommonQueryTypes;
        public bool ShowMostCommonQueryTypes
        {
            get => _showMostCommonQueryTypes;
            set
            {
                if (value == _showMostCommonQueryTypes)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ShowMostCommonQueryTypes = value;

                _showMostCommonQueryTypes = value;
                OnPropertyChanged();
            }
        }

        public List<TransportType> TransportTypes { get; set; }

        private TransportType _transportType;
        public TransportType TransportType
        {
            get => _transportType;
            set
            {
                if (value == _transportType)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_TransportType = value;

                _transportType = value;
                OnPropertyChanged();
            }
        }

        private int _attempts;
        public int Attempts
        {
            get => _attempts;
            set
            {
                if (value == _attempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Attempts = value;

                _attempts = value;
                OnPropertyChanged();
            }
        }

        private int _timeout;
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private bool _showStatistics;
        public bool ShowStatistics
        {
            get => _showStatistics;
            set
            {
                if (value == _showStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public DNSLookupSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            UseCustomDNSServer = SettingsManager.Current.DNSLookup_UseCustomDNSServer;

            if (SettingsManager.Current.DNSLookup_CustomDNSServer != null)
                CustomDNSServer = string.Join("; ", SettingsManager.Current.DNSLookup_CustomDNSServer);

            Port = SettingsManager.Current.DNSLookup_Port;
            AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix;
            UseCustomDNSSuffix = SettingsManager.Current.DNSLookup_UseCustomDNSSuffix;
            CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix;
            ResolveCNAME = SettingsManager.Current.DNSLookup_ResolveCNAME;
            Recursion = SettingsManager.Current.DNSLookup_Recursion;
            UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache;
            Classes = Enum.GetValues(typeof(QClass)).Cast<QClass>().OrderBy(x => x.ToString()).ToList();
            Class = Classes.First(x => x == SettingsManager.Current.DNSLookup_Class);
            ShowMostCommonQueryTypes = SettingsManager.Current.DNSLookup_ShowMostCommonQueryTypes;
            TransportTypes = Enum.GetValues(typeof(TransportType)).Cast<TransportType>().OrderBy(x => x.ToString()).ToList();
            TransportType = TransportTypes.First(x => x == SettingsManager.Current.DNSLookup_TransportType);
            Attempts = SettingsManager.Current.DNSLookup_Attempts;
            Timeout = SettingsManager.Current.DNSLookup_Timeout;
            ShowStatistics = SettingsManager.Current.DNSLookup_ShowStatistics;
        }
        #endregion
    }
}