using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Traceroute feature.
/// </summary>
public class TracerouteViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(TracerouteViewModel));

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    private string _host;

    /// <summary>
    /// Gets or sets the host address or hostname to trace.
    /// </summary>
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

    /// <summary>
    /// Gets the collection view for the host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether a traceroute is currently running.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the trace should be cancelled.
    /// </summary>
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

    private ObservableCollection<TracerouteHopInfo> _results = new();

    /// <summary>
    /// Gets or sets the collection of traceroute hop results.
    /// </summary>
    public ObservableCollection<TracerouteHopInfo> Results
    {
        get => _results;
        set
        {
            if (Equals(value, _results))
                return;

            _results = value;
        }
    }

    /// <summary>
    /// Gets the collection view for the traceroute results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    private TracerouteHopInfo _selectedResult;

    /// <summary>
    /// Gets or sets the currently selected traceroute result hop.
    /// </summary>
    public TracerouteHopInfo SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (value == _selectedResult)
                return;

            _selectedResult = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedResults = new ArrayList();

    /// <summary>
    /// Gets or sets the list of currently selected traceroute result hops (for multi-selection).
    /// </summary>
    public IList SelectedResults
    {
        get => _selectedResults;
        set
        {
            if (Equals(value, _selectedResults))
                return;

            _selectedResults = value;
            OnPropertyChanged();
        }
    }

    private bool _ipGeolocationRateLimitIsReached;

    private bool _isStatusMessageDisplayed;

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
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

    /// <summary>
    /// Gets the status message to display.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="TracerouteViewModel"/> class.
    /// </summary>
    /// <param name="tabId">The unique identifier for the tab.</param>
    /// <param name="host">The initial host to trace.</param>
    public TracerouteViewModel(Guid tabId, string host)
    {
        ConfigurationManager.Current.TracerouteTabCount++;

        _tabId = tabId;
        Host = host;

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Traceroute_HostHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(TracerouteHopInfo.Hop),
            ListSortDirection.Ascending));

        LoadSettings();
    }

    /// <summary>
    /// Called when the view is loaded. Starts the trace if it's the first load and a host is specified.
    /// </summary>
    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Host))
            StartTrace().ConfigureAwait(false);

        _firstLoad = false;
    }

    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to start or stop the traceroute.
    /// </summary>
    public ICommand TraceCommand => new RelayCommand(_ => TraceAction(), Trace_CanExecute);

    private bool Trace_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow)!.IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void TraceAction()
    {
        if (IsRunning)
            StopTrace();
        else
            StartTrace().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to redirect the selected result's data to another application.
    /// </summary>
    public ICommand RedirectDataToApplicationCommand => new RelayCommand(RedirectDataToApplicationAction);

    private void RedirectDataToApplicationAction(object name)
    {
        if (name is not ApplicationName applicationName)
            return;

        var host = !string.IsNullOrEmpty(SelectedResult.Hostname)
            ? SelectedResult.Hostname
            : SelectedResult.IPAddress.ToString();

        EventSystem.RedirectToApplication(applicationName, host);
    }

    /// <summary>
    /// Gets the command to perform a DNS lookup for the selected hop.
    /// </summary>
    public ICommand PerformDNSLookupCommand => new RelayCommand(PerformDNSLookupAction);

    private void PerformDNSLookupAction(object data)
    {
        EventSystem.RedirectToApplication(ApplicationName.DNSLookup, data.ToString());
    }

    /// <summary>
    /// Gets the command to copy the round-trip time to the clipboard.
    /// </summary>
    public ICommand CopyTimeToClipboardCommand => new RelayCommand(CopyTimeToClipboardAction);

    private void CopyTimeToClipboardAction(object timeIdentifier)
    {
        var time = timeIdentifier switch
        {
            "1" => Ping.TimeToString(SelectedResult.Status1, SelectedResult.Time1),
            "2" => Ping.TimeToString(SelectedResult.Status2, SelectedResult.Time2),
            "3" => Ping.TimeToString(SelectedResult.Status3, SelectedResult.Time3),
            _ => "-/-"
        };

        ClipboardHelper.SetClipboard(time);
    }

    /// <summary>
    /// Gets the command to export the traceroute results.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        Export().ConfigureAwait(false);
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
        _ipGeolocationRateLimitIsReached = false;
        StatusMessage = string.Empty;
        IsStatusMessageDisplayed = false;
        IsRunning = true;

        Results.Clear();

        DragablzTabItem.SetTabHeader(_tabId, Host);

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

    private Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : SelectedResults.Cast<TracerouteHopInfo>().ToArray());
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Traceroute_ExportFileType = instance.FileType;
            SettingsManager.Current.Traceroute_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.Traceroute_ExportFileType,
        SettingsManager.Current.Traceroute_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
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
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        // Stop trace
        if (IsRunning)
            StopTrace();

        ConfigurationManager.Current.TracerouteTabCount--;
    }

    #endregion

    #region Events

    private void Traceroute_HopReceived(object sender, TracerouteHopReceivedArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            // Check error
            if (e.Args.IPGeolocationResult.HasError)
            {
                Log.Error(
                    $"ip-api.com error: {e.Args.IPGeolocationResult.ErrorMessage}, error code: {e.Args.IPGeolocationResult.ErrorCode}");

                DisplayStatusMessage($"ip-api.com: {e.Args.IPGeolocationResult.ErrorMessage}");
            }

            // Check rate limit 
            if (!_ipGeolocationRateLimitIsReached && e.Args.IPGeolocationResult.RateLimitIsReached)
            {
                _ipGeolocationRateLimitIsReached = true;

                Log.Warn(
                    $"ip-api.com rate limit reached. Try again in {e.Args.IPGeolocationResult.RateLimitRemainingTime} seconds.");

                DisplayStatusMessage(
                    $"ip-api.com {string.Format(Strings.RateLimitReachedTryAgainInXSeconds, e.Args.IPGeolocationResult.RateLimitRemainingTime)}");
            }

            Results.Add(e.Args);
        }));
    }

    private void Traceroute_MaximumHopsReached(object sender, MaximumHopsReachedArgs e)
    {
        DisplayStatusMessage(string.Format(Strings.MaximumNumberOfHopsReached, e.Hops));
        IsRunning = false;
    }

    private void Traceroute_UserHasCanceled(object sender, EventArgs e)
    {
        DisplayStatusMessage(Strings.CanceledByUserMessage);
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