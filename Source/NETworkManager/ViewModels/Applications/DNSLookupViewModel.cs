using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Collections;
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
            _lookupResultView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DNSLookupRecordInfo.DNSServer)));

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
            StatusMessage = string.Empty;

            IsLookupRunning = true;

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
                dnsLookupOptions.UseCustomDNSServer = SettingsManager.Current.DNSLookup_UseCustomDNSServer;
                dnsLookupOptions.CustomDNSServers = SettingsManager.Current.DNSLookup_CustomDNSServer.Select(x => x.Trim()).ToList();
                dnsLookupOptions.Port = SettingsManager.Current.DNSLookup_Port;
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

            DNSLookup.LookupAsync(Host.Split(';').Select(x => x.Trim()).ToList(), dnsLookupOptions);
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

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(delegate ()
            {
                LookupResult.Add(DNSLookupRecordInfo);
            }));
        }

        private void DNSLookup_LookupError(object sender, DNSLookupErrorArgs e)
        {
            if (!string.IsNullOrEmpty(StatusMessage))
                StatusMessage += Environment.NewLine;

            if (e.ErrorCode == "Timeout Error")
                StatusMessage += string.Format(Application.Current.Resources["String_TimeoutWhenQueryingDNSServer"] as string, e.DNSServer);
            else
                StatusMessage += Application.Current.Resources["String_UnkownError"] as string;

            DisplayStatusMessage = true;

            LookupFinished();
        }

        private void DNSLookup_LookupComplete(object sender, EventArgs e)
        {
            LookupFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}