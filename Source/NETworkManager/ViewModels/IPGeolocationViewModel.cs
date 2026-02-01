using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the IP geolocation view.
/// </summary>
public class IPGeolocationViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPGeolocationViewModel));

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    /// <summary>
    /// Backing field for <see cref="Host"/>.
    /// </summary>
    private string _host;

    /// <summary>
    /// Gets or sets the host to query.
    /// </summary>
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

    /// <summary>
    /// Gets the collection view of host history.
    /// </summary>
    public ICollectionView HostHistoryView { get; }

    /// <summary>
    /// Backing field for <see cref="IsRunning"/>.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the query is running.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="IsResultVisible"/>.
    /// </summary>
    private bool _isResultVisible;

    /// <summary>
    /// Gets or sets a value indicating whether the result is visible.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Result"/>.
    /// </summary>
    private IPGeolocationInfo _result;

    /// <summary>
    /// Gets the IP geolocation result.
    /// </summary>
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

    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="IPGeolocationViewModel"/> class.
    /// </summary>
    /// <param name="tabId">The ID of the tab.</param>
    /// <param name="host">The host to query.</param>
    public IPGeolocationViewModel(Guid tabId, string host)
    {
        ConfigurationManager.Current.IPGeolocationTabCount++;

        _tabId = tabId;
        Host = host;

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPGeolocation_HostHistory);

        LoadSettings();
    }

    /// <summary>
    /// Called when the view is loaded.
    /// </summary>
    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Host))
            Query().ConfigureAwait(false);

        _firstLoad = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to start the query.
    /// </summary>
    public ICommand QueryCommand => new RelayCommand(_ => QueryAction(), Query_CanExecute);

    /// <summary>
    /// Checks if the query command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool Query_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Action to start the query.
    /// </summary>
    private void QueryAction()
    {
        Query().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to export the result.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    /// <summary>
    /// Action to export the result.
    /// </summary>
    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Performs the IP geolocation query.
    /// </summary>
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

    /// <summary>
    /// Exports the result.
    /// </summary>
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
                    [Result]);
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.IPGeolocation_ExportFileType = instance.FileType;
            SettingsManager.Current.IPGeolocation_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
                ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], false, SettingsManager.Current.IPGeolocation_ExportFileType,
        SettingsManager.Current.IPGeolocation_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Called when the view is closed.
    /// </summary>
    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        ConfigurationManager.Current.IPGeolocationTabCount--;
    }

    /// <summary>
    /// Adds the host to the history.
    /// </summary>
    /// <param name="host">The host to add.</param>
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