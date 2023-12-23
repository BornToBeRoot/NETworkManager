using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class ConnectionsViewModel : ViewModelBase
{
    #region Contructor, load settings

    public ConnectionsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        ((ListCollectionView)ResultsView).CustomSort = Comparer<ConnectionInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.LocalIPAddress, y.LocalIPAddress));

        ResultsView.Filter = o =>
        {
            if (o is not ConnectionInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            // Search by local/remote IP Address, local/remote Port, Protocol and State
            return info.LocalIPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.LocalPort.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.RemoteIPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.RemoteHostname.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.RemotePort.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Protocol.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   ResourceTranslator.Translate(ResourceIdentifier.TcpState, info.TcpState)
                       .IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.ProcessId.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.ProcessName.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.ProcessPath.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get connections
        Refresh().ConfigureAwait(false);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.Connections_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.Connections_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.Connections_AutoRefreshEnabled;

        _isLoading = false;
    }

    #endregion

    #region Events

    private async void AutoRefreshTimer_Tick(object sender, EventArgs e)
    {
        // Stop timer...
        _autoRefreshTimer.Stop();

        // Refresh
        await Refresh();

        // Restart timer...
        _autoRefreshTimer.Start();
    }

    #endregion

    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly bool _isLoading;
    private readonly DispatcherTimer _autoRefreshTimer = new();

    private string _search;

    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            ResultsView.Refresh();

            OnPropertyChanged();
        }
    }

    private ObservableCollection<ConnectionInfo> _results = new();

    public ObservableCollection<ConnectionInfo> Results
    {
        get => _results;
        set
        {
            if (value == _results)
                return;

            _results = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ResultsView { get; }

    private ConnectionInfo _selectedResult;

    public ConnectionInfo SelectedResult
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

    private bool _autoRefreshEnabled;

    public bool AutoRefreshEnabled
    {
        get => _autoRefreshEnabled;
        set
        {
            if (value == _autoRefreshEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.Connections_AutoRefreshEnabled = value;

            _autoRefreshEnabled = value;

            // Start timer to refresh automatically
            if (value)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(SelectedAutoRefreshTime);
                _autoRefreshTimer.Start();
            }
            else
            {
                _autoRefreshTimer.Stop();
            }

            OnPropertyChanged();
        }
    }

    public ICollectionView AutoRefreshTimes { get; }

    private AutoRefreshTimeInfo _selectedAutoRefreshTime;

    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get => _selectedAutoRefreshTime;
        set
        {
            if (value == _selectedAutoRefreshTime)
                return;

            if (!_isLoading)
                SettingsManager.Current.Connections_AutoRefreshTime = value;

            _selectedAutoRefreshTime = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (value == _isRefreshing)
                return;

            _isRefreshing = value;
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
        set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand RefreshCommand => new RelayCommand(_ => RefreshAction().ConfigureAwait(false), Refresh_CanExecute);

    private bool Refresh_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType,
                        instance.ExportAll
                            ? Results
                            : new ObservableCollection<ConnectionInfo>(SelectedResults.Cast<ConnectionInfo>()
                                .ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Connections_ExportFileType = instance.FileType;
                SettingsManager.Current.Connections_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new[]
            {
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            }, true, SettingsManager.Current.Connections_ExportFileType,
            SettingsManager.Current.Connections_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion

    #region Methods

    private async Task Refresh()
    {
        IsRefreshing = true;

        Results.Clear();

        (await Connection.GetActiveTcpConnectionsAsync()).ForEach(x => Results.Add(x));

        IsRefreshing = false;
    }

    public void OnViewVisible()
    {
        // Restart timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Start();
    }

    public void OnViewHide()
    {
        // Temporarily stop timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Stop();
    }

    #endregion
}