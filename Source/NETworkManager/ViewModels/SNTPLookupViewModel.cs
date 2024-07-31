using System;
using System.Collections;
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
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class SNTPLookupViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly Guid _tabId;
    private readonly bool _isLoading;
    private bool _closed;

    public ICollectionView SNTPServers { get; }

    private ServerConnectionInfoProfile _sntpServer = new();

    public ServerConnectionInfoProfile SNTPServer
    {
        get => _sntpServer;
        set
        {
            if (value == _sntpServer)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNTPLookup_SelectedSNTPServer = value;

            _sntpServer = value;
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

    private ObservableCollection<SNTPLookupInfo> _results = [];

    public ObservableCollection<SNTPLookupInfo> Results
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

    private SNTPLookupInfo _selectedResult;

    public SNTPLookupInfo SelectedResult
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

    public SNTPLookupViewModel(IDialogCoordinator instance, Guid tabId)
    {
        _isLoading = true;

        _dialogCoordinator = instance;
        ConfigurationManager.Current.SNTPLookupTabCount++;

        _tabId = tabId;

        SNTPServers = new CollectionViewSource { Source = SettingsManager.Current.SNTPLookup_SNTPServers }.View;
        SNTPServers.SortDescriptions.Add(new SortDescription(nameof(ServerConnectionInfoProfile.Name),
            ListSortDirection.Ascending));
        SNTPServer =
            SNTPServers.SourceCollection.Cast<ServerConnectionInfoProfile>().FirstOrDefault(x =>
                x.Name == SettingsManager.Current.SNTPLookup_SelectedSNTPServer.Name) ??
            SNTPServers.SourceCollection.Cast<ServerConnectionInfoProfile>().First();

        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(
            new SortDescription(nameof(SNTPLookupInfo.Server), ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    public ICommand QueryCommand => new RelayCommand(_ => QueryAction(), Query_CanExecute);

    private bool Query_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;
    }

    private void QueryAction()
    {
        if (!IsRunning)
            Query();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private void Query()
    {
        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        IsRunning = true;

        // Reset the latest results
        Results.Clear();

        DragablzTabItem.SetTabHeader(_tabId, SNTPServer.Name);

        SNTPLookupSettings settings = new(
            SettingsManager.Current.SNTPLookup_Timeout
        );

        SNTPLookup lookup = new(settings);

        lookup.ResultReceived += Lookup_ResultReceived;
        lookup.LookupError += Lookup_LookupError;
        lookup.LookupComplete += Lookup_LookupComplete;

        lookup.QueryAsync(SNTPServer.Servers, SettingsManager.Current.Network_ResolveHostnamePreferIPv4);
    }

    private async Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var customDialog = new CustomDialog
        {
            Title = Strings.Export
        };

        var exportViewModel = new ExportViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(window, customDialog);

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<SNTPLookupInfo>(SelectedResults.Cast<SNTPLookupInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(window, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.SNTPLookup_ExportFileType = instance.FileType;
            SettingsManager.Current.SNTPLookup_ExportFilePath = instance.FilePath;
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(window, customDialog); }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.SNTPLookup_ExportFileType, SettingsManager.Current.SNTPLookup_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(window, customDialog);
    }

    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        ConfigurationManager.Current.SNTPLookupTabCount--;
    }

    #endregion

    #region Events

    private void Lookup_ResultReceived(object sender, SNTPLookupResultArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(delegate { Results.Add(e.Args); }));
    }

    private void Lookup_LookupError(object sender, SNTPLookupErrorArgs e)
    {
        if (!string.IsNullOrEmpty(StatusMessage))
            StatusMessage += Environment.NewLine;

        StatusMessage += e.IsDNSError ? e.ErrorMessage : $"{e.Server} ({e.IPEndPoint}) ==> {e.ErrorMessage}";
        IsStatusMessageDisplayed = true;
    }

    private void Lookup_LookupComplete(object sender, EventArgs e)
    {
        IsRunning = false;
    }

    #endregion
}