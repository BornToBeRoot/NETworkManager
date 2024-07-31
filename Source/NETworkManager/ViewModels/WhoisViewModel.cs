using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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

public class WhoisViewModel : ViewModelBase
{
    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    private string _domain;

    public string Domain
    {
        get => _domain;
        set
        {
            if (value == _domain)
                return;

            _domain = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView WebsiteUriHistoryView { get; }

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

    private string _result;

    public string Result
    {
        get => _result;
        set
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

    public WhoisViewModel(IDialogCoordinator instance, Guid tabId, string domain)
    {
        _dialogCoordinator = instance;

        ConfigurationManager.Current.WhoisTabCount++;

        _tabId = tabId;
        Domain = domain;

        // Set collection view
        WebsiteUriHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Whois_DomainHistory);

        LoadSettings();
    }

    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Domain))
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

        DragablzTabItem.SetTabHeader(_tabId, Domain);

        try
        {
            var whoisServer = Whois.GetWhoisServer(Domain);

            if (string.IsNullOrEmpty(whoisServer))
            {
                StatusMessage = string.Format(Strings.WhoisServerNotFoundForTheDomain, Domain);
                IsStatusMessageDisplayed = true;
            }
            else
            {
                Result = await Whois.QueryAsync(Domain, whoisServer);
                IsResultVisible = true;

                AddDomainToHistory(Domain);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }

        IsRunning = false;
    }

    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        ConfigurationManager.Current.WhoisTabCount--;
    }

    private void AddDomainToHistory(string domain)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.Whois_DomainHistory.ToList(), domain,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.Whois_DomainHistory.Clear();
        OnPropertyChanged(nameof(Domain)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.Whois_DomainHistory.Add(x));
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
                ExportManager.Export(instance.FilePath, Result);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(window, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine +
                    ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.Whois_ExportFileType = instance.FileType;
            SettingsManager.Current.Whois_ExportFilePath = instance.FilePath;
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(window, customDialog); }, [
            ExportFileType.Txt
        ], false, SettingsManager.Current.Whois_ExportFileType, SettingsManager.Current.Whois_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(window, customDialog);
    }

    #endregion
}