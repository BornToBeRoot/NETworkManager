using Dragablz;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RemoteDesktop = NETworkManager.Profiles.Application.RemoteDesktop;

namespace NETworkManager.ViewModels;

public class RemoteDesktopHostViewModel : ProfileHostViewModelBase
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

    public RemoteDesktopHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.RemoteDesktop);
        InterTabPartition = nameof(ApplicationName.RemoteDesktop);

        TabItems = [];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.RemoteDesktop;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.RemoteDesktop_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.RemoteDesktop_Host;

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

    public ICommand ConnectCommand => new RelayCommand(_ => ConnectAction());

    private void ConnectAction()
    {
        _ = Connect();
    }

    private bool IsConnected_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return control.IsConnected;

        return false;
    }

    private bool IsDisconnected_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return !control.IsConnected && !control.IsConnecting;

        return false;
    }

    // Used to block actions that would bypass view-only mode (e.g. fullscreen, Ctrl+Alt+Del).
    private bool IsConnectedAndNotViewOnly_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return control.IsConnected && !control.IsViewOnly;

        return false;
    }

    public ICommand ReconnectCommand => new RelayCommand(ReconnectAction, IsDisconnected_CanExecute);

    private void ReconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand DisconnectCommand => new RelayCommand(DisconnectAction, IsConnected_CanExecute);

    private void DisconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
            if (control.DisconnectCommand.CanExecute(null))
                control.DisconnectCommand.Execute(null);
    }

    public ICommand FullscreenCommand => new RelayCommand(FullscreenAction, IsConnectedAndNotViewOnly_CanExecute);

    private void FullscreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.FullScreen();
    }

    public ICommand ViewOnlyCommand => new RelayCommand(ViewOnlyAction, IsConnected_CanExecute);

    private void ViewOnlyAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.ToggleViewOnly();
    }

    public ICommand AdjustScreenCommand => new RelayCommand(AdjustScreenAction, IsConnected_CanExecute);

    private void AdjustScreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.AdjustScreen(force: true);
    }

    public ICommand SendCtrlAltDelCommand =>
        new RelayCommand(view => SendKeyAction(view, Keystroke.CtrlAltDel), IsConnectedAndNotViewOnly_CanExecute);

    public ICommand SendTaskManagerCommand =>
        new RelayCommand(view => SendKeyAction(view, Keystroke.TaskManager), IsConnectedAndNotViewOnly_CanExecute);

    public ICommand SendLockCommand =>
        new RelayCommand(view => SendKeyAction(view, Keystroke.Lock), IsConnectedAndNotViewOnly_CanExecute);

    public ICommand SendShowDesktopCommand =>
        new RelayCommand(view => SendKeyAction(view, Keystroke.ShowDesktop), IsConnectedAndNotViewOnly_CanExecute);

    public ICommand SendExplorerCommand =>
        new RelayCommand(view => SendKeyAction(view, Keystroke.Explorer), IsConnectedAndNotViewOnly_CanExecute);

    public ICommand SendRunDialogCommand =>
        new RelayCommand(view => SendKeyAction(view, Keystroke.RunDialog), IsConnectedAndNotViewOnly_CanExecute);

    private async void SendKeyAction(object view, Keystroke keystroke)
    {
        if (view is not RemoteDesktopControl control)
            return;

        try
        {
            control.SendKey(keystroke);
        }
        catch (Exception ex)
        {
            ConfigurationManager.OnDialogOpen();

            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                 $"{Strings.CouldNotSendKeystroke}\n\nMessage:\n{ex.Message}", ChildWindowIcon.Error);

            ConfigurationManager.OnDialogClose();
        }
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

    public ICommand ConnectProfileAsCommand => new RelayCommand(_ => ConnectProfileAsAction());

    private void ConnectProfileAsAction()
    {
        _ = ConnectProfileAs();
    }

    public ICommand ConnectProfileExternalCommand => new RelayCommand(_ => ConnectProfileExternalAction());

    private void ConnectProfileExternalAction()
    {
        var args = new List<string>();

        if (SelectedProfile.RemoteDesktop_AdminSession)
            args.Add("/admin");

        args.Add($"/V:{SelectedProfile.RemoteDesktop_Host}");

        Process.Start("mstsc.exe", args);
    }

    #endregion

    #region Methods

    // Connect via Dialog
    private Task Connect(string host = null)
    {
        var childWindow = new RemoteDesktopConnectChildWindow();

        var childWindowViewModel = new RemoteDesktopConnectViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            // Create new session info with default settings
            var sessionInfo = RemoteDesktop.CreateSessionInfo();

            if (instance.Host.Contains(':'))
            {
                // Validate input via UI
                sessionInfo.Hostname = instance.Host.Split(':')[0];
                sessionInfo.Port = int.Parse(instance.Host.Split(':')[1]);
            }
            else
            {
                sessionInfo.Hostname = instance.Host;
            }

            if (instance.UseCredentials)
            {
                sessionInfo.UseCredentials = true;

                sessionInfo.Username = instance.Username;
                sessionInfo.Domain = instance.Domain;
                sessionInfo.Password = instance.Password;
            }

            sessionInfo.AdminSession = instance.AdminSession;
            sessionInfo.ViewOnly = instance.ViewOnly;

            // Add to history
            // Note: The history can only be updated after the values have been read.
            //       Otherwise, in some cases, incorrect values are taken over.
            AddHostToHistory(instance.Host);

            Connect(sessionInfo);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();
        })
        {
            Host = host
        };

        childWindow.Title = Strings.Connect;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        ConfigurationManager.OnDialogOpen();

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    // Connect via Profile
    private void ConnectProfile()
    {
        var profileInfo = SelectedProfile;

        var sessionInfo = RemoteDesktop.CreateSessionInfo(profileInfo);

        Connect(sessionInfo, profileInfo.Name);
    }

    // Connect via Profile with Credentials
    private Task ConnectProfileAs()
    {
        var profileInfo = SelectedProfile;

        var sessionInfo = RemoteDesktop.CreateSessionInfo(profileInfo);

        var childWindow = new RemoteDesktopConnectChildWindow();

        var childWindowViewModel = new RemoteDesktopConnectViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();

            if (instance.UseCredentials)
            {
                sessionInfo.UseCredentials = true;

                sessionInfo.Username = instance.Username;
                sessionInfo.Domain = instance.Domain;
                sessionInfo.Password = instance.Password;
            }

            sessionInfo.AdminSession = instance.AdminSession;
            sessionInfo.ViewOnly = instance.ViewOnly;

            Connect(sessionInfo, instance.Name);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            ConfigurationManager.OnDialogClose();
        },
        (
            profileInfo.Name,
            profileInfo.RemoteDesktop_Host,
            profileInfo.RemoteDesktop_AdminSession,
            sessionInfo.ViewOnly
        ));

        childWindow.Title = Strings.ConnectAs;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        ConfigurationManager.OnDialogOpen();

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private void Connect(RemoteDesktopSessionInfo sessionInfo, string header = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Hostname, new RemoteDesktopControl(tabId, sessionInfo),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    public void AddTab(string host)
    {
        _ = Connect(host);
    }

    // Modify history list
    private static void AddHostToHistory(string host)
    {
        if (string.IsNullOrEmpty(host))
            return;

        SettingsManager.Current.RemoteDesktop_HostHistory = new ObservableCollection<string>(
            ListHelper.Modify(SettingsManager.Current.RemoteDesktop_HostHistory.ToList(), host,
                SettingsManager.Current.General_HistoryListEntries));
    }

    #endregion
}
