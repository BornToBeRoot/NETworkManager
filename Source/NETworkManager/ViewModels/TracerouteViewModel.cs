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
using System.Diagnostics;
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

namespace NETworkManager.ViewModels;

public class TracerouteViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly int _tabId;
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

    private ObservableCollection<TracerouteHopInfo> _traceResults = new();

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

    private bool _ipGeolocationIsRateLimitReached;

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

        _tabId = tabId;
        Host = host;

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Traceroute_HostHistory);

        // Result view
        TraceResultsView = CollectionViewSource.GetDefaultView(TraceResults);
        TraceResultsView.SortDescriptions.Add(new SortDescription(nameof(TracerouteHopInfo.Hop),
            ListSortDirection.Ascending));

        LoadSettings();

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

    public ICommand TraceCommand => new RelayCommand(_ => TraceAction(), Trace_CanExecute);

    private bool Trace_CanExecute(object parameter) => Application.Current.MainWindow != null &&
                                                       !((MetroWindow)Application.Current.MainWindow)!.IsAnyDialogOpen;

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
        if (name is not ApplicationName applicationName)
            return;

        var host = !string.IsNullOrEmpty(SelectedTraceResult.Hostname)
            ? SelectedTraceResult.Hostname
            : SelectedTraceResult.IPAddress.ToString();

        EventSystem.RedirectToApplication(applicationName, host);
    }

    public ICommand PerformDNSLookupCommand => new RelayCommand(PerformDNSLookupAction);

    private void PerformDNSLookupAction(object data)
    {
        EventSystem.RedirectToApplication(ApplicationName.DNSLookup, data.ToString());
    }

    public ICommand CopyDataToClipboardCommand => new RelayCommand(CopyDataToClipboardAction);

    private static void CopyDataToClipboardAction(object data)
    {
        ClipboardHelper.SetClipboard(data.ToString());
    }

    public ICommand CopyTimeToClipboardCommand => new RelayCommand(CopyTimeToClipboardAction);

    private void CopyTimeToClipboardAction(object timeIdentifier)
    {
        var time = timeIdentifier switch
        {
            "1" => Ping.TimeToString(SelectedTraceResult.Status1, SelectedTraceResult.Time1),
            "2" => Ping.TimeToString(SelectedTraceResult.Status2, SelectedTraceResult.Time2),
            "3" => Ping.TimeToString(SelectedTraceResult.Status3, SelectedTraceResult.Time3),
            _ => "-/-"
        };

        ClipboardHelper.SetClipboard(time);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

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
        _ipGeolocationIsRateLimitReached = false;
        StatusMessage = string.Empty;
        IsStatusMessageDisplayed = false;
        IsRunning = true;

        TraceResults.Clear();

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
        if (!IPAddress.TryParse(Host, out var ipAddress))
        {
            var dnsResult =
                await DNSClientHelper.ResolveAorAaaaAsync(Host,
                    SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

            if (dnsResult.HasError)
            {
                DisplayStatusMessage(DNSClientHelper.FormatDNSClientResultError(Host, dnsResult));

                IsRunning = false;

                return;
            }

            ipAddress = dnsResult.Value;
        }

        try
        {
            var traceroute = new Traceroute(new TracerouteOptions(
                SettingsManager.Current.Traceroute_Timeout,
                new byte[SettingsManager.Current.Traceroute_Buffer],
                SettingsManager.Current.Traceroute_MaximumHops,
                true,
                SettingsManager.Current.Traceroute_ResolveHostname,
                SettingsManager.Current.Traceroute_CheckIPApiIPGeolocation
            ));

            traceroute.HopReceived += Traceroute_HopReceived;
            traceroute.TraceComplete += Traceroute_TraceComplete;
            traceroute.MaximumHopsReached += Traceroute_MaximumHopsReached;
            traceroute.TraceError += Traceroute_TraceError;
            traceroute.UserHasCanceled += Traceroute_UserHasCanceled;

            traceroute.TraceAsync(ipAddress, _cancellationTokenSource.Token);

            // Add the host to history
            AddHostToHistory(Host);
        }
        catch (Exception ex) // This will catch any exception
        {
            IsRunning = false;

            DisplayStatusMessage(ex.Message);
        }
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
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        instance.ExportAll
                            ? TraceResults
                            : SelectedTraceResults.Cast<TracerouteHopInfo>().ToArray());
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error,
                        Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Traceroute_ExportFileType = instance.FileType;
                SettingsManager.Current.Traceroute_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            new[] { ExportFileType.CSV, ExportFileType.XML, ExportFileType.JSON }, true,
            SettingsManager.Current.Traceroute_ExportFileType, SettingsManager.Current.Traceroute_ExportFilePath
        );

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.Traceroute_HostHistory.ToList(), host,
            SettingsManager.Current.General_HistoryListEntries);

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
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            if (e.Args.IPGeolocationResult.HasError)
                DisplayStatusMessage($"ip-api.com: {e.Args.IPGeolocationResult.ErrorMessage}");

            if (!_ipGeolocationIsRateLimitReached && e.Args.IPGeolocationResult.IsRateLimitReached)
            {
                _ipGeolocationIsRateLimitReached = true;
                DisplayStatusMessage($"ip-api: {Localization.Resources.Strings.RateLimitReachedMessage}");
            }

            TraceResults.Add(e.Args);
        }));
    }

    private void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
    {
        DisplayStatusMessage(string.Format(Localization.Resources.Strings.MaximumNumberOfHopsReached, e.Hops));
        IsRunning = false;
    }

    private void Traceroute_UserHasCanceled(object sender, EventArgs e)
    {
        DisplayStatusMessage(Localization.Resources.Strings.CanceledByUserMessage);
        CancelTrace = false;
        IsRunning = false;
    }

    private void Traceroute_TraceError(object sender, TracerouteErrorArgs e)
    {
        DisplayStatusMessage(e.ErrorMessage);
        IsRunning = false;
    }

    private void Traceroute_TraceComplete(object sender, EventArgs e)
    {
        IsRunning = false;
    }

    private void DisplayStatusMessage(string message)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += message;
        IsStatusMessageDisplayed = true;
    }

    #endregion
}