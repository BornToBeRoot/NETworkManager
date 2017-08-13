using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Collections;

namespace NETworkManager.ViewModels.Applications
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        private bool _isLoading = true;

        private string _hostnameOrIPAddress;
        public string HostnameOrIPAddress
        {
            get { return _hostnameOrIPAddress; }
            set
            {
                if (value == _hostnameOrIPAddress)
                    return;

                _hostnameOrIPAddress = value;
                OnPropertyChanged();
            }
        }

        private List<string> _hostnameOrIPAddressHistory = new List<string>();
        public List<string> HostnameOrIPAddressHistory
        {
            get { return _hostnameOrIPAddressHistory; }
            set
            {
                if (value == _hostnameOrIPAddressHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_HostnameOrIPAddressHistory = value;

                _hostnameOrIPAddressHistory = value;
                OnPropertyChanged();
            }
        }

        private bool _isLookupRunning;
        public bool IsLookupRunning
        {
            get { return _isLookupRunning; }
            set
            {
                if (value == _isLookupRunning)
                    return;

                _isLookupRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelLookup;
        public bool CancelLookup
        {
            get { return _cancelLookup; }
            set
            {
                if (value == _cancelLookup)
                    return;

                _cancelLookup = value;
                OnPropertyChanged();
            }
        }

        private AsyncObservableCollection<DNSLookupRecordInfo> _lookupResult = new AsyncObservableCollection<DNSLookupRecordInfo>();
        public AsyncObservableCollection<DNSLookupRecordInfo> LookupResult
        {
            get { return _lookupResult; }
            set
            {
                if (value == _lookupResult)
                    return;

                _lookupResult = value;
            }
        }
        #endregion

        #region Contructor
        public DNSLookupViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadSettings();

            _isLoading = false;
        }
        #endregion

        #region Load settings
        private void LoadSettings()
        {
            if (SettingsManager.Current.DNSLookup_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(SettingsManager.Current.DNSLookup_HostnameOrIPAddressHistory);
        }
        #endregion

        #region ICommands & Actions
        public ICommand LookupCommand
        {
            get { return new RelayCommand(p => LookupAction()); }
        }

        private void LookupAction()
        {
            if (!IsLookupRunning)
                StartLookup();
        }
        #endregion

        #region Methods      
        private void StartLookup()
        {
            IsLookupRunning = true;

            // Reset the latest results
            LookupResult.Clear();

            HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, SettingsManager.Current.Application_HistoryListEntries));

            DNSLookupOptions dnsLookupOptions = new DNSLookupOptions
            {
                UseCustomDNSServer = SettingsManager.Current.DNSLookup_UseCustomDNSServer,
                CustomDNSServer = SettingsManager.Current.DNSLookup_CustomDNSServer,
                Class = SettingsManager.Current.DNSLookup_Class,
                Type = SettingsManager.Current.DNSLookup_Type,
                Recursion = SettingsManager.Current.DNSLookup_Recursion,
                UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache,
                TransportType = SettingsManager.Current.DNSLookup_TransportType,
                Attempts = SettingsManager.Current.DNSLookup_Attempts,
                Timeout = SettingsManager.Current.DNSLookup_Timeout
            };

            DNSLookup dnsLookup = new DNSLookup();

            dnsLookup.RecordReceived += DnsLookup_RecordReceived;
            dnsLookup.LookupError += DnsLookup_LookupError;
            dnsLookup.LookupComplete += DnsLookup_LookupComplete;

            dnsLookup.LookupAsync(HostnameOrIPAddress, dnsLookupOptions);
        }
        #endregion

        #region Events
        private void DnsLookup_RecordReceived(object sender, DNSLookupRecordArgs e)
        {
            DNSLookupRecordInfo dnsLookupRecordInfo = DNSLookupRecordInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                LookupResult.Add(dnsLookupRecordInfo);
            }));                        
        }

        private void DnsLookup_LookupError(object sender, DNSLookupErrorArgs e)
        {
            MessageBox.Show(e.Error);
            IsLookupRunning = false;
        }

        private void DnsLookup_LookupComplete(object sender, EventArgs e)
        {
            IsLookupRunning = false;
        }
        #endregion
    }
}