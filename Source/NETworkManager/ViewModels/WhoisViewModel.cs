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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class WhoisViewModel : ViewModelBase
{
    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(WhoisViewModel));

    private readonly Guid _tabId;
    private bool _firstLoad = true;
    private bool _closed;

    public string Domain
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

    public ICollectionView WebsiteUriHistoryView { get; }

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

    public bool IsResultVisible
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

    public string Result
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

    #region Contructor, load settings

    public WhoisViewModel(Guid tabId, string domain)
    {
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
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
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
                ExportManager.Export(instance.FilePath, Result);
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Whois_ExportFileType = instance.FileType;
            SettingsManager.Current.Whois_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Txt
        ], false, SettingsManager.Current.Whois_ExportFileType, SettingsManager.Current.Whois_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    #endregion
}