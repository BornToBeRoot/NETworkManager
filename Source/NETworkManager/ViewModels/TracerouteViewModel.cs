using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using NETworkManager.Models.Settings;
using NETworkManager.Models.Network;
using System.Threading;
using NETworkManager.Utilities;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Models.Export;
using NETworkManager.Views;
using NETworkManager.Models.EventSystem;

namespace NETworkManager.ViewModels
{
    public class TracerouteViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private CancellationTokenSource _cancellationTokenSource;

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

        private bool _isTraceRunning;
        public bool IsTraceRunning
        {
            get => _isTraceRunning;
            set
            {
                if (value == _isTraceRunning)
                    return;

                _isTraceRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelTrace;
        public bool CancelTrace
        {
            get => _cancelTrace;
            set
            {
                if (value == _cancelTrace)
                    return;

                _cancelTrace = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TracerouteHopInfo> _traceResults = new ObservableCollection<TracerouteHopInfo>();
        public ObservableCollection<TracerouteHopInfo> TraceResults
        {
            get => _traceResults;
            set
            {
                if (Equals(value, _traceResults))
                    return;

                _traceResults = value;
            }
        }

        public ICollectionView TraceResultsView { get; }

        private TracerouteHopInfo _selectedTraceResult;
        public TracerouteHopInfo SelectedTraceResult
        {
            get => _selectedTraceResult;
            set
            {
                if (value == _selectedTraceResult)
                    return;

                _selectedTraceResult = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedTraceResults = new ArrayList();
        public IList SelectedTraceResults
        {
            get => _selectedTraceResults;
            set
            {
                if (Equals(value, _selectedTraceResults))
                    return;

                _selectedTraceResults = value;
                OnPropertyChanged();
            }
        }

        public bool ResolveHostname => SettingsManager.Current.Traceroute_ResolveHostname;

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
                    SettingsManager.Current.Traceroute_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        private int _hops;
        public int Hops
        {
            get => _hops;
            set
            {
                if (value == _hops)
                    return;

                _hops = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatistics => SettingsManager.Current.Traceroute_ShowStatistics;

        #endregion

        #region Constructor, load settings
        public TracerouteViewModel(IDialogCoordinator instance, int tabId, string host)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            _tabId = tabId;
            Host = host;

            // Set collection view
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Traceroute_HostHistory);

            // Result view
            TraceResultsView = CollectionViewSource.GetDefaultView(TraceResults);
            TraceResultsView.SortDescriptions.Add(new SortDescription(nameof(TracerouteHopInfo.Hop), ListSortDirection.Ascending));

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        } 

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Host))
                StartTrace();

            _firstLoad = false;
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.Traceroute_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand TraceCommand => new RelayCommand(p => TraceAction(), Trace_CanExecute);

        private bool Trace_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void TraceAction()
        {
            if (IsTraceRunning)
                StopTrace();
            else
                StartTrace();
        }

        public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

        private void RedirectDataToApplicationAction(object name)
        {
            if (!(name is string appName))
                return;

            if (!System.Enum.TryParse(appName, out ApplicationViewManager.Name app))
                return;

            var host = !string.IsNullOrEmpty(SelectedTraceResult.Hostname) ? SelectedTraceResult.Hostname : SelectedTraceResult.IPAddress.ToString();

            EventSystem.RedirectDataToApplication(app, host);
        }

        public ICommand PerformDNSLookupIPAddressCommand => new RelayCommand(p => PerformDNSLookupIPAddressAction());

        private void PerformDNSLookupIPAddressAction()
        {
            EventSystem.RedirectDataToApplication(ApplicationViewManager.Name.DNSLookup, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupHostnameCommand => new RelayCommand(p => PerformDNSLookupHostnameAction());

        private void PerformDNSLookupHostnameAction()
        {
            EventSystem.RedirectDataToApplication(ApplicationViewManager.Name.DNSLookup, SelectedTraceResult.Hostname);
        }

        public ICommand CopySelectedHopCommand => new RelayCommand(p => CopySelectedHopAction());

        private void CopySelectedHopAction()
        {
            CommonMethods.SetClipboard(SelectedTraceResult.Hop.ToString());
        }

        public ICommand CopySelectedTime1Command => new RelayCommand(p => CopySelectedTime1Action());

        private void CopySelectedTime1Action()
        {
            CommonMethods.SetClipboard(SelectedTraceResult.Time1.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedTime2Command => new RelayCommand(p => CopySelectedTime2Action());

        private void CopySelectedTime2Action()
        {
            CommonMethods.SetClipboard(SelectedTraceResult.Time2.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedTime3Command => new RelayCommand(p => CopySelectedTime3Action());

        private void CopySelectedTime3Action()
        {
            CommonMethods.SetClipboard(SelectedTraceResult.Time3.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedIPAddressCommand => new RelayCommand(p => CopySelectedIPAddressAction());

        private void CopySelectedIPAddressAction()
        {
            CommonMethods.SetClipboard(SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand => new RelayCommand(p => CopySelectedHostnameAction());

        private void CopySelectedHostnameAction()
        {
            CommonMethods.SetClipboard(SelectedTraceResult.Hostname);
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private void ExportAction()
        {
            Export();
        }
        #endregion

        #region Methods
        private void StopTrace()
        {
            CancelTrace = true;
            _cancellationTokenSource.Cancel();
        }

        private async void StartTrace()
        {
            DisplayStatusMessage = false;
            IsTraceRunning = true;

            // Measure the time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            TraceResults.Clear();
            Hops = 0;

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = Host;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            // Try to parse the string into an IP-Address
            IPAddress.TryParse(Host, out var ipAddress);

            try
            {
                // Try to resolve the hostname
                if (ipAddress == null)
                {
                    var ipHostEntries = await Dns.GetHostEntryAsync(Host);

                    foreach (var ipAddr in ipHostEntries.AddressList)
                    {
                        switch (ipAddr.AddressFamily)
                        {
                            case AddressFamily.InterNetwork when SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4:
                                ipAddress = ipAddr;
                                break;
                            case AddressFamily.InterNetworkV6 when SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4:
                                ipAddress = ipAddr;
                                break;
                        }
                    }

                    // Fallback --> If we could not resolve our prefered ip protocol
                    if (ipAddress == null)
                    {
                        foreach (var ip in ipHostEntries.AddressList)
                        {
                            ipAddress = ip;
                            break;
                        }
                    }
                }

                var traceroute = new Traceroute
                {
                    Timeout = SettingsManager.Current.Traceroute_Timeout,
                    Buffer = new byte[SettingsManager.Current.Traceroute_Buffer],
                    MaximumHops = SettingsManager.Current.Traceroute_MaximumHops,
                    DontFragement = true,
                    ResolveHostname = SettingsManager.Current.Traceroute_ResolveHostname
                };

                traceroute.HopReceived += Traceroute_HopReceived;
                traceroute.TraceComplete += Traceroute_TraceComplete;
                traceroute.MaximumHopsReached += Traceroute_MaximumHopsReached;
                traceroute.UserHasCanceled += Traceroute_UserHasCanceled;

                traceroute.TraceAsync(ipAddress, _cancellationTokenSource.Token);

                // Add the host to history
                AddHostToHistory(Host);
            }
            catch (SocketException) // This will catch DNS resolve errors
            {
                TracerouteFinished();

                StatusMessage = string.Format( Resources.Localization.Strings.CouldNotResolveHostnameFor, Host);
                DisplayStatusMessage = true;
            }
            catch (Exception ex) // This will catch any exception
            {
                TracerouteFinished();

                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
        }

        private void TracerouteFinished()
        {
            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();

            CancelTrace = false;
            IsTraceRunning = false;
        }

        private async void Export()
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
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? TraceResults : new ObservableCollection<TracerouteHopInfo>(SelectedTraceResults.Cast<TracerouteHopInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, Resources.Localization.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Traceroute_ExportFileType = instance.FileType;
                SettingsManager.Current.Traceroute_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, SettingsManager.Current.Traceroute_ExportFileType, SettingsManager.Current.Traceroute_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.Traceroute_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Traceroute_HostHistory.Clear();
            OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Traceroute_HostHistory.Add(x));
        }

        public void OnClose()
        {
            if (IsTraceRunning)
                StopTrace();
        }
        #endregion

        #region Events
        private void Traceroute_HopReceived(object sender, TracerouteHopReceivedArgs e)
        {
            var tracerouteInfo = TracerouteHopInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (TraceResults)
                    TraceResults.Add(tracerouteInfo);
            }));

            Hops++;
        }

        private void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
        {
            TracerouteFinished();

            StatusMessage = string.Format(Resources.Localization.Strings.MaximumNumberOfHopsReached, e.Hops);
            DisplayStatusMessage = true;
        }

        private void Traceroute_UserHasCanceled(object sender, EventArgs e)
        {
            TracerouteFinished();

            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;
        }

        private void Traceroute_TraceComplete(object sender, EventArgs e)
        {
            TracerouteFinished();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.Traceroute_ResolveHostname):
                    OnPropertyChanged(nameof(ResolveHostname));
                    break;
                case nameof(SettingsInfo.Traceroute_ShowStatistics):
                    OnPropertyChanged(nameof(ShowStatistics));
                    break;
            }
        }
        #endregion               
    }
}