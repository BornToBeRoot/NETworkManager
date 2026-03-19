using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
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
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

public class SNTPLookupViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(SNTPLookupViewModel));

    private readonly Guid _tabId;
    private readonly bool _isLoading;
    private bool _closed;

    public ICollectionView SNTPServers { get; }

    public ServerConnectionInfoProfile SNTPServer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNTPLookup_SelectedSNTPServer = value;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public bool IsRunning
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

    public ObservableCollection<SNTPLookupInfo> Results
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
        }
    } = [];

    public ICollectionView ResultsView { get; }

    public SNTPLookupInfo SelectedResult
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

    #region Contructor, load settings

    public SNTPLookupViewModel(Guid tabId)
    {
        _isLoading = true;

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
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
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

    private Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

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
                        : new ObservableCollection<SNTPLookupInfo>(SelectedResults.Cast<SNTPLookupInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                      Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                       Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.SNTPLookup_ExportFileType = instance.FileType;
            SettingsManager.Current.SNTPLookup_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.SNTPLookup_ExportFileType, SettingsManager.Current.SNTPLookup_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
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