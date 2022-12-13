using System.Net;
using System.Windows.Input;
using System.Windows;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using NETworkManager.Settings;
using NETworkManager.Models.Network;
using System.Threading;
using NETworkManager.Utilities;
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
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using System.Threading.Tasks;
using Amazon.EC2.Model;

namespace NETworkManager.ViewModels
{
    public class TracerouteViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private CancellationTokenSource _cancellationTokenSource;

        public readonly int TabId;
        private bool _firstLoad = true;

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

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value == _isRunning)
                    return;

                _isRunning = value;
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

        private bool _isStatusMessageDisplayed;
        public bool IsStatusMessageDisplayed
        {
            get => _isStatusMessageDisplayed;
            set
            {
                if (value == _isStatusMessageDisplayed)
                    return;

                _isStatusMessageDisplayed = value;
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
        #endregion

        #region Constructor, load settings
        public TracerouteViewModel(IDialogCoordinator instance, int tabId, string host)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            TabId = tabId;
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

        }
        #endregion

        #region ICommands & Actions
        public ICommand TraceCommand => new RelayCommand(p => TraceAction(), Trace_CanExecute);

        private bool Trace_CanExecute(object paramter) => System.Windows.Application.Current.MainWindow != null && !((MetroWindow)System.Windows.Application.Current.MainWindow).IsAnyDialogOpen;

        private void TraceAction()
        {
            if (IsRunning)
                StopTrace();
            else
                StartTrace();
        }

        public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

        private void RedirectDataToApplicationAction(object name)
        {
            if (name is not string appName)
                return;

            if (!Enum.TryParse(appName, out ApplicationName app))
                return;

            var host = !string.IsNullOrEmpty(SelectedTraceResult.Hostname) ? SelectedTraceResult.Hostname : SelectedTraceResult.IPAddress.ToString();

            EventSystem.RedirectToApplication(app, host);
        }

        public ICommand PerformDNSLookupIPAddressCommand => new RelayCommand(p => PerformDNSLookupIPAddressAction());

        private void PerformDNSLookupIPAddressAction()
        {
            EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand PerformDNSLookupHostnameCommand => new RelayCommand(p => PerformDNSLookupHostnameAction());

        private void PerformDNSLookupHostnameAction()
        {
            EventSystem.RedirectToApplication(ApplicationName.DNSLookup, SelectedTraceResult.Hostname);
        }

        public ICommand CopySelectedHopCommand => new RelayCommand(p => CopySelectedHopAction());

        private void CopySelectedHopAction()
        {
            ClipboardHelper.SetClipboard(SelectedTraceResult.Hop.ToString());
        }

        public ICommand CopySelectedTime1Command => new RelayCommand(p => CopySelectedTime1Action());

        private void CopySelectedTime1Action()
        {
            ClipboardHelper.SetClipboard(SelectedTraceResult.Time1.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedTime2Command => new RelayCommand(p => CopySelectedTime2Action());

        private void CopySelectedTime2Action()
        {
            ClipboardHelper.SetClipboard(SelectedTraceResult.Time2.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedTime3Command => new RelayCommand(p => CopySelectedTime3Action());

        private void CopySelectedTime3Action()
        {
            ClipboardHelper.SetClipboard(SelectedTraceResult.Time3.ToString(CultureInfo.CurrentCulture));
        }

        public ICommand CopySelectedIPAddressCommand => new RelayCommand(p => CopySelectedIPAddressAction());

        private void CopySelectedIPAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedTraceResult.IPAddress.ToString());
        }

        public ICommand CopySelectedHostnameCommand => new RelayCommand(p => CopySelectedHostnameAction());

        private void CopySelectedHostnameAction()
        {
            ClipboardHelper.SetClipboard(SelectedTraceResult.Hostname);
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

        private async Task StartTrace()
        {
            IsStatusMessageDisplayed = false;
            IsRunning = true;

            TraceResults.Clear();

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == TabId).Header = Host;
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            // Try to parse the string into an IP-Address
            if (!IPAddress.TryParse(Host, out var ipAddress))
            {
                var dnsResult = await DNSHelper.ResolveAorAaaaAsync(Host, SettingsManager.Current.Traceroute_ResolveHostnamePreferIPv4);

                if (!dnsResult.HasError)
                {
                    ipAddress = dnsResult.Value;
                }
                else
                {
                    StatusMessage = string.Format(Localization.Resources.Strings.CouldNotResolveIPAddressFor, Host) + Environment.NewLine + dnsResult.ErrorMessage;
                    IsStatusMessageDisplayed = true;
                    IsRunning = false;
                    return;
                }
            }

            try
            {
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
            catch (Exception ex) // This will catch any exception
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
                IsRunning = false;
            }
        }

        private void UserHasCanceled()
        {
            CancelTrace = false;
            IsRunning = false;
        }

        private async Task Export()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Export
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
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Traceroute_ExportFileType = instance.FileType;
                SettingsManager.Current.Traceroute_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.Traceroute_ExportFileType, SettingsManager.Current.Traceroute_ExportFilePath);

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
            if (IsRunning)
                StopTrace();
        }
        #endregion

        #region Events
        private void Traceroute_HopReceived(object sender, TracerouteHopReceivedArgs e)
        {
            var tracerouteInfo = TracerouteHopInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                //lock (TraceResults)
                TraceResults.Add(tracerouteInfo);
            }));
        }

        private void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
        {
            StatusMessage = string.Format(Localization.Resources.Strings.MaximumNumberOfHopsReached, e.Hops);
            IsStatusMessageDisplayed = true;
            IsRunning = false;
        }

        private void Traceroute_UserHasCanceled(object sender, EventArgs e)
        {
            UserHasCanceled();

            StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
            IsStatusMessageDisplayed = true;
        }

        private void Traceroute_TraceComplete(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.Traceroute_ResolveHostname):
                    OnPropertyChanged(nameof(ResolveHostname));
                    break;
            }
        }
        #endregion               
    }
}