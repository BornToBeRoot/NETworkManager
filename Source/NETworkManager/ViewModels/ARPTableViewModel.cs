using log4net;
using MahApps.Metro.Controls;
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
/// View model for the ARP table view.
/// </summary>
public class ARPTableViewModel : ViewModelBase
{
    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="ARPTableViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public ARPTableViewModel()
    {
        _isLoading = true;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        ((ListCollectionView)ResultsView).CustomSort = Comparer<ARPInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress));

        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not ARPInfo info)
                return false;

            // Search by IPAddress and MACAddress
            return info.IPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.MACAddress.ToString().IndexOf(Search.Replace("-", "").Replace(":", ""),
                       StringComparison.OrdinalIgnoreCase) > -1 ||
                   (info.IsMulticast ? Strings.Yes : Strings.No).IndexOf(
                       Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get ARP table
        Refresh(true).ConfigureAwait(false);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.ARPTable_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.ARPTable_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.ARPTable_AutoRefreshEnabled;

        _isLoading = false;
    }

    #endregion

    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(ARPTableViewModel));

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
    private ObservableCollection<ARPInfo> _results = [];

    /// <summary>
    /// Gets or sets the collection of ARP results.
    /// </summary>
    public ObservableCollection<ARPInfo> Results
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
    /// Gets the collection view for the ARP results.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedResult"/>.
    /// </summary>
    private ARPInfo _selectedResult;

    /// <summary>
    /// Gets or sets the currently selected ARP result.
    /// </summary>
    public ARPInfo SelectedResult
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
    /// Gets or sets the list of selected ARP results.
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
                SettingsManager.Current.ARPTable_AutoRefreshEnabled = value;

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
                SettingsManager.Current.ARPTable_AutoRefreshTime = value;

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

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to refresh the ARP table.
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
    /// Action to refresh the ARP table.
    /// </summary>
    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
    }

    /// <summary>
    /// Gets the command to delete the ARP table.
    /// </summary>
    public ICommand DeleteTableCommand =>
        new RelayCommand(_ => DeleteTableAction().ConfigureAwait(false), DeleteTable_CanExecute);

    /// <summary>
    /// Checks if the delete table command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool DeleteTable_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to delete the ARP table.
    /// </summary>
    private async Task DeleteTableAction()
    {
        IsStatusMessageDisplayed = false;

        try
        {
            var arpTable = new ARP();

            arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

            await arpTable.DeleteTableAsync();

            await Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
    }

    /// <summary>
    /// Gets the command to delete an ARP entry.
    /// </summary>
    public ICommand DeleteEntryCommand =>
        new RelayCommand(_ => DeleteEntryAction().ConfigureAwait(false), DeleteEntry_CanExecute);

    /// <summary>
    /// Checks if the delete entry command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool DeleteEntry_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to delete an ARP entry.
    /// </summary>
    private async Task DeleteEntryAction()
    {
        IsStatusMessageDisplayed = false;

        try
        {
            var arpTable = new ARP();

            arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

            await arpTable.DeleteEntryAsync(SelectedResult.IPAddress.ToString());

            await Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
    }

    /// <summary>
    /// Gets the command to add an ARP entry.
    /// </summary>
    public ICommand AddEntryCommand =>
        new RelayCommand(_ => AddEntryAction().ConfigureAwait(false), AddEntry_CanExecute);

    /// <summary>
    /// Checks if the add entry command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool AddEntry_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to add an ARP entry.
    /// </summary>
    private async Task AddEntryAction()
    {
        IsStatusMessageDisplayed = false;

        var childWindow = new ARPTableAddEntryChildWindow();


        var childWindowViewModel = new ARPTableAddEntryViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                var arpTable = new ARP();

                arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

                await arpTable.AddEntryAsync(instance.IPAddress, MACAddressHelper.Format(instance.MACAddress, "-"));

                await Refresh();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddEntry;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to export the ARP table.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    /// <summary>
    /// Action to export the ARP table.
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
                        : new ObservableCollection<ARPInfo>(SelectedResults.Cast<ARPInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.ARPTable_ExportFileType = instance.FileType;
            SettingsManager.Current.ARPTable_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.ARPTable_ExportFileType,
        SettingsManager.Current.ARPTable_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Refreshes the ARP table.
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

        (await ARP.GetTableAsync()).ForEach(Results.Add);

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

    #region Events

    /// <summary>
    /// Handles the UserHasCanceled event from the ARP table.
    /// </summary>
    private void ArpTable_UserHasCanceled(object sender, EventArgs e)
    {
        StatusMessage = Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;
    }

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
}