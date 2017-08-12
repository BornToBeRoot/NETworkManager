using Heijden.DNS;
using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsApplicationDNSLookupViewModel : ViewModelBase
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
                    SettingsManager.Current.DNSLookup_CustomDNSServer = value;

                _customDNSServer = value;
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

        public List<QType> Types { get; set; }

        private QType _type;
        public QType Type
        {
            get { return _type; }
            set
            {
                if (value == _type)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Type = value;

                _type = value;
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
        #endregion

        #region Constructor, load settings
        public SettingsApplicationDNSLookupViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            UseCustomDNSServer = SettingsManager.Current.DNSLookup_UseCustomDNSServer;
            CustomDNSServer = SettingsManager.Current.DNSLookup_CustomDNSServer;
            Classes = Enum.GetValues(typeof(QClass)).Cast<QClass>().OrderBy(x => x.ToString()).ToList();
            Class = Classes.First(x => x == SettingsManager.Current.DNSLookup_Class);
            Types = Enum.GetValues(typeof(QType)).Cast<QType>().OrderBy(x => x.ToString()).ToList();
            Type = Types.First(x => x == SettingsManager.Current.DNSLookup_Type);
            Recursion = SettingsManager.Current.DNSLookup_Recursion;
            UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache;
            TransportTypes = Enum.GetValues(typeof(TransportType)).Cast<TransportType>().OrderBy(x => x.ToString()).ToList();
            TransportType = TransportTypes.First(x => x == SettingsManager.Current.DNSLookup_TransportType);
            Attempts = SettingsManager.Current.DNSLookup_Attempts;
            Timeout = SettingsManager.Current.DNSLookup_Timeout;
        }
        #endregion
    }
}