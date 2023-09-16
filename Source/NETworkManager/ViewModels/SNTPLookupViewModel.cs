using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using NETworkManager.Models.Export;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class SNTPLookupViewModel : ViewModelBase
{
    #region Variables
    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly int _tabId;

    private readonly bool _isLoading;

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

    private ObservableCollection<SNTPLookupInfo> _results = new();
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
    public SNTPLookupViewModel(IDialogCoordinator instance, int tabId)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        _tabId = tabId;

        SNTPServers = new CollectionViewSource { Source = SettingsManager.Current.SNTPLookup_SNTPServers }.View;
        SNTPServers.SortDescriptions.Add(new SortDescription(nameof(ServerConnectionInfoProfile.Name), ListSortDirection.Ascending));
        SNTPServer = SNTPServers.SourceCollection.Cast<ServerConnectionInfoProfile>().FirstOrDefault(x => x.Name == SettingsManager.Current.SNTPLookup_SelectedSNTPServer.Name) ?? SNTPServers.SourceCollection.Cast<ServerConnectionInfoProfile>().First();

        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(SNTPLookupInfo.Server), ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {

    }

    #endregion

    #region ICommands & Actions
    public ICommand LookupCommand => new RelayCommand(_ => LookupAction(), Lookup_CanExecute);

    private bool Lookup_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private void LookupAction()
    {
        if (!IsRunning)
            StartLookup();
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

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
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? Results : new ObservableCollection<SNTPLookupInfo>(SelectedResults.Cast<SNTPLookupInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.SNTPLookup_ExportFileType = instance.FileType;
            SettingsManager.Current.SNTPLookup_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        }, new[]
        {
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        }, true, SettingsManager.Current.SNTPLookup_ExportFileType, SettingsManager.Current.SNTPLookup_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }
    #endregion

    #region Methods      
    private void StartLookup()
    {
        IsStatusMessageDisplayed = false;
        StatusMessage = string.Empty;

        IsRunning = true;

        // Reset the latest results
        Results.Clear();

        // Change the tab title (not nice, but it works)
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window != null)
        {
            foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
            {
                tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = SNTPServer.Name;
            }
        }

        SNTPLookupSettings settings = new(
            SettingsManager.Current.SNTPLookup_Timeout
        );

        SNTPLookup lookup = new(settings);

        lookup.ResultReceived += Lookup_ResultReceived;
        lookup.LookupError += Lookup_LookupError;
        lookup.LookupComplete += Lookup_LookupComplete;

        lookup.QueryAsync(SNTPServer.Servers, SettingsManager.Current.Network_ResolveHostnamePreferIPv4);
    }

    public void OnClose()
    {

    }
    #endregion

    #region Events
    private void Lookup_ResultReceived(object sender, SNTPLookupResultArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
        {
            Results.Add(e.Args);
        }));
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
