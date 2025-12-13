using DnsClient;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the DNS lookup view.
/// </summary>
public class DNSLookupViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(DNSLookupViewModel));

    /// <summary>
    /// The dialog coordinator instance.
    /// </summary>
    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Backing field for <see cref="Host"/>.
    /// </summary>
    private string _host;

    /// <summary>
    /// Gets or sets the host to lookup.
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
    /// Gets the collection view of host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    /// <summary>
    /// Gets the collection view of DNS servers.
    /// </summary>
    public ICollectionView DNSServers { get; }

    /// <summary>
    /// Backing field for <see cref="DNSServer"/>.
    /// </summary>
    private string _dnsServer;

    /// <summary>
    /// Gets or sets the selected DNS server.
    /// This can either be an ip/host:port or a profile name.
    /// </summary>
    public string DNSServer
    {
        get => _dnsServer;
        set
        {
            if (_dnsServer == value)
                return;

            // Try finding matching dns server profile by name, otherwise set to null (de-select)
            SelectedDNSServer = SettingsManager.Current.DNSLookup_DNSServers
                .FirstOrDefault(x => x.Name == value);

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_SelectedDNSServer_v2 = value;

            _dnsServer = value;
            OnPropertyChanged();
        }
    }

    private DNSServerConnectionInfoProfile _selectedDNSServer;
    public DNSServerConnectionInfoProfile SelectedDNSServer
    {
        get => _selectedDNSServer;
        set
        {
            if (_selectedDNSServer == value)
                return;

            _selectedDNSServer = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="QueryTypes"/>.
    /// </summary>
    private List<QueryType> _queryTypes = [];

    /// <summary>
    /// Gets the list of available query types.
    /// </summary>
    public List<QueryType> QueryTypes
    {
        get => _queryTypes;
        private set
        {
            if (value == _queryTypes)
                return;

            _queryTypes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="QueryType"/>.
    /// </summary>
    private QueryType _queryType;

    /// <summary>
    /// Gets or sets the selected query type.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="IsRunning"/>.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the lookup is running.
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

    /// <summary>
    /// Backing field for <see cref="Results"/>.
    /// </summary>
    private ObservableCollection<DNSLookupRecordInfo> _results = [];

    /// <summary>
    /// Gets or sets the collection of lookup results.
    /// </summary>
    public ObservableCollection<DNSLookupRecordInfo> Results
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
    /// Gets the collection view for lookup results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedResult"/>.
    /// </summary>
    private DNSLookupRecordInfo _selectedResult;

    /// <summary>
    /// Gets or sets the selected lookup result.
    /// </summary>
    public DNSLookupRecordInfo SelectedResult
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

    /// <summary>
    /// Backing field for <see cref="SelectedResults"/>.
    /// </summary>
    private IList _selectedResults = new ArrayList();

    /// <summary>
    /// Gets or sets the list of selected lookup results.
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

    /// <summary>
    /// Backing field for <see cref="IsStatusMessageDisplayed"/>.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="StatusMessage"/>.
    /// </summary>
    private string _statusMessage;

    /// <summary>
    /// Gets the status message.
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

    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="DNSLookupViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    /// <param name="tabId">The ID of the tab.</param>
    /// <param name="host">The host to lookup.</param>
    public DNSLookupViewModel(IDialogCoordinator instance, Guid tabId, string host)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        ConfigurationManager.Current.DNSLookupTabCount++;

        _tabId = tabId;
        Host = host;

        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_HostHistory);

        DNSServers = new CollectionViewSource { Source = SettingsManager.Current.DNSLookup_DNSServers }.View;
        DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerConnectionInfoProfile.UseWindowsDNSServer),
            ListSortDirection.Descending));
        DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerConnectionInfoProfile.Name),
            ListSortDirection.Ascending));

        DNSServer = string.IsNullOrEmpty(SettingsManager.Current.DNSLookup_SelectedDNSServer_v2)
            ? SettingsManager.Current.DNSLookup_DNSServers.FirstOrDefault()?.Name
            : SettingsManager.Current.DNSLookup_SelectedDNSServer_v2;

        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DNSLookupRecordInfo.NameServerAsString)));
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(DNSLookupRecordInfo.NameServerIPAddress),
            ListSortDirection.Descending));

        LoadSettings();

        // Detect if settings have changed...
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        _isLoading = false;
    }

    /// <summary>
    /// Called when the view is loaded.
    /// </summary>
    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Host))
            QueryAsync().ConfigureAwait(false);

        _firstLoad = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        LoadTypes();
    }

    /// <summary>
    /// Loads the query types.
    /// </summary>
    private void LoadTypes()
    {
        var queryTypes = (QueryType[])Enum.GetValues(typeof(QueryType));

        //if (SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes)
        //    QueryTypes = [.. queryTypes.Where(GlobalStaticConfiguration.DNSLookup_CustomQueryTypes.Contains).OrderBy(x => x.ToString())];
        //else
        //    QueryTypes = [.. queryTypes.OrderBy(x => x.ToString())];

        QueryTypes = [.. queryTypes.Where(DNSLookup.QueryTypes.Contains).OrderBy(x => x.ToString())];
        QueryType = QueryTypes.FirstOrDefault(x => x == SettingsManager.Current.DNSLookup_QueryType);

        // Fallback
        if (QueryType == 0)
            QueryType = GlobalStaticConfiguration.DNSLookup_QueryType;
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to start the query.
    /// </summary>
    public ICommand QueryCommand => new RelayCommand(_ => QueryAction(), Query_CanExecute);

    /// <summary>
    /// Checks if the query command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool Query_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to start the query.
    /// </summary>
    private void QueryAction()
    {
        if (!IsRunning)
            QueryAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to export the results.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    /// <summary>
    /// Action to export the results.
    /// </summary>
    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Performs the DNS query.
    /// </summary>
    private async Task QueryAsync()
    {
        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        IsRunning = true;

        // Reset the latest results
        Results.Clear();

        DragablzTabItem.SetTabHeader(_tabId, Host);

        AddHostToHistory(Host);

        DNSLookupSettings dnsSettings = new()
        {
            AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix,
            QueryClass = SettingsManager.Current.DNSLookup_QueryClass,
            QueryType = QueryType,
            Recursion = SettingsManager.Current.DNSLookup_Recursion,
            UseCache = SettingsManager.Current.DNSLookup_UseCache,
            UseTCPOnly = SettingsManager.Current.DNSLookup_UseTCPOnly,
            Retries = SettingsManager.Current.DNSLookup_Retries,
            Timeout = TimeSpan.FromSeconds(SettingsManager.Current.DNSLookup_Timeout)
        };

        if (SettingsManager.Current.DNSLookup_UseCustomDNSSuffix)
        {
            dnsSettings.UseCustomDNSSuffix = true;
            dnsSettings.CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix?.TrimStart('.');
        }

        // Try find existing dns server profile
        var dnsServerProfile = SettingsManager.Current.DNSLookup_DNSServers
            .FirstOrDefault(x => x.Name == DNSServer);

        DNSLookup dnsLookup = null;

        List<ServerConnectionInfo> dnsServersToResolve = [];

        // Use profile if found
        if (dnsServerProfile != null)
        {
            // Use windows dns server
            if (dnsServerProfile.UseWindowsDNSServer)
                dnsLookup = new DNSLookup(dnsSettings);
            // Use custom dns servers from profile
            else
                dnsServersToResolve = dnsServerProfile.Servers;
        }
        // Parse custom dns server from input
        else
        {
            foreach (var dnsServer in DNSServer.Split(';').Select(x => x.Trim()))
            {
                Debug.WriteLine("PARSE: " + dnsServer);

                // [IPv6]:Port
                if (dnsServer.StartsWith('[') && dnsServer.Contains("]:"))
                {
                    var endIndex = dnsServer.IndexOf("]:", StringComparison.Ordinal);

                    var ipPart = dnsServer[1..endIndex];
                    var portPart = dnsServer[(endIndex + 2)..];

                    Debug.WriteLine($"IPV6 with port detected. IP: {ipPart}, Port: {portPart}");

                    // Validate IPv6 and port
                    if (IPAddress.TryParse(ipPart, out IPAddress ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&
                        int.TryParse(portPart, out int port) && port > 0 && port <= 65535)
                    {
                        dnsServersToResolve.Add(new ServerConnectionInfo(ip.ToString(), port));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(StatusMessage))
                            StatusMessage += Environment.NewLine;

                        StatusMessage += $"{Strings.DNSServer}: Could not parse DNS server '{dnsServer}'";
                        IsStatusMessageDisplayed = true;

                        continue;
                    }
                }
                // [IPv6]
                else if (dnsServer.StartsWith('[') && dnsServer.EndsWith(']'))
                {
                    var ipPart = dnsServer[1..^1];

                    Debug.WriteLine($"IPV6 without port detected. IP: {ipPart}");

                    // Validate IPv6
                    if (IPAddress.TryParse(ipPart, out IPAddress ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        dnsServersToResolve.Add(new ServerConnectionInfo(ip.ToString(), 53));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(StatusMessage))
                            StatusMessage += Environment.NewLine;

                        StatusMessage += $"{Strings.DNSServer}: Could not parse DNS server '{dnsServer}'";
                        IsStatusMessageDisplayed = true;

                        continue;
                    }
                }
                // IPv6
                else if (dnsServer.Count(c => c == ':') > 1)
                {
                    Debug.WriteLine($"IPv6 without port detected. IP: {dnsServer}");

                    // Validate IPv6
                    if (IPAddress.TryParse(dnsServer, out IPAddress ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        dnsServersToResolve.Add(new ServerConnectionInfo(ip.ToString(), 53));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(StatusMessage))
                            StatusMessage += Environment.NewLine;

                        StatusMessage += $"{Strings.DNSServer}: Could not parse DNS server '{dnsServer}'";
                        IsStatusMessageDisplayed = true;

                        continue;
                    }
                }
                // IPv4/hostname:port
                else if (dnsServer.Contains(':'))
                {
                    var parts = dnsServer.Split([':'], 2);

                    Debug.WriteLine($"IPv4 or Hostname with port detected. Host: {parts[0]}, Port: {parts[1]}");

                    // Validate IPv4/Hostname and port
                    if ((RegexHelper.IPv4AddressRegex().IsMatch(parts[0]) || RegexHelper.HostnameOrDomainRegex().IsMatch(parts[0])) && int.TryParse(parts[1], out int port) && port > 0 && port <= 65535)
                    {
                        dnsServersToResolve.Add(new ServerConnectionInfo(parts[0], port));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(StatusMessage))
                            StatusMessage += Environment.NewLine;

                        StatusMessage += $"{Strings.DNSServer}: Could not parse DNS server '{dnsServer}'";
                        IsStatusMessageDisplayed = true;

                        continue;
                    }
                }
                // IPv4/hostname
                else
                {
                    Debug.WriteLine($"IPv4 or Hostname without port detected. Host: {dnsServer}");
                    // Validate IPv4/Hostname

                    if (RegexHelper.IPv4AddressRegex().IsMatch(dnsServer) || RegexHelper.HostnameOrDomainRegex().IsMatch(dnsServer))
                    {
                        dnsServersToResolve.Add(new ServerConnectionInfo(dnsServer, 53));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(StatusMessage))
                            StatusMessage += Environment.NewLine;

                        StatusMessage += $"{Strings.DNSServer}: Could not parse DNS server '{dnsServer}'";
                        IsStatusMessageDisplayed = true;

                        continue;
                    }
                }
            }
        }

        // DNS Lookup instance not created yet (not using Windows DNS server), need to resolve dns servers first
        if (dnsLookup == null)
        {
            List<ServerConnectionInfo> resolvedDNSServers = [];

            foreach (var dnsServerToResolve in dnsServersToResolve)
            {
                Debug.WriteLine("RESOLVE: " + dnsServerToResolve.Server);

                // Check if already an IP address
                if (IPAddress.TryParse(dnsServerToResolve.Server, out _))
                {
                    resolvedDNSServers.Add(dnsServerToResolve);
                }
                // Need to resolve hostname to IP address
                else
                {
                    var dnsResult = await DNSClientHelper.ResolveAorAaaaAsync(dnsServerToResolve.Server,
                        SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

                    if (dnsResult.HasError)
                    {
                        var dnsErrorMessage = DNSClientHelper.FormatDNSClientResultError(dnsServerToResolve.Server, dnsResult);

                        if (!string.IsNullOrEmpty(StatusMessage))
                            StatusMessage += Environment.NewLine;

                        StatusMessage += $"{Strings.DNSServer}: {dnsErrorMessage}";
                        IsStatusMessageDisplayed = true;

                        continue; // Skip this server, try next one
                    }

                    resolvedDNSServers.Add(new ServerConnectionInfo(
                        dnsResult.Value.ToString(),
                        dnsServerToResolve.Port,
                        dnsServerToResolve.TransportProtocol)
                    );
                }
            }

            // Create DNS lookup instance if we have any resolved dns servers
            if (resolvedDNSServers.Count > 0)
            {
                dnsLookup = new DNSLookup(dnsSettings, resolvedDNSServers);
            }
            // No valid dns servers, return with error
            else
            {
                if (!string.IsNullOrEmpty(StatusMessage))
                    StatusMessage += Environment.NewLine;

                StatusMessage += "Could not parse / resolve any of the specified DNS servers.";
                IsStatusMessageDisplayed = true;

                IsRunning = false;

                return;
            }
        }

        dnsLookup.RecordReceived += DNSLookup_RecordReceived;
        dnsLookup.LookupError += DNSLookup_LookupError;
        dnsLookup.LookupComplete += DNSLookup_LookupComplete;

        dnsLookup.ResolveAsync([.. Host.Split(';').Select(x => x.Trim())]);
    }

    /// <summary>
    /// Called when the view is closed.
    /// </summary>
    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        ConfigurationManager.Current.DNSLookupTabCount--;
    }

    // Modify history list
    /// <summary>
    /// Adds the host to the history.
    /// </summary>
    /// <param name="host">The host to add.</param>
    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify([.. SettingsManager.Current.DNSLookup_HostHistory], host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.DNSLookup_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.DNSLookup_HostHistory.Add(x));
    }


    /// <summary>
    /// Exports the results.
    /// </summary>
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
                        : new ObservableCollection<DNSLookupRecordInfo>(SelectedResults
                            .Cast<DNSLookupRecordInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(window, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.DNSLookup_ExportFileType = instance.FileType;
            SettingsManager.Current.DNSLookup_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], true,
            SettingsManager.Current.DNSLookup_ExportFileType, SettingsManager.Current.DNSLookup_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the RecordReceived event of the DNS lookup.
    /// </summary>
    private void DNSLookup_RecordReceived(object sender, DNSLookupRecordReceivedArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(delegate { Results.Add(e.Args); }));
    }

    /// <summary>
    /// Handles the LookupError event of the DNS lookup.
    /// </summary>
    private void DNSLookup_LookupError(object sender, DNSLookupErrorArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            // Build the message based on the information in the DNSLookupErrorArgs
            var statusMessage = $"{e.Query} @ {e.IPEndPoint} ==> {e.ErrorMessage}";

            // Show the message
            if (!IsStatusMessageDisplayed)
            {
                StatusMessage = statusMessage;
                IsStatusMessageDisplayed = true;

                return;
            }

            // Append the message
            StatusMessage += Environment.NewLine + statusMessage;
        }));
    }

    /// <summary>
    /// Handles the LookupComplete event of the DNS lookup.
    /// </summary>
    private void DNSLookup_LookupComplete(object sender, EventArgs e)
    {
        IsRunning = false;
    }

    /// <summary>
    /// Handles the PropertyChanged event of the SettingsManager.
    /// </summary>
    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            //case nameof(SettingsInfo.DNSLookup_ShowOnlyMostCommonQueryTypes):
            //    LoadTypes();
            //    break;
        }
    }

    #endregion
}