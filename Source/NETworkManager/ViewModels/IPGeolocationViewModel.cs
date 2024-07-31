using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class IPGeolocationViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPGeolocationViewModel));

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    private string _host;

    public string Host
    {
        get => _host;
        set
        {
            if (value == _host)
                return;

            _host = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView HostHistoryView { get; }

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

    private bool _isResultVisible;

    public bool IsResultVisible
    {
        get => _isResultVisible;
        set
        {
            if (value == _isResultVisible)
                return;

            _isResultVisible = value;
            OnPropertyChanged();
        }
    }

    private IPGeolocationInfo _result;

    public IPGeolocationInfo Result
    {
        get => _result;
        private set
        {
            if (value == _result)
                return;

            _result = value;
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

    public IPGeolocationViewModel(IDialogCoordinator instance, Guid tabId, string host)
    {
        _dialogCoordinator = instance;

        ConfigurationManager.Current.IPGeolocationTabCount++;

        _tabId = tabId;
        Host = host;

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPGeolocation_HostHistory);

        LoadSettings();
    }

    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Host))
            Query().ConfigureAwait(false);

        _firstLoad = false;
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
        Query().ConfigureAwait(false);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task Query()
    {
        IsStatusMessageDisplayed = false;
        IsResultVisible = false;
        IsRunning = true;

        Result = null;

        DragablzTabItem.SetTabHeader(_tabId, Host);

        try
        {
            var result = await IPGeolocationService.GetInstance().GetIPGeolocationAsync(Host);

            if (result.HasError)
            {
                Log.Error($"ip-api.com error: {result.ErrorMessage}, error code: {result.ErrorCode}");

                StatusMessage = $"ip-api.com: {result.ErrorMessage}";
                IsStatusMessageDisplayed = true;

                return;
            }

            if (result.RateLimitIsReached)
            {
                Log.Warn($"ip-api.com rate limit reached. Try again in {result.RateLimitRemainingTime} seconds.");

                StatusMessage =
                    $"ip-api.com {string.Format(Strings.RateLimitReachedTryAgainInXSeconds, result.RateLimitRemainingTime)}";
                IsStatusMessageDisplayed = true;

                return;
            }

            Result = result.Info;
            IsResultVisible = true;

            AddHostToHistory(Host);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }

        IsRunning = false;
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
                        [Result]);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(window, Strings.Error,
                        Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                        Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.IPGeolocation_ExportFileType = instance.FileType;
                SettingsManager.Current.IPGeolocation_ExportFilePath = instance.FilePath;
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(window, customDialog); }, [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
            ], false, SettingsManager.Current.IPGeolocation_ExportFileType,
            SettingsManager.Current.IPGeolocation_ExportFilePath);

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

        ConfigurationManager.Current.IPGeolocationTabCount--;
    }

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.IPGeolocation_HostHistory.ToList(), host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.IPGeolocation_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.Whois_DomainHistory.Add(x));
    }

    #endregion
}