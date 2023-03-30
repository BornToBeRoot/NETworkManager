using NETworkManager.Models.Network;
using System;
using System.Collections;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using NETworkManager.Utilities;
using NETworkManager.Settings;
using System.Windows.Threading;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Views;
using System.Threading.Tasks;
using NETworkManager.Localization;
using NETworkManager.Localization.Translators;

namespace NETworkManager.ViewModels;

public class ConnectionsViewModel : ViewModelBase
{
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

            ConnectionResultsView.Refresh();

            OnPropertyChanged();
        }
    }

    private ObservableCollection<ConnectionInfo> _connectionResults = new();
    public ObservableCollection<ConnectionInfo> ConnectionResults
    {
        get => _connectionResults;
        set
        {
            if (value == _connectionResults)
                return;

            _connectionResults = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ConnectionResultsView { get; }

    private ConnectionInfo _selectedConnectionInfo;
    public ConnectionInfo SelectedConnectionInfo
    {
        get => _selectedConnectionInfo;
        set
        {
            if (value == _selectedConnectionInfo)
                return;

            _selectedConnectionInfo = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedConnectionInfos = new ArrayList();
    public IList SelectedConnectionInfos
    {
        get => _selectedConnectionInfos;
        set
        {
            if (Equals(value, _selectedConnectionInfos))
                return;

            _selectedConnectionInfos = value;
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

    #region Contructor, load settings
    public ConnectionsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        // Result view + search
        ConnectionResultsView = CollectionViewSource.GetDefaultView(ConnectionResults);
        ConnectionResultsView.SortDescriptions.Add(new SortDescription(nameof(ConnectionInfo.LocalIPAddressInt32), ListSortDirection.Ascending));
        ConnectionResultsView.Filter = o =>
        {
            if (o is not ConnectionInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            // Search by local/remote IP Address, local/remote Port, Protocol and State
            return info.LocalIPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || info.LocalPort.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteIPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || info.RemotePort.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || info.Protocol.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || TcpStateTranslator.GetInstance().Translate(info.TcpState).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get connections
        Refresh();

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x => (x.Value == SettingsManager.Current.Connections_AutoRefreshTime.Value && x.TimeUnit == SettingsManager.Current.Connections_AutoRefreshTime.TimeUnit));
        AutoRefreshEnabled = SettingsManager.Current.Connections_AutoRefreshEnabled;
        
        _isLoading = false;
    }
    #endregion

    #region ICommands & Actions
    public ICommand RefreshCommand => new RelayCommand(p => RefreshAction(), Refresh_CanExecute);

    private bool Refresh_CanExecute(object paramter)
    {
        return Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
    }

    public ICommand CopySelectedLocalIpAddressCommand => new RelayCommand(p => CopySelectedLocalIpAddressAction());

    private void CopySelectedLocalIpAddressAction()
    {
        ClipboardHelper.SetClipboard(SelectedConnectionInfo.LocalIPAddress.ToString());
    }

    public ICommand CopySelectedLocalPortCommand => new RelayCommand(p => CopySelectedLocalPortAction());

    private void CopySelectedLocalPortAction()
    {
        ClipboardHelper.SetClipboard(SelectedConnectionInfo.LocalPort.ToString());
    }

    public ICommand CopySelectedRemoteIpAddressCommand => new RelayCommand(p => CopySelectedRemoteIpAddressAction());

    private void CopySelectedRemoteIpAddressAction()
    {
        ClipboardHelper.SetClipboard(SelectedConnectionInfo.RemoteIPAddress.ToString());
    }

    public ICommand CopySelectedRemotePortCommand => new RelayCommand(p => CopySelectedRemotePortAction());

    private void CopySelectedRemotePortAction()
    {
        ClipboardHelper.SetClipboard(SelectedConnectionInfo.RemotePort.ToString());
    }

    public ICommand CopySelectedProtocolCommand => new RelayCommand(p => CopySelectedProtocolAction());

    private void CopySelectedProtocolAction()
    {
        ClipboardHelper.SetClipboard(SelectedConnectionInfo.Protocol.ToString());
    }

    public ICommand CopySelectedStateCommand => new RelayCommand(p => CopySelectedStateAction());

    private void CopySelectedStateAction()
    {
        ClipboardHelper.SetClipboard(TcpStateTranslator.GetInstance().Translate(SelectedConnectionInfo.TcpState));
    }

    public ICommand ExportCommand => new RelayCommand(p => ExportAction());

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
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? ConnectionResults : new ObservableCollection<ConnectionInfo>(SelectedConnectionInfos.Cast<ConnectionInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.Connections_ExportFileType = instance.FileType;
            SettingsManager.Current.Connections_ExportFilePath = instance.FilePath;
        }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportFileType[] { ExportFileType.CSV, ExportFileType.XML, ExportFileType.JSON }, true, SettingsManager.Current.Connections_ExportFileType, SettingsManager.Current.Connections_ExportFilePath);

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

        ConnectionResults.Clear();

        (await Connection.GetActiveTcpConnectionsAsync()).ForEach(x => ConnectionResults.Add(x));

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
}
