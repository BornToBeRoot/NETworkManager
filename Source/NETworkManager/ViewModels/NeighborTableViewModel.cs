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
/// View model for the neighbor table view (IPv4 ARP + IPv6 NDP).
/// </summary>
public class NeighborTableViewModel : ViewModelBase
{
    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="NeighborTableViewModel"/> class.
    /// </summary>
    public NeighborTableViewModel()
    {
        _isLoading = true;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);

        ((ListCollectionView)ResultsView).CustomSort = Comparer<NeighborInfo>.Create((x, y) =>
            IPAddressHelper.CompareIPAddresses(x.IPAddress, y.IPAddress));

        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not NeighborInfo info)
                return false;

            var stateLocalized = ResourceTranslate(info.State);

            return info.IPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.MACAddress.ToString().IndexOf(Search.Replace("-", "").Replace(":", ""),
                       StringComparison.OrdinalIgnoreCase) > -1 ||
                   (info.InterfaceAlias ?? string.Empty).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.State.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   stateLocalized.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   (info.IsMulticast ? Strings.Yes : Strings.No).IndexOf(
                       Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get neighbor table
        Refresh(true).ConfigureAwait(false);

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x =>
            x.Value == SettingsManager.Current.NeighborTable_AutoRefreshTime.Value &&
            x.TimeUnit == SettingsManager.Current.NeighborTable_AutoRefreshTime.TimeUnit);
        AutoRefreshEnabled = SettingsManager.Current.NeighborTable_AutoRefreshEnabled;

        _isLoading = false;
    }

    private static string ResourceTranslate(NeighborState state)
    {
        return Localization.ResourceTranslator.Translate(Localization.ResourceIdentifier.NeighborState, state);
    }
    #endregion

    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(NeighborTableViewModel));

    private readonly bool _isLoading;

    private readonly DispatcherTimer _autoRefreshTimer = new();

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

    public ObservableCollection<NeighborInfo> Results
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ICollectionView ResultsView { get; }

    public NeighborInfo SelectedResult
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

    public bool AutoRefreshEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.NeighborTable_AutoRefreshEnabled = value;

            field = value;

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

    public AutoRefreshTimeInfo SelectedAutoRefreshTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.NeighborTable_AutoRefreshTime = value;

            field = value;

            if (AutoRefreshEnabled)
            {
                _autoRefreshTimer.Interval = AutoRefreshTime.CalculateTimeSpan(value);
                _autoRefreshTimer.Start();
            }

            OnPropertyChanged();
        }
    }

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

    public bool IsModifying
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

    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands & Actions

    public ICommand RefreshCommand => new RelayCommand(_ => RefreshAction().ConfigureAwait(false), Refresh_CanExecute);

    private bool Refresh_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen &&
               !IsRefreshing &&
               !IsModifying &&
               !AutoRefreshEnabled;
    }

    private async Task RefreshAction()
    {
        IsStatusMessageDisplayed = false;

        await Refresh();
    }

    public ICommand DeleteTableCommand =>
        new RelayCommand(_ => DeleteTableAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task DeleteTableAction()
    {
        IsModifying = true;
        IsStatusMessageDisplayed = false;

        try
        {
            await NeighborTable.DeleteTableAsync();

            await Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsModifying = false;
        }
    }

    public ICommand DeleteEntryCommand =>
        new RelayCommand(_ => DeleteEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task DeleteEntryAction()
    {
        IsModifying = true;
        IsStatusMessageDisplayed = false;

        try
        {
            await NeighborTable.DeleteEntryAsync(SelectedResult.IPAddress.ToString(), SelectedResult.InterfaceIndex);

            await Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
        finally
        {
            IsModifying = false;
        }
    }

    public ICommand AddEntryCommand =>
        new RelayCommand(_ => AddEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task AddEntryAction()
    {
        IsModifying = true;
        IsStatusMessageDisplayed = false;

        var interfaces = await NeighborTable.GetInterfacesAsync();

        var childWindow = new NeighborTableAddEntryChildWindow();

        var childWindowViewModel = new NeighborTableAddEntryViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                await NeighborTable.AddEntryAsync(instance.IPAddress, MACAddressHelper.Format(instance.MACAddress, "-"), instance.SelectedInterface.Key);

                await Refresh();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }
            finally
            {
                IsModifying = false;
            }
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = false;
        }, interfaces);

        childWindow.Title = Strings.AddEntry;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private bool ModifyEntry_CanExecute(object parameter)
    {
        return ConfigurationManager.Current.IsAdmin &&
               Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen &&
               !IsRefreshing &&
               !IsModifying;
    }

    public ICommand RestartAsAdminCommand => new RelayCommand(_ => RestartAsAdminAction().ConfigureAwait(false));

    private async Task RestartAsAdminAction()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.RestartApplication(true);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

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
                        : new ObservableCollection<NeighborInfo>(SelectedResults.Cast<NeighborInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.NeighborTable_ExportFileType = instance.FileType;
            SettingsManager.Current.NeighborTable_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.NeighborTable_ExportFileType,
        SettingsManager.Current.NeighborTable_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    private async Task Refresh(bool init = false)
    {
        IsRefreshing = true;

        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        if (!init)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        Results.Clear();

        (await NeighborTable.GetTableAsync()).ForEach(Results.Add);

        StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
        IsStatusMessageDisplayed = true;

        IsRefreshing = false;
    }

    public void OnViewVisible()
    {
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Start();
    }

    public void OnViewHide()
    {
        if (AutoRefreshEnabled)
            _autoRefreshTimer.Stop();
    }

    #endregion

    #region Events

    private async void AutoRefreshTimer_Tick(object sender, EventArgs e)
    {
        _autoRefreshTimer.Stop();

        // Skip refresh while a modify operation (add/delete) is in progress to avoid
        // clearing the table while the user is interacting with it.
        if (!IsModifying)
            await Refresh();

        _autoRefreshTimer.Start();
    }

    #endregion
}
