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

        private bool _displayErrorMessage;
        public bool DisplayErrorMessage
        {
            get { return _displayErrorMessage; }
            set
            {
                if (value == _displayErrorMessage)
                    return;

                _displayErrorMessage = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (value == _errorMessage)
                    return;

                _errorMessage = value;
                OnPropertyChanged();
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
            DisplayErrorMessage = false;
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
                Timeout = SettingsManager.Current.DNSLookup_Timeout,
                ResolveCNAME = SettingsManager.Current.DNSLookup_ResolveCNAME
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

            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,new Action(delegate ()
            {
                LookupResult.Add(dnsLookupRecordInfo);
            }));                        
        }

        private void DnsLookup_LookupError(object sender, DNSLookupErrorArgs e)
        {
            if (e.ErrorCode == "Timeout Error")
                ErrorMessage = string.Format(Application.Current.Resources["String_TimeoutWhenQueryingDNSServer"] as string, e.DNSServer);
            else
                ErrorMessage = Application.Current.Resources["String_UnkownError"] as string;

            DisplayErrorMessage = true;

            IsLookupRunning = false;
        }

        private void DnsLookup_LookupComplete(object sender, DNSLookupCompleteArgs e)
        {
            if (e.ResourceRecordsCount == 0)
            {
                ErrorMessage = string.Format(Application.Current.Resources["String_NoDnsRecordFoundCheckYourInputAndSettings"] as string, HostnameOrIPAddress);
                DisplayErrorMessage = true;
            }

            IsLookupRunning = false;
        }
        #endregion
    }
}