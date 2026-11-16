using log4net;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Utilities.WPF;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

public sealed class MDNSDiscoveryViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(MDNSDiscoveryViewModel));
    private readonly DispatcherTimer _flushTimer;
    private readonly SearchDelay _searchDelay;
    private readonly Dictionary<string, MDNSServiceInfo> _rows = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MDNSRecordInfo> _recordRows = new(StringComparer.Ordinal);
    private MDNSDiscoverySnapshot _pending;
    private CancellationTokenSource _cancellation;
    private Task _browseTask = Task.CompletedTask;

    public MDNSDiscoveryViewModel()
    {
        ServiceType = SettingsManager.Current.MDNSDiscovery_ServiceType;
        if (!MDNSServiceType.TryNormalize(ServiceType, out _))
            ServiceType = GlobalStaticConfiguration.MDNSDiscovery_ServiceType;
        Duration = Math.Clamp(SettingsManager.Current.MDNSDiscovery_Duration, 0, 3600);
        _searchDelay = new SearchDelay(GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan, () =>
        {
            ResultsView.Refresh();
            RecordsView.Refresh();
        });
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(MDNSServiceInfo.ServiceType), ListSortDirection.Ascending));
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(MDNSServiceInfo.Name), ListSortDirection.Ascending));
        ResultsView.Filter = value => value is MDNSServiceInfo item && (string.IsNullOrWhiteSpace(Search) ||
            new[] { item.Name, item.ServiceType, item.Hostname, item.Addresses, item.Port?.ToString(), item.TXTRecords }
                .Any(x => x?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true));
        RecordsView = CollectionViewSource.GetDefaultView(Records);
        RecordsView.SortDescriptions.Add(new SortDescription(nameof(MDNSRecordInfo.Name), ListSortDirection.Ascending));
        RecordsView.SortDescriptions.Add(new SortDescription(nameof(MDNSRecordInfo.Type), ListSortDirection.Ascending));
        RecordsView.Filter = value => value is MDNSRecordInfo item && (string.IsNullOrWhiteSpace(Search) ||
            new[] { item.Name, item.Type, item.Value }.Any(x => x.Contains(Search, StringComparison.OrdinalIgnoreCase)));
        _flushTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _flushTimer.Tick += (_, _) => FlushResults();
        RefreshInterfaces();
    }

    public ObservableCollection<MDNSInterfaceInfo> Interfaces { get; } = new();
    public MDNSInterfaceInfo SelectedInterface
    {
        get;
        set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public IReadOnlyList<string> ServiceTypes { get; } = ["*", .. MDNSServiceCache.CommonServiceTypes];
    public string ServiceType
    {
        get;
        set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public int Duration
    {
        get;
        set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public bool IsRunning
    {
        get;
        private set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BrowseButtonText));
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool IsCanceling
    {
        get;
        private set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public string BrowseButtonText => IsRunning ? Strings.Stop : Strings.MDNSDiscovery_Browse;
    public string StatusMessage
    {
        get;
        private set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public StatusLevel StatusMessageLevel
    {
        get;
        private set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public string Search
    {
        get;
        set { if (value == field) return; field = value; OnPropertyChanged(); _searchDelay?.Restart(); }
    }
    public ObservableCollection<MDNSServiceInfo> Results { get; } = new();
    public ICollectionView ResultsView { get; }
    public ObservableCollection<MDNSRecordInfo> Records { get; } = new();
    public ICollectionView RecordsView { get; }
    public IList SelectedRecords { get; set; } = new ArrayList();
    public int SelectedTabIndex
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayedResultCount));
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public int DisplayedResultCount => SelectedTabIndex == 0 ? Results.Count : Records.Count;
    public MDNSServiceInfo SelectedResult
    {
        get;
        set { if (value == field) return; field = value; OnPropertyChanged(); }
    }
    public IList SelectedResults { get; set; } = new ArrayList();

    public ICommand ApplySearchCommand => _searchDelay.ApplyCommand;
    public ICommand RefreshInterfacesCommand => new RelayCommand(_ => RefreshInterfaces(), _ => !IsRunning && !DialogHelper.IsAnyDialogOpen());
    public ICommand BrowseCommand => new RelayCommand(_ =>
    {
        if (IsRunning) Stop();
        else _browseTask = BrowseAsync();
    }, parameter => !IsCanceling && !DialogHelper.IsAnyDialogOpen() &&
        (IsRunning || SelectedInterface != null && Duration is >= 0 and <= 3600 && MDNSServiceType.TryNormalize(ServiceType, out _)));

    private void RefreshInterfaces()
    {
        var id = SelectedInterface?.Id ?? SettingsManager.Current.MDNSDiscovery_InterfaceId;
        try
        {
            var links = MDNSBrowser.GetInterfaces();
            Interfaces.Clear();
            foreach (var link in links)
                Interfaces.Add(link);
            SelectedInterface = Interfaces.FirstOrDefault(x => x.Id == id) ?? Interfaces.FirstOrDefault();
            if (SelectedInterface == null)
                SetStatus(Strings.MDNSDiscovery_NoInterface, StatusLevel.Warning);
        }
        catch (Exception exception)
        {
            Log.Error("Cannot enumerate multicast interfaces.", exception);
            SetStatus(Strings.MDNSDiscovery_Failed + Environment.NewLine + exception.Message, StatusLevel.Error);
        }
    }

    private async Task BrowseAsync()
    {
        if (IsRunning || SelectedInterface == null || !MDNSServiceType.TryNormalize(ServiceType, out var type))
            return;
        _cancellation?.Dispose();
        using var timeout = new CancellationTokenSource();
        if (Duration > 0)
            timeout.CancelAfter(TimeSpan.FromSeconds(Duration));
        _cancellation = new CancellationTokenSource();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token, timeout.Token);
        var link = SelectedInterface;
        SettingsManager.Current.MDNSDiscovery_ServiceType = ServiceType;
        SettingsManager.Current.MDNSDiscovery_Duration = Duration;
        SettingsManager.Current.MDNSDiscovery_InterfaceId = link.Id;
        Results.Clear();
        Records.Clear();
        _rows.Clear();
        _recordRows.Clear();
        OnPropertyChanged(nameof(DisplayedResultCount));
        Interlocked.Exchange(ref _pending, null);
        IsRunning = true;
        IsCanceling = false;
        SetStatus(Strings.MDNSDiscovery_Browsing, StatusLevel.None);
        _flushTimer.Start();
        try
        {
            await Task.Run(() => MDNSBrowser.BrowseAsync(link, type, values => Interlocked.Exchange(ref _pending, values), linked.Token));
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
            SetStatus(_cancellation.IsCancellationRequested ? Strings.MDNSDiscovery_Stopped : Strings.MDNSDiscovery_Completed, StatusLevel.None);
        }
        catch (Exception exception)
        {
            Log.Error("mDNS service discovery failed.", exception);
            SetStatus(Strings.MDNSDiscovery_Failed + Environment.NewLine + exception.Message, StatusLevel.Error);
        }
        finally
        {
            FlushResults();
            _flushTimer.Stop();
            IsCanceling = false;
            IsRunning = false;
            _cancellation.Dispose();
            _cancellation = null;
        }
    }

    private void FlushResults()
    {
        var values = Interlocked.Exchange(ref _pending, null);
        if (values == null) return;
        var names = values.Services.Select(x => x.FullName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var name in _rows.Keys.Where(x => !names.Contains(x)).ToArray())
        {
            Results.Remove(_rows[name]);
            _rows.Remove(name);
        }
        foreach (var value in values.Services)
            if (_rows.TryGetValue(value.FullName, out var row))
                row.UpdateFrom(value);
            else
            {
                _rows.Add(value.FullName, value);
                Results.Add(value);
            }
        var recordIds = values.Records.Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var id in _recordRows.Keys.Where(x => !recordIds.Contains(x)).ToArray())
        {
            Records.Remove(_recordRows[id]);
            _recordRows.Remove(id);
        }
        foreach (var value in values.Records)
            if (_recordRows.TryGetValue(value.Id, out var row))
                row.UpdateFrom(value);
            else
            {
                _recordRows.Add(value.Id, value);
                Records.Add(value);
            }
        OnPropertyChanged(nameof(DisplayedResultCount));
        // A late TXT/address record can make an existing row match the current search.
        if (!string.IsNullOrWhiteSpace(Search)) ResultsView.Refresh();
    }

    private void SetStatus(string text, StatusLevel level)
    {
        StatusMessageLevel = level;
        StatusMessage = text;
    }

    private void Stop()
    {
        if (!IsRunning) return;
        IsCanceling = true;
        _cancellation?.Cancel();
    }

    public void OnViewHide() => Stop();
    public void OnViewVisible() { if (!IsRunning) RefreshInterfaces(); }

    public async Task OpenAsync(ApplicationRequest request)
    {
        if (string.IsNullOrEmpty(request.Target)) return;
        Stop();
        await _browseTask;
        if (!MDNSServiceType.TryNormalize(request.Target, out _)) return;
        ServiceType = request.Target;
        if (request.Start) await (_browseTask = BrowseAsync());
    }

    public ICommand CopySelectionCommand => new RelayCommand(_ => ClipboardHelper.SetClipboard(SelectedTabIndex == 0
        ? string.Join(Environment.NewLine + Environment.NewLine, SelectedResults.Cast<MDNSServiceInfo>()
            .Select(x => $"{x.FullName}\n{x.Hostname}\t{x.Port}\t{x.Addresses}\n{x.TXTRecords}"))
        : string.Join(Environment.NewLine, SelectedRecords.Cast<MDNSRecordInfo>()
            .Select(x => $"{x.Name}\t{x.Type}\t{x.Class}\t{x.TTL}\t{x.Value}"))),
        _ => (SelectedTabIndex == 0 ? SelectedResults.Count : SelectedRecords.Count) > 0);

    public ICommand CopyPortCommand => new RelayCommand(_ => ClipboardHelper.SetClipboard(SelectedResult.Port.Value.ToString(CultureInfo.InvariantCulture)),
        _ => SelectedResult?.Port.HasValue == true);

    public ICommand ExportCommand => new RelayCommand(_ => { _ = ExportAsync(); },
        _ => DisplayedResultCount > 0 && !DialogHelper.IsAnyDialogOpen());

    private Task ExportAsync()
    {
        var exportRecords = SelectedTabIndex != 0;
        var window = new ExportChildWindow { Title = Strings.Export };
        window.DataContext = new ExportViewModel(async instance =>
        {
            window.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
            try
            {
                // Detach the rows before background export; incoming advertisements can still update the grid.
                if (exportRecords)
                {
                    var records = RecordsView.GetItemsInViewOrder<MDNSRecordInfo>(instance.ExportAll, SelectedRecords)
                        .Select(x => new MDNSRecordInfo(x.Id, x.Name, x.Type, x.Value, x.NetworkInterface, x.CacheFlush, x.TTL)).ToArray();
                    await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, records));
                }
                else
                {
                    var values = ResultsView.GetItemsInViewOrder<MDNSServiceInfo>(instance.ExportAll, SelectedResults)
                        .Select(x => new MDNSServiceInfo(x.FullName, x.Name, x.ServiceType, x.NetworkInterface, x.Hostname,
                            x.Port, x.Priority, x.Weight, x.Addresses, x.TXTRecords, x.TTL)).ToArray();
                    await Task.Run(() => ExportManager.Export(instance.FilePath, instance.FileType, values));
                }
                SettingsManager.Current.MDNSDiscovery_ExportFilePath = instance.FilePath;
                SettingsManager.Current.MDNSDiscovery_ExportFileType = instance.FileType;
            }
            catch (Exception exception)
            {
                Log.Error("Cannot export mDNS services.", exception);
                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + exception.Message, ChildWindowIcon.Error);
            }
        }, _ =>
        {
            window.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json], true,
            SettingsManager.Current.MDNSDiscovery_ExportFileType, SettingsManager.Current.MDNSDiscovery_ExportFilePath);
        ConfigurationManager.Current.IsChildWindowOpen = true;
        return Application.Current.MainWindow.ShowChildWindowAsync(window);
    }
}
