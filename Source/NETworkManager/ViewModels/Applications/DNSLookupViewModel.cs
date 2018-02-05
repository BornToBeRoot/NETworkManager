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
using Heijden.DNS;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace NETworkManager.ViewModels.Applications
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private bool _isLoading = true;

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _hostHistoryView;
        public ICollectionView HostHistoryView
        {
            get { return _hostHistoryView; }
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

        private ICollectionView _lookupResultView;
        public ICollectionView LookupResultView
        {
            get { return _lookupResultView; }
        }

        private DNSLookupRecordInfo _selectedLookupResult;
        public DNSLookupRecordInfo SelectedLookupResult
        {
            get { return _selectedLookupResult; }
            set
            {
                if (value == _selectedLookupResult)
                    return;

                _selectedLookupResult = value;
                OnPropertyChanged();
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

        #region Contructor, load settings
        public DNSLookupViewModel()
        {
            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_HostHistory);
            _lookupResultView = CollectionViewSource.GetDefaultView(LookupResult);

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Types = Enum.GetValues(typeof(QType)).Cast<QType>().OrderBy(x => x.ToString()).ToList();
            Type = Types.First(x => x == SettingsManager.Current.DNSLookup_Type);

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

        public ICommand CopySelectedNameCommand
        {
            get { return new RelayCommand(p => CopySelectedNameAction()); }
        }

        private void CopySelectedNameAction()
        {
            Clipboard.SetText(SelectedLookupResult.Name);
        }

        public ICommand CopySelectedTTLCommand
        {
            get { return new RelayCommand(p => CopySelectedTTLAction()); }
        }

        private void CopySelectedTTLAction()
        {
            Clipboard.SetText(SelectedLookupResult.TTL.ToString());
        }

        public ICommand CopySelectedClassCommand
        {
            get { return new RelayCommand(p => CopySelectedClassAction()); }
        }

        private void CopySelectedClassAction()
        {
            Clipboard.SetText(SelectedLookupResult.Class);
        }

        public ICommand CopySelectedTypeCommand
        {
            get { return new RelayCommand(p => CopySelectedTypeAction()); }
        }

        private void CopySelectedTypeAction()
        {
            Clipboard.SetText(SelectedLookupResult.Type);
        }

        public ICommand CopySelectedResultCommand
        {
            get { return new RelayCommand(p => CopySelectedResultAction()); }
        }

        private void CopySelectedResultAction()
        {
            Clipboard.SetText(SelectedLookupResult.Result);
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

            AddHostToHistory(Host);

            DNSLookupOptions dnsLookupOptions = new DNSLookupOptions();

            if (SettingsManager.Current.DNSLookup_UseCustomDNSServer)
            {
                if (SettingsManager.Current.DNSLookup_CustomDNSServers != null)
                {
                    dnsLookupOptions.UseCustomDNSServer = SettingsManager.Current.DNSLookup_UseCustomDNSServer;
                    dnsLookupOptions.CustomDNSServers = SettingsManager.Current.DNSLookup_CustomDNSServers;
                }
                else
                {
                    StatusMessage = Application.Current.Resources["String_CustomDNSServerIsEmptyCheckYourSettingsUseWindowsOwnDNSServer"] as string;
                    DisplayStatusMessage = true;
                }
            }

            dnsLookupOptions.AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix;

            if (SettingsManager.Current.DNSLookup_UseCustomDNSSuffix)
            {
                dnsLookupOptions.UseCustomDNSSuffix = true;
                dnsLookupOptions.CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix.TrimStart('.');
            }

            dnsLookupOptions.Class = SettingsManager.Current.DNSLookup_Class;
            dnsLookupOptions.Type = Type;
            dnsLookupOptions.Recursion = SettingsManager.Current.DNSLookup_Recursion;
            dnsLookupOptions.UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache;
            dnsLookupOptions.TransportType = SettingsManager.Current.DNSLookup_TransportType;
            dnsLookupOptions.Attempts = SettingsManager.Current.DNSLookup_Attempts;
            dnsLookupOptions.Timeout = SettingsManager.Current.DNSLookup_Timeout;
            dnsLookupOptions.ResolveCNAME = SettingsManager.Current.DNSLookup_ResolveCNAME;

            DNSLookup DNSLookup = new DNSLookup();

            DNSLookup.RecordReceived += DNSLookup_RecordReceived;
            DNSLookup.LookupError += DNSLookup_LookupError;
            DNSLookup.LookupComplete += DNSLookup_LookupComplete; ;

            DNSLookup.LookupAsync(Host.Split(';').ToList(), dnsLookupOptions);
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

        // Modify history list
        private void AddHostToHistory(string host)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.DNSLookup_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.DNSLookup_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.DNSLookup_HostHistory.Add(x));
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

        private void DNSLookup_LookupComplete(object sender, EventArgs e)
        {
            LookupFinished();
        }

        /* private void DNSLookup_LookupComplete1(object sender, DNSLookupCompleteArgs e)
         {
             DNSServerAndPort = e.ServerAndPort;
             Questions = e.QuestionsCount;
             Answers = e.AnswersCount;
             Authorities = e.AuthoritiesCount;
             Additionals = e.AdditionalsCount;
             MessageSize = e.MessageSize;

             if (e.AnswersCount == 0)
             {
                 StatusMessage = string.Format(Application.Current.Resources["String_NoDNSRecordFoundCheckYourInputAndSettings"] as string, Host);
                 DisplayStatusMessage = true;
             }
         }*/

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}