using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using Heijden.DNS;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;

namespace NETworkManager.ViewModels
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        private readonly int _tabId;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly bool _isLoading;

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView HostHistoryView { get; }

        private List<QType> _types = new List<QType>();
        public List<QType> Types
        {
            get => _types;
            set
            {
                if (value == _types)
                    return;

                _types = value;
                OnPropertyChanged();
            }
        }

        private QType _type;
        public QType Type
        {
            get => _type;
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
            get => _isLookupRunning;
            set
            {
                if (value == _isLookupRunning)
                    return;

                _isLookupRunning = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DNSLookupRecordInfo> _lookupResult = new ObservableCollection<DNSLookupRecordInfo>();
        public ObservableCollection<DNSLookupRecordInfo> LookupResult
        {
            get => _lookupResult;
            set
            {
                if (value != null && value == _lookupResult)
                    return;

                _lookupResult = value;
            }
        }

        public ICollectionView LookupResultView { get; }

        private DNSLookupRecordInfo _selectedLookupResult;
        public DNSLookupRecordInfo SelectedLookupResult
        {
            get => _selectedLookupResult;
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
            get => _displayStatusMessage;
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
            get => _statusMessage;
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
            get => _startTime;
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
            get => _duration;
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
            get => _endTime;
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
            get => _expandStatistics;
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

        public bool ShowStatistics => SettingsManager.Current.DNSLookup_ShowStatistics;
        #endregion

        #region Contructor, load settings
        public DNSLookupViewModel(int tabId, string host)
        {
            _isLoading = true;

            _tabId = tabId;
            Host = host;

            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_HostHistory);
            LookupResultView = CollectionViewSource.GetDefaultView(LookupResult);
            LookupResultView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DNSLookupRecordInfo.DNSServer)));

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Host))
                StartLookup();

            _firstLoad = false;
        }

        private void LoadSettings()
        {
            LoadTypes();
            
            ExpandStatistics = SettingsManager.Current.DNSLookup_ExpandStatistics;
        }

        private void LoadTypes()
        {
            // Filter by common types...
            Types = SettingsManager.Current.DNSLookup_ShowMostCommonQueryTypes ? Enum.GetValues(typeof(QType)).Cast<QType>().Where(x => (x == QType.A || x == QType.AAAA || x == QType.ANY || x == QType.CNAME || x == QType.MX || x == QType.NS || x == QType.PTR || x == QType.SOA || x == QType.LOC || x == QType.TXT)).OrderBy(x => x.ToString()).ToList() : Enum.GetValues(typeof(QType)).Cast<QType>().OrderBy(x => x.ToString()).ToList();
            Type = Types.FirstOrDefault(x => x == SettingsManager.Current.DNSLookup_Type);

            // Fallback
            if (Type == 0)
                Type = QType.ANY;

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
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            // Reset the latest results
            LookupResult.Clear();

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Host;
                }
            }

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

            var dnsLookup = new DNSLookup();

            dnsLookup.RecordReceived += DNSLookup_RecordReceived;
            dnsLookup.LookupError += DNSLookup_LookupError;
            dnsLookup.LookupComplete += DNSLookup_LookupComplete;

            dnsLookup.ResolveAsync(Host.Split(';').Select(x => x.Trim()).ToList(), dnsLookupOptions);
        }

        private void LookupFinished()
        {
            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();

            IsLookupRunning = false;
        }

        public void OnClose()
        {

        }

        // Modify history list
        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.DNSLookup_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

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
            var dnsLookupRecordInfo = DNSLookupRecordInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (LookupResult)
                    LookupResult.Add(dnsLookupRecordInfo);
            }));
        }

        private void DNSLookup_LookupError(object sender, DNSLookupErrorArgs e)
        {
            if (!string.IsNullOrEmpty(StatusMessage))
                StatusMessage += Environment.NewLine;

            if (e.ErrorCode == "Timeout Error")
                StatusMessage += string.Format(Resources.Localization.Strings.TimeoutWhenQueryingDNSServerMessage, e.DNSServer);
            else
                StatusMessage += Resources.Localization.Strings.UnkownError;

            DisplayStatusMessage = true;

            LookupFinished();
        }

        private void DNSLookup_LookupComplete(object sender, EventArgs e)
        {
            LookupFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.DNSLookup_ShowStatistics):
                    OnPropertyChanged(nameof(ShowStatistics));
                    break;
                case nameof(SettingsInfo.DNSLookup_ShowMostCommonQueryTypes):
                    LoadTypes();
                    break;
            }
        }
        #endregion
    }
}