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
        private bool _isLoading = true;

        private bool _useCustomDNSServer;
        public bool UseCustomDNSServer
        {
            get { return _useCustomDNSServer; }
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
            get { return _customDNSServer; }
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
            get { return _port;}
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
            get { return _addDNSSuffix; }
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
            get { return _useCustomDNSSuffix; }
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
            get { return _customDNSSuffix; }
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
            get { return _resolveCNAME; }
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
            get { return _recursion; }
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
            get { return _useResolverCache; }
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
            get { return _class; }
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

        public List<TransportType> TransportTypes { get; set; }

        private TransportType _transportType;
        public TransportType TransportType
        {
            get { return _transportType; }
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
            get { return _attempts; }
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
            get { return _timeout; }
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
            get { return _showStatistics; }
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
            TransportTypes = Enum.GetValues(typeof(TransportType)).Cast<TransportType>().OrderBy(x => x.ToString()).ToList();
            TransportType = TransportTypes.First(x => x == SettingsManager.Current.DNSLookup_TransportType);
            Attempts = SettingsManager.Current.DNSLookup_Attempts;
            Timeout = SettingsManager.Current.DNSLookup_Timeout;
            ShowStatistics = SettingsManager.Current.DNSLookup_ShowStatistics;
        }
        #endregion
    }
}