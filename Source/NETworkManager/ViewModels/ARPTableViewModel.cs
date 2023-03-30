using NETworkManager.Models.Network;
using System;
using System.Collections;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Settings;
using System.Windows.Threading;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using NETworkManager.Models.Export;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels;

public class ARPTableViewModel : ViewModelBase
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

            ARPInfoResultsView.Refresh();

            OnPropertyChanged();
        }
    }

    private ObservableCollection<ARPInfo> _arpInfoResults = new();
    public ObservableCollection<ARPInfo> ARPInfoResults
    {
        get => _arpInfoResults;
        set
        {
            if (value == _arpInfoResults)
                return;

            _arpInfoResults = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ARPInfoResultsView { get; }

    private ARPInfo _selectedARPInfo;
    public ARPInfo SelectedARPInfo
    {
        get => _selectedARPInfo;
        set
        {
            if (value == _selectedARPInfo)
                return;

            _selectedARPInfo = value;
            OnPropertyChanged();
        }
    }

    private IList _selectedARPInfos = new ArrayList();
    public IList SelectedARPInfos
    {
        get => _selectedARPInfos;
        set
        {
            if (Equals(value, _selectedARPInfos))
                return;

            _selectedARPInfos = value;
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
    public ARPTableViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;
        _dialogCoordinator = instance;

        // Result view + search
        ARPInfoResultsView = CollectionViewSource.GetDefaultView(ARPInfoResults);
        ARPInfoResultsView.SortDescriptions.Add(new SortDescription(nameof(ARPInfo.IPAddressInt32), ListSortDirection.Ascending));
        ARPInfoResultsView.Filter = o =>
        {
            if (o is not ARPInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            // Search by IPAddress and MACAddress
            return info.IPAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || info.MACAddress.ToString().IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 || (info.IsMulticast ? Localization.Resources.Strings.Yes : Localization.Resources.Strings.No).IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get ARP table
        Refresh();

        // Auto refresh
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.GetDefaults);
        SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x => (x.Value == SettingsManager.Current.ARPTable_AutoRefreshTime.Value && x.TimeUnit == SettingsManager.Current.ARPTable_AutoRefreshTime.TimeUnit));
        AutoRefreshEnabled = SettingsManager.Current.ARPTable_AutoRefreshEnabled;

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

    public ICommand DeleteTableCommand => new RelayCommand(p => DeleteTableAction(), DeleteTable_CanExecute);

    private bool DeleteTable_CanExecute(object paramter)
    {
        return Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

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

    public ICommand DeleteEntryCommand => new RelayCommand(p => DeleteEntryAction(), DeleteEntry_CanExecute);

    private bool DeleteEntry_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private async Task DeleteEntryAction()
    {
        IsStatusMessageDisplayed = false;

        try
        {
            var arpTable = new ARP();

            arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

            await arpTable.DeleteEntryAsync(SelectedARPInfo.IPAddress.ToString());

            await Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }
    }

    public ICommand AddEntryCommand => new RelayCommand(p => AddEntryAction(), AddEntry_CanExecute);

    private bool AddEntry_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private async Task AddEntryAction()
    {
        IsStatusMessageDisplayed = false;

        var customDialog = new CustomDialog
        {
            Title = Localization.Resources.Strings.AddEntry
        };

        var arpTableAddEntryViewModel = new ArpTableAddEntryViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

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
        }, instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        });

        customDialog.Content = new ARPTableAddEntryDialog
        {
            DataContext = arpTableAddEntryViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public ICommand CopySelectedIPAddressCommand => new RelayCommand(p => CopySelectedIPAddressAction());

    private void CopySelectedIPAddressAction()
    {
        ClipboardHelper.SetClipboard(SelectedARPInfo.IPAddress.ToString());
    }

    public ICommand CopySelectedMACAddressCommand => new RelayCommand(p => CopySelectedMACAddressAction());

    private void CopySelectedMACAddressAction()
    {
        ClipboardHelper.SetClipboard(MACAddressHelper.GetDefaultFormat(SelectedARPInfo.MACAddress.ToString()));
    }

    public ICommand CopySelectedMulticastCommand => new RelayCommand(p => CopySelectedMulticastAction());

    private void CopySelectedMulticastAction()
    {
        ClipboardHelper.SetClipboard(SelectedARPInfo.IsMulticast ? Localization.Resources.Strings.Yes : Localization.Resources.Strings.No);
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
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? ARPInfoResults : new ObservableCollection<ARPInfo>(SelectedARPInfos.Cast<ARPInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.ARPTable_ExportFileType = instance.FileType;
            SettingsManager.Current.ARPTable_ExportFilePath = instance.FilePath;
        }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportFileType[] { ExportFileType.CSV, ExportFileType.XML, ExportFileType.JSON }, true, SettingsManager.Current.ARPTable_ExportFileType, SettingsManager.Current.ARPTable_ExportFilePath);

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

        ARPInfoResults.Clear();

        (await ARP.GetTableAsync()).ForEach(x => ARPInfoResults.Add(x));

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
    private void ArpTable_UserHasCanceled(object sender, EventArgs e)
    {
        StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
        IsStatusMessageDisplayed = true;
    }

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