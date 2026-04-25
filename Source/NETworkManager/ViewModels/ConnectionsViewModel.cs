using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization;
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the connections view.
/// </summary>
public class ConnectionsViewModel : ViewModelBase
{
    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionsViewModel"/> class.
    /// </summary>
    public ConnectionsViewModel()
    {
        _isLoading = true;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        ((ListCollectionView)ResultsView).CustomSort = Comparer<ConnectionInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.LocalIPAddress, y.LocalIPAddress));

        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not ConnectionInfo info)
                return false;

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
        Refresh(true).ConfigureAwait(false);

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

    /// <summary>
    /// Handles the Tick event of the auto-refresh timer.
    /// </summary>
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

    private static readonly ILog Log = LogManager.GetLogger(typeof(ConnectionsViewModel));

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// The timer for auto-refresh.
    /// </summary>
    private readonly DispatcherTimer _autoRefreshTimer = new();

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            ResultsView.Refresh();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the collection of connection results.
    /// </summary>
    public ObservableCollection<ConnectionInfo> Results
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    /// Gets the collection view for the connection results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Gets or sets the currently selected connection result.
    /// </summary>
    public ConnectionInfo SelectedResult
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the list of selected connection results.
    /// </summary>
    public IList SelectedResults
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    /// <summary>
    /// Gets or sets a value indicating whether auto-refresh is enabled.
    /// </summary>
    public bool AutoRefreshEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Connections_AutoRefreshEnabled = value;

            field = value;

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

    /// <summary>
    /// Gets the collection view for the auto-refresh times.
    /// </summary>
    public ICollectionView AutoRefreshTimes { get; }

    /// <summary>
    /// Gets or sets the selected auto-refresh time.
    /// </summary>
    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Connections_AutoRefreshTime = value;

            field = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view model is currently refreshing.
    /// </summary>
    public bool IsRefreshing
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to refresh the connections.
    /// </summary>
    public ICommand RefreshCommand => new RelayCommand(_ => RefreshAction().ConfigureAwait(false), Refresh_CanExecute);

    /// <summary>
    /// Checks if the refresh command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool Refresh_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen &&
               !IsRefreshing &&
               !AutoRefreshEnabled;
    }

    /// <summary>
    /// Action to refresh the connections.
    /// </summary>
    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
    }

    /// <summary>
    /// Gets the command to export the connections.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    /// <summary>
    /// Action to export the connections.
    /// </summary>
    private Task ExportAction()
    {
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
                        : new ObservableCollection<ConnectionInfo>(SelectedResults.Cast<ConnectionInfo>()
                            .ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Connections_ExportFileType = instance.FileType;
            SettingsManager.Current.Connections_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], true, SettingsManager.Current.Connections_ExportFileType,
            SettingsManager.Current.Connections_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Refreshes the connections.
    /// </summary>
    /// <param name="init">Indicates whether this is the initial refresh.</param>
    private async Task Refresh(bool init = false)
    {
        IsRefreshing = true;

        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        if (init == false)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        Results.Clear();

        (await Connection.GetActiveTcpConnectionsAsync()).ForEach(Results.Add);

        StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
        IsStatusMessageDisplayed = true;

        IsRefreshing = false;
    }

    /// <summary>
    /// Called when the view becomes visible.
    /// </summary>
    public void OnViewVisible()
    {
        // Restart timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Start();
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
        // Temporarily stop timer...
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Stop();
    }

    #endregion
}