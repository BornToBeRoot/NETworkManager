using Dragablz;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.TigerVNC;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TigerVNC = NETworkManager.Profiles.Application.TigerVNC;

namespace NETworkManager.ViewModels;

public class TigerVNCHostViewModel : ProfileHostViewModelBase
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

    public bool IsConfigured
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

    #region Constructor, load settings

    public TigerVNCHostViewModel()
    {
        CheckSettings();

        InterTabClient = new DragablzInterTabClient(ApplicationName.TigerVNC);
        InterTabPartition = nameof(ApplicationName.TigerVNC);

        TabItems = [];

        InitializeProfileHost();

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.TigerVNC;

    public override void OnProfileManagerDialogOpen()
    {
        ConfigurationManager.OnDialogOpen();
    }

    public override void OnProfileManagerDialogClose()
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

    private bool Connect_CanExecute(object obj)
    {
        return IsConfigured;
    }

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction(), Connect_CanExecute);

    private void ConnectAction()
    {
        _ = Connect();
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction);

    private void ReconnectAction(object view)
    {
        if (view is not TigerVNCControl control)
            return;

        if (control.ReconnectCommand.CanExecute(null))
            control.ReconnectCommand.Execute(null);
    }

    public ICommand ConnectProfileCommand => new RelayCommand(_ => ConnectProfileAction(), ConnectProfile_CanExecute);

    private bool ConnectProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void ConnectProfileAction()
    {
        ConnectProfile(SelectedProfile);
    }

    public ICommand ConnectProfileExternalCommand => new RelayCommand(_ => ConnectProfileExternalAction());

    private void ConnectProfileExternalAction()
    {
        ConnectProfileExternal();
    }

    public ICommand OpenSettingsCommand => new RelayCommand(_ => OpenSettingsAction());

    private static void OpenSettingsAction()
    {
        NavigationManager.OpenSettings();
    }

    #endregion

    #region Methods

    private void CheckSettings()
    {
        IsConfigured = !string.IsNullOrEmpty(SettingsManager.Current.TigerVNC_ApplicationFilePath) &&
                       File.Exists(SettingsManager.Current.TigerVNC_ApplicationFilePath);
    }

    private Task Connect(string host = null)
    {
        var childWindow = new TigerVNCConnectChildWindow();

        var childWindowViewModel = new TigerVNCConnectViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            // Create profile info
            var sessionInfo = new TigerVNCSessionInfo
            {
                Host = instance.Host,
                Port = instance.Port
            };

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            ListHelper.Modify(SettingsManager.Current.TigerVNC_HostHistory, instance.Host,
                SettingsManager.Current.General_HistoryListEntries);
            ListHelper.Modify(SettingsManager.Current.TigerVNC_PortHistory, instance.Port,
                SettingsManager.Current.General_HistoryListEntries);

            // Connect
            Connect(sessionInfo);
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();
        }, host);


        childWindow.Title = Strings.Connect;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        ConfigurationManager.OnDialogOpen();

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    ///     Connects to the profile.
    /// </summary>
    /// <param name="profileInfo">Profile to connect to.</param>
    public void ConnectProfile(ProfileInfo profileInfo)
    {
        Connect(TigerVNC.CreateSessionInfo(profileInfo), profileInfo.Name);
    }

    private void ConnectProfileExternal()
    {
        var sessionInfo = TigerVNC.CreateSessionInfo(SelectedProfile);

        Process.Start(new ProcessStartInfo
        {
            FileName = SettingsManager.Current.TigerVNC_ApplicationFilePath,
            Arguments = Models.TigerVNC.TigerVNC.BuildCommandLine(sessionInfo)
        });
    }

    private void Connect(TigerVNCSessionInfo sessionInfo, string header = null)
    {
        sessionInfo.ApplicationFilePath = SettingsManager.Current.TigerVNC_ApplicationFilePath;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Host, new TigerVNCControl(tabId, sessionInfo), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    public void AddTab(string host)
    {
        _ = Connect(host);
    }

    // Modify history list
    #endregion

    #region Event

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsInfo.TigerVNC_ApplicationFilePath))
            CheckSettings();
    }

    #endregion
}
