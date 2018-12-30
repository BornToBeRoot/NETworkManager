using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System;
using System.Collections;
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
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        
        private readonly int _tabId;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private string _lastSortDescriptionAscending = string.Empty;
        
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

        public ICollectionView DNSServers { get; }

        private DNSServerInfo _dnsServer = new DNSServerInfo();
        public DNSServerInfo DNSServer
        {
            get => _dnsServer;
            set
            {
                if (value == _dnsServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_SelectedDNSServer = value;

                _dnsServer = value;
                OnPropertyChanged();
            }
        }

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

        private ObservableCollection<DNSLookupRecordInfo> _lookupResults = new ObservableCollection<DNSLookupRecordInfo>();
        public ObservableCollection<DNSLookupRecordInfo> LookupResults
        {
            get => _lookupResults;
            set
            {
               if(Equals(value, _lookupResults))
                   return;

                _lookupResults = value;
            }
        }

        public ICollectionView LookupResultsView { get; }

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

        private IList _selectedLookupResults = new ArrayList();
        public IList SelectedLookupResults
        {
            get => _selectedLookupResults;
            set
            {
                if (Equals(value, _selectedLookupResults))
                    return;

                _selectedLookupResults = value;
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
        public DNSLookupViewModel(IDialogCoordinator instance, int tabId, string host)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            _tabId = tabId;
            Host = host;

            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_HostHistory);

            if (SettingsManager.Current.DNSLookup_DNSServers.Count == 0)
                SettingsManager.Current.DNSLookup_DNSServers = new ObservableCollection<DNSServerInfo>(Models.Network.DNSServer.DefaultDNSServerList());

            DNSServers = new CollectionViewSource { Source = SettingsManager.Current.DNSLookup_DNSServers }.View;
            DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerInfo.UseWindowsDNSServer), ListSortDirection.Descending));
            DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerInfo.Name), ListSortDirection.Ascending));
            DNSServer = DNSServers.SourceCollection.Cast<DNSServerInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.DNSLookup_SelectedDNSServer.Name) ?? DNSServers.SourceCollection.Cast<DNSServerInfo>().First();

            LookupResultsView = CollectionViewSource.GetDefaultView(LookupResults);
            LookupResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DNSLookupRecordInfo.DNSServer)));
            LookupResultsView.SortDescriptions.Add(new SortDescription(nameof(DNSLookupRecordInfo.DNSServer), ListSortDirection.Descending));

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
            CommonMethods.SetClipboard(SelectedLookupResult.Name);
        }

        public ICommand CopySelectedTTLCommand
        {
            get { return new RelayCommand(p => CopySelectedTTLAction()); }
        }

        private void CopySelectedTTLAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.TTL.ToString());
        }

        public ICommand CopySelectedClassCommand
        {
            get { return new RelayCommand(p => CopySelectedClassAction()); }
        }

        private void CopySelectedClassAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.Class);
        }

        public ICommand CopySelectedTypeCommand
        {
            get { return new RelayCommand(p => CopySelectedTypeAction()); }
        }

        private void CopySelectedTypeAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.Type);
        }

        public ICommand CopySelectedResultCommand
        {
            get { return new RelayCommand(p => CopySelectedResultAction()); }
        }

        private void CopySelectedResultAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.Result);
        }

        public ICommand ExportCommand
        {
            get { return new RelayCommand(p => ExportAction()); }
        }

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? LookupResults : new ObservableCollection<DNSLookupRecordInfo>(SelectedLookupResults.Cast<DNSLookupRecordInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, Resources.Localization.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.DNSLookup_ExportFileType = instance.FileType;
                SettingsManager.Current.DNSLookup_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, SettingsManager.Current.DNSLookup_ExportFileType, SettingsManager.Current.DNSLookup_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
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
            LookupResults.Clear();

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

            var dnsLookupOptions = new DNSLookupOptions
            {
                AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix,
                Class = SettingsManager.Current.DNSLookup_Class,
                Type = Type,
                Recursion = SettingsManager.Current.DNSLookup_Recursion,
                UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache,
                TransportType = SettingsManager.Current.DNSLookup_TransportType,
                Attempts = SettingsManager.Current.DNSLookup_Attempts,
                Timeout = SettingsManager.Current.DNSLookup_Timeout,
                ResolveCNAME = SettingsManager.Current.DNSLookup_ResolveCNAME
            };

            if (!DNSServer.UseWindowsDNSServer)
            {
                dnsLookupOptions.UseCustomDNSServer = true;
                dnsLookupOptions.CustomDNSServers = DNSServer.Server;
                dnsLookupOptions.Port = DNSServer.Port;
            }

            if (SettingsManager.Current.DNSLookup_UseCustomDNSSuffix)
            {
                dnsLookupOptions.UseCustomDNSSuffix = true;
                dnsLookupOptions.CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix.TrimStart('.');
            }
            
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

        public void SortResultByPropertyName(string sortDescription)
        {
            LookupResultsView.SortDescriptions.Clear();
            LookupResultsView.SortDescriptions.Add(new SortDescription(nameof(DNSLookupRecordInfo.DNSServer), ListSortDirection.Descending));

            if (_lastSortDescriptionAscending.Equals(sortDescription))
            {
                LookupResultsView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Descending));
                _lastSortDescriptionAscending = string.Empty;
            }
            else
            {
                LookupResultsView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Ascending));
                _lastSortDescriptionAscending = sortDescription;
            }
        }
        #endregion

        #region Events
        private void DNSLookup_RecordReceived(object sender, DNSLookupRecordArgs e)
        {
            var dnsLookupRecordInfo = DNSLookupRecordInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (LookupResults)
                    LookupResults.Add(dnsLookupRecordInfo);
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