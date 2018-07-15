using Heijden.DNS;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class IPScannerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private bool _showScanResultForAllIPAddresses;
        public bool ShowScanResultForAllIPAddresses
        {
            get => _showScanResultForAllIPAddresses;
            set
            {
                if (value == _showScanResultForAllIPAddresses)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ShowScanResultForAllIPAddresses = value;

                _showScanResultForAllIPAddresses = value;
                OnPropertyChanged();
            }
        }

        private int _threads;
        public int Threads
        {
            get => _threads;
            set
            {
                if (value == _threads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_Threads = value;

                _threads = value;
                OnPropertyChanged();
            }
        }

        private int _icmpTimeout;
        public int ICMPTimeout
        {
            get => _icmpTimeout;
            set
            {
                if (value == _icmpTimeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ICMPTimeout = value;

                _icmpTimeout = value;
                OnPropertyChanged();
            }
        }

        private int _icmpBuffer;
        public int ICMPBuffer
        {
            get => _icmpBuffer;
            set
            {
                if (value == _icmpBuffer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ICMPBuffer = value;

                _icmpBuffer = value;
                OnPropertyChanged();
            }
        }

        private int _icmpAttempts;
        public int ICMPAttempts
        {
            get => _icmpAttempts;
            set
            {
                if (value == _icmpAttempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ICMPAttempts = value;

                _icmpAttempts = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostname;
        public bool ResolveHostname
        {
            get => _resolveHostname;
            set
            {
                if (value == _resolveHostname)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ResolveHostname = value;

                _resolveHostname = value;
                OnPropertyChanged();
            }
        }

        private bool _useCustomDNSServer;
        public bool UseCustomDNSServer
        {
            get => _useCustomDNSServer;
            set
            {
                if (value == _useCustomDNSServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_UseCustomDNSServer = value;

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
                    SettingsManager.Current.IPScanner_CustomDNSServer = value.Split(';').ToList();

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
                    SettingsManager.Current.IPScanner_DNSPort = value;

                _dnsPort = value;
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
                    SettingsManager.Current.IPScanner_DNSRecursion = value;

                _dnsRecursion = value;
                OnPropertyChanged();
            }
        }

        private bool _dnsUseResolverCache;
        public bool DNSUseResolverCache
        {
            get => _dnsUseResolverCache;
            set
            {
                if (value == _dnsUseResolverCache)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSUseResolverCache = value;

                _dnsUseResolverCache = value;
                OnPropertyChanged();
            }
        }

        public List<TransportType> DNSTransportTypes { get; set; }

        private TransportType _dnsTransportType;
        public TransportType DNSTransportType
        {
            get => _dnsTransportType;
            set
            {
                if (value == _dnsTransportType)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSTransportType = value;

                _dnsTransportType = value;
                OnPropertyChanged();
            }
        }

        private int _dnsAttempts;
        public int DNSAttempts
        {
            get => _dnsAttempts;
            set
            {
                if (value == _dnsAttempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSAttempts = value;

                _dnsAttempts = value;
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
                    SettingsManager.Current.IPScanner_DNSTimeout = value;

                _dnsTimeout = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveMACAddress;
        public bool ResolveMACAddress
        {
            get => _resolveMACAddress;
            set
            {
                if (value == _resolveMACAddress)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ResolveMACAddress = value;

                _resolveMACAddress = value;
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
                    SettingsManager.Current.IPScanner_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public IPScannerSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ShowScanResultForAllIPAddresses = SettingsManager.Current.IPScanner_ShowScanResultForAllIPAddresses;
            Threads = SettingsManager.Current.IPScanner_Threads;
            ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout;
            ICMPBuffer = SettingsManager.Current.IPScanner_ICMPBuffer;
            ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts;
            ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname;
            UseCustomDNSServer = SettingsManager.Current.IPScanner_UseCustomDNSServer;

            if (SettingsManager.Current.IPScanner_CustomDNSServer != null)
                CustomDNSServer = string.Join("; ", SettingsManager.Current.IPScanner_CustomDNSServer);

            DNSPort = SettingsManager.Current.IPScanner_DNSPort;
            DNSRecursion = SettingsManager.Current.IPScanner_DNSRecursion;
            DNSUseResolverCache = SettingsManager.Current.IPScanner_DNSUseResolverCache;
            DNSTransportTypes = Enum.GetValues(typeof(TransportType)).Cast<TransportType>().OrderBy(x => x.ToString()).ToList();
            DNSTransportType = DNSTransportTypes.First(x => x == SettingsManager.Current.IPScanner_DNSTransportType);
            DNSAttempts = SettingsManager.Current.IPScanner_DNSAttempts;
            DNSTimeout = SettingsManager.Current.IPScanner_DNSTimeout;
            ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress;
            ShowStatistics = SettingsManager.Current.IPScanner_ShowStatistics;
        }
        #endregion
    }
}
