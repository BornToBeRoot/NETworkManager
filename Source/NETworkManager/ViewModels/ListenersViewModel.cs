using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
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
/// View model for the listeners view.
/// </summary>
public class ListenersViewModel : ViewModelBase
{
    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenersViewModel"/> class.
    /// </summary>
    public ListenersViewModel()
    {
        _isLoading = true;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        ((ListCollectionView)ResultsView).CustomSort = Comparer<ListenerInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress));

        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not ListenerInfo info)
                return false;

            // Search by IP Address, Port and Protocol
            return info.IPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Port.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Protocol.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get listeners
        Refresh(true).ConfigureAwait(false);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.Listeners_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.Listeners_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.Listeners_AutoRefreshEnabled;

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

    private static readonly ILog Log = LogManager.GetLogger(typeof(ListenersViewModel));

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// The timer for auto-refresh.
    /// </summary>
    private readonly DispatcherTimer _autoRefreshTimer = new();

    /// <summary>
    /// Backing field for <see cref="Search"/>.
    /// </summary>
    private string _search;

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Results"/>.
    /// </summary>
    private ObservableCollection<ListenerInfo> _results = new();

    /// <summary>
    /// Gets or sets the collection of listener results.
    /// </summary>
    public ObservableCollection<ListenerInfo> Results
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

    /// <summary>
    /// Gets the collection view for the listener results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedResult"/>.
    /// </summary>
    private ListenerInfo _selectedResult;

    /// <summary>
    /// Gets or sets the currently selected listener result.
    /// </summary>
    public ListenerInfo SelectedResult
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
    /// Gets or sets the list of selected listener results.
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
    /// Backing field for <see cref="AutoRefreshEnabled"/>.
    /// </summary>
    private bool _autoRefreshEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether auto-refresh is enabled.
    /// </summary>
    public bool AutoRefreshEnabled
    {
        get => _autoRefreshEnabled;
        set
        {
            if (value == _autoRefreshEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.Listeners_AutoRefreshEnabled = value;

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

    /// <summary>
    /// Gets the collection view for the auto-refresh times.
    /// </summary>
    public ICollectionView AutoRefreshTimes { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedAutoRefreshTime"/>.
    /// </summary>
    private AutoRefreshTimeInfo _selectedAutoRefreshTime;

    /// <summary>
    /// Gets or sets the selected auto-refresh time.
    /// </summary>
    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get => _selectedAutoRefreshTime;
        set
        {
            if (value == _selectedAutoRefreshTime)
                return;

            if (!_isLoading)
                SettingsManager.Current.Listeners_AutoRefreshTime = value;

            _selectedAutoRefreshTime = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="IsRefreshing"/>.
    /// </summary>
    private bool _isRefreshing;

    /// <summary>
    /// Gets or sets a value indicating whether the view model is currently refreshing.
    /// </summary>
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
    /// Gets or sets the status message.
    /// </summary>
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

    /// <summary>
    /// Gets the command to refresh the listeners.
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
    /// Action to refresh the listeners.
    /// </summary>
    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
    }

    /// <summary>
    /// Gets the command to export the listeners.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    /// <summary>
    /// Action to export the listeners.
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
                        : new ObservableCollection<ListenerInfo>(SelectedResults.Cast<ListenerInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Listeners_ExportFileType = instance.FileType;
            SettingsManager.Current.Listeners_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.Listeners_ExportFileType, SettingsManager.Current.Listeners_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Refreshes the listeners.
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

        (await Listener.GetAllActiveListenersAsync()).ForEach(Results.Add);

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