using Dragablz;
using MahApps.Metro.SimpleChildWindow;
using Microsoft.Web.WebView2.Core;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.WebConsole;
using NETworkManager.Profiles;
using NETworkManager.Profiles.Application;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class WebConsoleHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    public IInterTabClient InterTabClient { get; }

    public string InterTabPartition
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

    public ObservableCollection<DragablzTabItem> TabItems { get; }

    /// <summary>
    ///     Variable indicates if the Edge WebView2 runtime is available.
    /// </summary>
    public bool IsRuntimeAvailable
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

    public int SelectedTabIndex
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

    #endregion

    #region Constructor

    public WebConsoleHostViewModel()
    {
        try
        {
            CoreWebView2Environment.GetAvailableBrowserVersionString();
            IsRuntimeAvailable = true;
        }
        catch (WebView2RuntimeNotFoundException)
        {
            IsRuntimeAvailable = false;
        }

        InterTabClient = new DragablzInterTabClient(ApplicationName.WebConsole);
        InterTabPartition = nameof(ApplicationName.WebConsole);

        TabItems = [];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.WebConsole;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.WebConsole_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.WebConsole_Url;

    public void OnProfileManagerDialogOpen()
    {
        ConfigurationManager.OnDialogOpen();
    }

    public void OnProfileManagerDialogClose()
    {
        ConfigurationManager.OnDialogClose();
    }

    #endregion

    #region ICommand & Actions

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction());

    private void ConnectAction()
    {
        _ = Connect();
    }

    public ICommand ReloadCommand => new RelayCommand(ReloadAction);

    private void ReloadAction(object view)
    {
        if (view is WebConsoleControl control)
            if (control.ReloadCommand.CanExecute(null))
                control.ReloadCommand.Execute(null);
    }

    public ICommand ConnectProfileCommand => new RelayCommand(_ => ConnectProfileAction(), ConnectProfile_CanExecute);

    private bool ConnectProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void ConnectProfileAction()
    {
        ConnectProfile();
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        EventSystem.RedirectToSettings();
    }

    public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

    private static void OpenWebsiteAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }

    #endregion

    #region Methods

    private Task Connect()
    {
        var childWindow = new WebConsoleConnectChildWindow();

        var childWindowViewModel = new WebConsoleConnectViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            // Create profile info
            var info = new WebConsoleSessionInfo
            {
                Url = instance.Url
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddUrlToHistory(instance.Url);

            Connect(info);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();
        });

        childWindow.Title = Strings.Connect;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        ConfigurationManager.OnDialogOpen();

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private void ConnectProfile()
    {
        Connect(WebConsole.CreateSessionInfo(SelectedProfile),
            SelectedProfile.Name);
    }

    private void Connect(WebConsoleSessionInfo sessionInfo, string header = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Url, new WebConsoleControl(tabId, sessionInfo), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    // Modify history list
    private static void AddUrlToHistory(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        SettingsManager.Current.WebConsole_UrlHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.WebConsole_UrlHistory.ToList(), url,
                SettingsManager.Current.General_HistoryListEntries));
    }

    #endregion
}
