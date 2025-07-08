using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.HostsFileEditor;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class HostsFileEditorViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(HostsFileEditorViewModel));

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly bool _isLoading;

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

    private ObservableCollection<HostsFileEntry> _results = [];

    public ObservableCollection<HostsFileEntry> Results
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

    private HostsFileEntry _selectedResult;

    public HostsFileEntry SelectedResult
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

    private bool _isModifying;

    public bool IsModifying
    {
        get => _isModifying;
        set
        {
            if (value == _isModifying)
                return;

            _isModifying = value;
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
        private set
        {
            if (value == _statusMessage)
                return;

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public HostsFileEditorViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;
        _dialogCoordinator = instance;

        // Result view + search
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not HostsFileEntry entry)
                return false;

            return entry.IPAddress.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   entry.Hostname.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   entry.Comment.IndexOf(Search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Get hosts file entries
        Refresh(true).ConfigureAwait(false);

        // Watch hosts file for changes
        HostsFileEditor.HostsFileChanged += (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Refresh().ConfigureAwait(false);
            });
        };

        _isLoading = false;
    }

    private void LoadSettings()
    {

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
               !IsModifying;
    }

    private async Task RefreshAction()
    {
        await Refresh();
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
                         : new ObservableCollection<HostsFileEntry>(SelectedResults.Cast<HostsFileEntry>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.HostsFileEditor_ExportFileType = instance.FileType;
            SettingsManager.Current.HostsFileEditor_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.HostsFileEditor_ExportFileType, SettingsManager.Current.HostsFileEditor_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    public ICommand EnableEntryCommand => new RelayCommand(_ => EnableEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task EnableEntryAction()
    {
        IsModifying = true;

        var result = await HostsFileEditor.EnableEntryAsync(SelectedResult);

        if (result != HostsFileEntryModifyResult.Success)
            await ShowErrorMessageAsync(result);

        IsModifying = false;
    }

    public ICommand DisableEntryCommand => new RelayCommand(_ => DisableEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task DisableEntryAction()
    {
        IsModifying = true;

        var result = await HostsFileEditor.DisableEntryAsync(SelectedResult);

        if (result != HostsFileEntryModifyResult.Success)
            await ShowErrorMessageAsync(result);

        IsModifying = false;
    }

    public ICommand AddEntryCommand => new RelayCommand(_ => AddEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task AddEntryAction()
    {
        IsModifying = true;

        var childWindow = new HostsFileEditorEntryChildWindow();

        var childWindowViewModel = new HostsFileEditorEntryViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            var result = await HostsFileEditor.AddEntryAsync(new HostsFileEntry
            {
                IsEnabled = instance.IsEnabled,
                IPAddress = instance.IPAddress,
                Hostname = instance.Hostname,
                Comment = instance.Comment
            });

            if (result != HostsFileEntryModifyResult.Success)
                await ShowErrorMessageAsync(result);

            IsModifying = false;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = false;
        });

        childWindow.Title = Strings.AddEntry;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    private async Task ShowErrorMessageAsync(HostsFileEntryModifyResult result)
    {
        string message = result switch
        {
            HostsFileEntryModifyResult.ReadError => Strings.HostsFileReadErrorMessage,
            HostsFileEntryModifyResult.WriteError => Strings.HostsFileWriteErrorMessage,
            HostsFileEntryModifyResult.NotFound => Strings.HostsFileEntryNotFoundMessage,
            HostsFileEntryModifyResult.BackupError => Strings.HostsFileBackupErrorMessage,
            _ => Strings.UnkownError
        };

        var childWindow = new OKMessageChildWindow();

        var childWindowViewModel = new OKMessageViewModel(_ =>
         {
             childWindow.IsOpen = false;
             ConfigurationManager.Current.IsChildWindowOpen = false;
         }, message);

        childWindow.Title = Strings.Error;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    public ICommand EditEntryCommand => new RelayCommand(_ => EditEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task EditEntryAction()
    {
        IsModifying = true;

        var childWindow = new HostsFileEditorEntryChildWindow();

        var childWindowViewModel = new HostsFileEditorEntryViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            var result = await HostsFileEditor.EditEntryAsync(instance.Entry, new HostsFileEntry
            {
                IsEnabled = instance.IsEnabled,
                IPAddress = instance.IPAddress,
                Hostname = instance.Hostname,
                Comment = instance.Comment
            });

            if (result != HostsFileEntryModifyResult.Success)
                await ShowErrorMessageAsync(result);

            IsModifying = false;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = false;
        }, SelectedResult);

        childWindow.Title = Strings.EditEntry;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    public ICommand DeleteEntryCommand => new RelayCommand(_ => DeleteEntryAction().ConfigureAwait(false), ModifyEntry_CanExecute);

    private async Task DeleteEntryAction()
    {
        IsModifying = true;

        var childWindow = new OKCancelInfoMessageChildWindow();

        var childWindowViewModel = new OKCancelInfoMessageViewModel(async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            var result = await HostsFileEditor.DeleteEntryAsync(SelectedResult);

            if (result != HostsFileEntryModifyResult.Success)
                await ShowErrorMessageAsync(result);

            IsModifying = false;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = false;
        },
            string.Format(Strings.DeleteHostsFileEntryMessage, SelectedResult.IPAddress, SelectedResult.Hostname, string.IsNullOrEmpty(SelectedResult.Comment) ? "" : $"# {SelectedResult.Comment}"),
            Strings.Delete
        );

        childWindow.Title = Strings.DeleteEntry;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    private bool ModifyEntry_CanExecute(object obj)
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
            await _dialogCoordinator.ShowMessageAsync(this, Strings.Error, ex.Message,
                MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
        }
    }
    #endregion

    #region Methods

    private async Task Refresh(bool init = false)
    {
        if (IsRefreshing)
            return;

        IsRefreshing = true;

        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        // Retry 3 times if the hosts file is locked
        for (var i = 1; i < 4; i++)
        {
            // Wait for 2.5 seconds on refresh
            if (init == false || i > 1)
                await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

            try
            {
                var entries = await HostsFileEditor.GetHostsFileEntriesAsync();

                Results.Clear();

                entries.ToList().ForEach(Results.Add);

                StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
                IsStatusMessageDisplayed = true;

                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                StatusMessage = string.Format(Strings.FailedToLoadHostsFileMessage, ex.Message);

                if (i < 3)
                    StatusMessage += Environment.NewLine + string.Format(Strings.RetryingInXSecondsDots, GlobalStaticConfiguration.ApplicationUIRefreshInterval / 1000);

                IsStatusMessageDisplayed = true;

                await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);
            }
        }

        IsRefreshing = false;
    }

    public void OnViewVisible()
    {

    }

    public void OnViewHide()
    {

    }
    #endregion
}