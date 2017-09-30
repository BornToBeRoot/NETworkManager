using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Collections;
using System.Net.NetworkInformation;
using System.Windows.Threading;
using System.Diagnostics;

namespace NETworkManager.ViewModels.Applications
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

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

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get { return _startTime; }
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get { return _endTime; }
            set
            {
                if (value == _endTime)
                    return;

                _endTime = value;
                OnPropertyChanged();
            }
        }

        private string _dnsServerAndPort;
        public string DNSServerAndPort
        {
            get { return _dnsServerAndPort; }
            set
            {
                if (value == _dnsServerAndPort)
                    return;

                _dnsServerAndPort = value;
                OnPropertyChanged();
            }
        }

        private int _questions;
        public int Questions
        {
            get { return _questions; }
            set
            {
                if (value == _questions)
                    return;

                _questions = value;
                OnPropertyChanged();
            }
        }

        private int _answers;
        public int Answers
        {
            get { return _answers; }
            set
            {
                if (value == _answers)
                    return;

                _answers = value;
                OnPropertyChanged();
            }
        }

        private int _authorities;
        public int Authorities
        {
            get { return _authorities; }
            set
            {
                if (value == _authorities)
                    return;

                _authorities = value;
                OnPropertyChanged();
            }
        }

        private int _additionals;
        public int Additionals
        {
            get { return _additionals; }
            set
            {
                if (value == _additionals)
                    return;

                _additionals = value;
                OnPropertyChanged();
            }
        }

        private int _messageSize;
        public int MessageSize
        {
            get { return _messageSize; }
            set
            {
                if (value == _messageSize)
                    return;

                _messageSize = value;
                OnPropertyChanged();
            }
        }

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get { return _expandStatistics; }
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor
        public DNSLookupViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }
        #endregion

        #region Load settings
        private void LoadSettings()
        {
            if (SettingsManager.Current.DNSLookup_HostnameOrIPAddressHistory != null)
                HostnameOrIPAddressHistory = new List<string>(SettingsManager.Current.DNSLookup_HostnameOrIPAddressHistory);

            ExpandStatistics = SettingsManager.Current.DNSLookup_ExpandStatistics;
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
            DisplayStatusMessage = false;
            IsLookupRunning = true;

            // Reset statistic
            DNSServerAndPort = string.Empty;
            Questions = 0;
            Answers = 0;
            Authorities = 0;
            Additionals = 0;
            MessageSize = 0;

            // Measure the time
            StartTime = DateTime.Now;
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            // Reset the latest results
            LookupResult.Clear();

            HostnameOrIPAddressHistory = new List<string>(HistoryListHelper.Modify(HostnameOrIPAddressHistory, HostnameOrIPAddress, SettingsManager.Current.Application_HistoryListEntries));

            DNSLookupOptions DNSLookupOptions = new DNSLookupOptions();

            if (SettingsManager.Current.DNSLookup_UseCustomDNSServer)
            {
                if (!string.IsNullOrEmpty(SettingsManager.Current.DNSLookup_CustomDNSServer))
                {
                    DNSLookupOptions.UseCustomDNSServer = SettingsManager.Current.DNSLookup_UseCustomDNSServer;
                    DNSLookupOptions.CustomDNSServer = SettingsManager.Current.DNSLookup_CustomDNSServer;
                }
                else
                {
                    StatusMessage = Application.Current.Resources["String_CustomDNSServerIsEmptyCheckYourSettingsUseWindowsOwnDNSServer"] as string;
                    DisplayStatusMessage = true;
                }
            }

            DNSLookupOptions.Class = SettingsManager.Current.DNSLookup_Class;
            DNSLookupOptions.Type = SettingsManager.Current.DNSLookup_Type;
            DNSLookupOptions.Recursion = SettingsManager.Current.DNSLookup_Recursion;
            DNSLookupOptions.UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache;
            DNSLookupOptions.TransportType = SettingsManager.Current.DNSLookup_TransportType;
            DNSLookupOptions.Attempts = SettingsManager.Current.DNSLookup_Attempts;
            DNSLookupOptions.Timeout = SettingsManager.Current.DNSLookup_Timeout;
            DNSLookupOptions.ResolveCNAME = SettingsManager.Current.DNSLookup_ResolveCNAME;

            DNSLookup DNSLookup = new DNSLookup();

            DNSLookup.RecordReceived += DNSLookup_RecordReceived;
            DNSLookup.LookupError += DNSLookup_LookupError;
            DNSLookup.LookupComplete += DNSLookup_LookupComplete;

            string hostnameOrIPAddress = HostnameOrIPAddress;
            string dnsSuffix = string.Empty;

            // Detect hostname (usually they don't contain ".")
            if (HostnameOrIPAddress.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
            {
                if (SettingsManager.Current.DNSLookup_AddDNSSuffix)
                {
                    if (SettingsManager.Current.DNSLookup_UseCustomDNSSuffix)
                        dnsSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix;
                    else
                        dnsSuffix = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                }
            }

            // Append dns suffix to hostname
            if (!string.IsNullOrEmpty(dnsSuffix))
                hostnameOrIPAddress += string.Format("{0}{1}", dnsSuffix.StartsWith(".") ? "" : ".", dnsSuffix);

            DNSLookup.LookupAsync(hostnameOrIPAddress, DNSLookupOptions);
        }

        private void LookupFinished()
        {
            // Stop timer and stopwatch
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();

            IsLookupRunning = false;
        }
        #endregion

        #region Events
        private void DNSLookup_RecordReceived(object sender, DNSLookupRecordArgs e)
        {
            DNSLookupRecordInfo DNSLookupRecordInfo = DNSLookupRecordInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate ()
             {
                 LookupResult.Add(DNSLookupRecordInfo);
             }));
        }

        private void DNSLookup_LookupError(object sender, DNSLookupErrorArgs e)
        {
            if (e.ErrorCode == "Timeout Error")
                StatusMessage = string.Format(Application.Current.Resources["String_TimeoutWhenQueryingDNSServer"] as string, e.DNSServer);
            else
                StatusMessage = Application.Current.Resources["String_UnkownError"] as string;

            DisplayStatusMessage = true;

            LookupFinished();
        }

        private void DNSLookup_LookupComplete(object sender, DNSLookupCompleteArgs e)
        {
            DNSServerAndPort = e.ServerAndPort;
            Questions = e.QuestionsCount;
            Answers = e.AnswersCount;
            Authorities = e.AuthoritiesCount;
            Additionals = e.AdditionalsCount;
            MessageSize = e.MessageSize;

            if (e.AnswersCount == 0)
            {
                StatusMessage = string.Format(Application.Current.Resources["String_NoDNSRecordFoundCheckYourInputAndSettings"] as string, HostnameOrIPAddress);
                DisplayStatusMessage = true;
            }

            LookupFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}