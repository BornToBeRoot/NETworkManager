using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Views;
using DnsClient;

namespace NETworkManager.ViewModels
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public readonly int TabId;
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

        private List<QueryType> _queryTypes = new List<QueryType>();
        public List<QueryType> QueryTypes
        {
            get => _queryTypes;
            set
            {
                if (value == _queryTypes)
                    return;

                _queryTypes = value;
                OnPropertyChanged();
            }
        }

        private QueryType _queryType;
        public QueryType QueryType
        {
            get => _queryType;
            set
            {
                if (value == _queryType)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_QueryType = value;

                _queryType = value;
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
                if (Equals(value, _lookupResults))
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

            TabId = tabId;
            Host = host;

            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_HostHistory);

            // Add default DNS server...
            if (SettingsManager.Current.DNSLookup_DNSServers.Count == 0)
                SettingsManager.Current.DNSLookup_DNSServers = new ObservableCollection<DNSServerInfo>(Models.Network.DNSServer.DefaultList());

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
            QueryTypes = SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes ? System.Enum.GetValues(typeof(QueryType)).Cast<QueryType>().Where(x => (x == QueryType.A || x == QueryType.AAAA || x == QueryType.ANY || x == QueryType.CNAME || x == QueryType.MX || x == QueryType.NS || x == QueryType.PTR || x == QueryType.SOA || x == QueryType.TXT)).OrderBy(x => x.ToString()).ToList() : System.Enum.GetValues(typeof(QueryType)).Cast<QueryType>().OrderBy(x => x.ToString()).ToList();
            QueryType = QueryTypes.FirstOrDefault(x => x == SettingsManager.Current.DNSLookup_QueryType);

            // Fallback
            if (QueryType == 0)
                QueryType = QueryType.ANY;
        }
        #endregion

        #region ICommands & Actions
        public ICommand LookupCommand => new RelayCommand(p => LookupAction(), Lookup_CanExecute);

        private bool Lookup_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void LookupAction()
        {
            if (!IsLookupRunning)
                StartLookup();
        }

        public ICommand CopySelectedDomainNameCommand => new RelayCommand(p => CopySelectedDomainNameAction());

        private void CopySelectedDomainNameAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.DomainName);
        }

        public ICommand CopySelectedTTLCommand => new RelayCommand(p => CopySelectedTTLAction());

        private void CopySelectedTTLAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.TTL.ToString());
        }

        public ICommand CopySelectedClassCommand => new RelayCommand(p => CopySelectedClassAction());

        private void CopySelectedClassAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.Class);
        }

        public ICommand CopySelectedTypeCommand => new RelayCommand(p => CopySelectedTypeAction());

        private void CopySelectedTypeAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.Type);
        }

        public ICommand CopySelectedResultCommand => new RelayCommand(p => CopySelectedResultAction());

        private void CopySelectedResultAction()
        {
            CommonMethods.SetClipboard(SelectedLookupResult.Result);
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.LanguageFiles.Strings.Export
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
                    settings.AffirmativeButtonText = Localization.LanguageFiles.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.LanguageFiles.Strings.Error, Localization.LanguageFiles.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
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
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == TabId).Header = Host;
                }
            }

            AddHostToHistory(Host);

            var dnsLookup = new DNSLookup
            {
                AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix,
                QueryClass = SettingsManager.Current.DNSLookup_QueryClass,
                QueryType = QueryType,
                Recursion = SettingsManager.Current.DNSLookup_Recursion,
                UseCache = SettingsManager.Current.DNSLookup_UseCache,
                UseTCPOnly = SettingsManager.Current.DNSLookup_UseTCPOnly,
                Retries = SettingsManager.Current.DNSLookup_Retries,
                Timeout = TimeSpan.FromSeconds(SettingsManager.Current.DNSLookup_Timeout),
            };

            if (!DNSServer.UseWindowsDNSServer)
            {
                dnsLookup.UseCustomDNSServer = true;
                dnsLookup.CustomDNSServer = DNSServer;                
            }

            if (SettingsManager.Current.DNSLookup_UseCustomDNSSuffix)
            {
                dnsLookup.UseCustomDNSSuffix = true;
                dnsLookup.CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix.TrimStart('.');
            }

            dnsLookup.RecordReceived += DNSLookup_RecordReceived;
            dnsLookup.LookupError += DNSLookup_LookupError;
            dnsLookup.LookupComplete += DNSLookup_LookupComplete;

            dnsLookup.ResolveAsync(Host.Split(';').Select(x => x.Trim()).ToList());
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
            
            StatusMessage += $"{e.DNSServer.Address}: {e.ErrorCode}";

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
                case nameof(SettingsInfo.DNSLookup_ShowOnlyMostCommonQueryTypes):
                    LoadTypes();
                    break;
            }
        }
        #endregion
    }
}