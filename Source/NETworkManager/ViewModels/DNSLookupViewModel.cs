using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
using System.Threading.Tasks;

namespace NETworkManager.ViewModels;

public class DNSLookupViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly int _tabId;
    private bool _firstLoad = true;

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

    private DNSServerConnectionInfoProfile _dnsServer = new();

    public DNSServerConnectionInfoProfile DNSServer
    {
        get => _dnsServer;
        set
        {
            if (value == _dnsServer)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_SelectedDNSServer_v2 = value;

            _dnsServer = value;
            OnPropertyChanged();
        }
    }

    private List<QueryType> _queryTypes = new();

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

    private ObservableCollection<DNSLookupRecordInfo> _results = new();

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

    public ICollectionView ResultsView { get; }

    private DNSLookupRecordInfo _selectedResult;

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

    private IList _selectedResults = new ArrayList();

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

    public DNSLookupViewModel(IDialogCoordinator instance, int tabId, string host)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        _tabId = tabId;
        Host = host;

        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_HostHistory);

        DNSServers = new CollectionViewSource { Source = SettingsManager.Current.DNSLookup_DNSServers_v2 }.View;
        DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerConnectionInfoProfile.UseWindowsDNSServer),
            ListSortDirection.Descending));
        DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerConnectionInfoProfile.Name),
            ListSortDirection.Ascending));
        DNSServer = DNSServers.SourceCollection.Cast<DNSServerConnectionInfoProfile>()
                        .FirstOrDefault(x => x.Name == SettingsManager.Current.DNSLookup_SelectedDNSServer_v2.Name) ??
                    DNSServers.SourceCollection.Cast<DNSServerConnectionInfoProfile>().First();

        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DNSLookupRecordInfo.IPEndPoint)));
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(DNSLookupRecordInfo.IPEndPoint),
            ListSortDirection.Descending));

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
    }

    private void LoadTypes()
    {
        // Filter by common types...
        QueryTypes = SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes
            ? Enum.GetValues(typeof(QueryType)).Cast<QueryType>().Where(x =>
                x is QueryType.A or QueryType.AAAA or QueryType.ANY or QueryType.CNAME or QueryType.MX or QueryType.NS
                    or QueryType.PTR or QueryType.SOA or QueryType.TXT).OrderBy(x => x.ToString()).ToList()
            : Enum.GetValues(typeof(QueryType)).Cast<QueryType>().OrderBy(x => x.ToString()).ToList();
        QueryType = QueryTypes.FirstOrDefault(x => x == SettingsManager.Current.DNSLookup_QueryType);

        // Fallback
        if (QueryType == 0)
            QueryType = QueryType.ANY;
    }

    #endregion

    #region ICommands & Actions

    public ICommand LookupCommand => new RelayCommand(_ => LookupAction(), Lookup_CanExecute);

    private bool Lookup_CanExecute(object parameter) => Application.Current.MainWindow != null &&
                                                       !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private void LookupAction()
    {
        if (!IsRunning)
            StartLookup();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
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
                            ? Results
                            : new ObservableCollection<DNSLookupRecordInfo>(SelectedResults
                                .Cast<DNSLookupRecordInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error,
                        Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.DNSLookup_ExportFileType = instance.FileType;
                SettingsManager.Current.DNSLookup_ExportFilePath = instance.FilePath;
            }, _ =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            },
            new[]
            {
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            }, true,
            SettingsManager.Current.DNSLookup_ExportFileType, SettingsManager.Current.DNSLookup_ExportFilePath);

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
        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        IsRunning = true;

        // Reset the latest results
        Results.Clear();

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

        DNSLookupSettings dnsSettings = new()
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

        if (SettingsManager.Current.DNSLookup_UseCustomDNSSuffix)
        {
            dnsSettings.UseCustomDNSSuffix = true;
            dnsSettings.CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix?.TrimStart('.');
        }

        var dnsLookup = DNSServer.UseWindowsDNSServer ? new DNSLookup(dnsSettings) : new DNSLookup(dnsSettings, DNSServer.Servers);

        dnsLookup.RecordReceived += DNSLookup_RecordReceived;
        dnsLookup.LookupError += DNSLookup_LookupError;
        dnsLookup.LookupComplete += DNSLookup_LookupComplete;

        dnsLookup.ResolveAsync(Host.Split(';').Select(x => x.Trim()).ToList());
    }

    public void OnClose()
    {
    }

    // Modify history list
    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.DNSLookup_HostHistory.ToList(), host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.DNSLookup_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.DNSLookup_HostHistory.Add(x));
    }

    public void SortResultByPropertyName(string sortDescription)
    {
        ResultsView.SortDescriptions.Clear();
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(DNSLookupRecordInfo.Server),
            ListSortDirection.Descending));

        if (_lastSortDescriptionAscending.Equals(sortDescription))
        {
            ResultsView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Descending));
            _lastSortDescriptionAscending = string.Empty;
        }
        else
        {
            ResultsView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Ascending));
            _lastSortDescriptionAscending = sortDescription;
        }
    }

    #endregion

    #region Events

    private void DNSLookup_RecordReceived(object sender, DNSLookupRecordReceivedArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(delegate { Results.Add(e.Args); }));
    }

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

    private void DNSLookup_LookupComplete(object sender, EventArgs e)
    {
        IsRunning = false;
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.DNSLookup_ShowOnlyMostCommonQueryTypes):
                LoadTypes();
                break;
        }
    }
    #endregion
}