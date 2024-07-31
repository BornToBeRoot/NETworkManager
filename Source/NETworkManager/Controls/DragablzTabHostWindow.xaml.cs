using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using Application = System.Windows.Application;

namespace NETworkManager.Controls;

public sealed partial class DragablzTabHostWindow : INotifyPropertyChanged
{
    #region Constructor

    public DragablzTabHostWindow(ApplicationName applicationName)
    {
        InitializeComponent();
        DataContext = this;

        ApplicationName = applicationName;

        InterTabClient = new DragablzInterTabClient(applicationName);
        InterTabPartition = applicationName.ToString();

        Title =
            $"NETworkManager {AssemblyManager.Current.Version} - {ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, applicationName)}";
    }

    #endregion

    #region Methods

    private async void FocusEmbeddedWindow()
    {
        // Delay the focus to prevent blocking the ui
        // Detect if window is resizing
        do
        {
            await Task.Delay(250);
        } while (Control.MouseButtons == MouseButtons.Left);

        /* Don't continue if
           - Header ContextMenu is opened
        */
        if (HeaderContextMenuIsOpen)
            return;

        // Return if the application is not an embedded window
        if (!_embeddedWindowApplicationNames.Contains(ApplicationName))
            return;

        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window == null)
            return;

        // Find all TabablzControl in the active window
        foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
        {
            // Skip if no items
            if (tabablzControl.Items.Count == 0)
                continue;

            // Focus embedded window in the selected tab
            (((DragablzTabItem)tabablzControl.SelectedItem)?.View as IEmbeddedWindow)?.FocusEmbeddedWindow();

            break;
        }
    }

    #endregion

    #region PropertyChangedEventHandler

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Variables

    public IInterTabClient InterTabClient { get; }

    private readonly HashSet<ApplicationName> _embeddedWindowApplicationNames =
    [
        ApplicationName.PowerShell,
        ApplicationName.PuTTY,
        ApplicationName.AWSSessionManager
    ];

    private ApplicationName _applicationName;

    public ApplicationName ApplicationName
    {
        get => _applicationName;
        set
        {
            if (value == _applicationName)
                return;

            _applicationName = value;
            OnPropertyChanged();
        }
    }

    private string _interTabPartition;

    public string InterTabPartition
    {
        get => _interTabPartition;
        set
        {
            if (value == _interTabPartition)
                return;

            _interTabPartition = value;
            OnPropertyChanged();
        }
    }

    private bool _headerContextMenuIsOpen;

    public bool HeaderContextMenuIsOpen
    {
        get => _headerContextMenuIsOpen;
        set
        {
            if (value == _headerContextMenuIsOpen)
                return;

            _headerContextMenuIsOpen = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommand & Actions

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((IDragablzTabItem)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
    }

    #region RemoteDesktop commands

    private bool RemoteDesktop_IsConnected_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return control.IsConnected;

        return false;
    }

    private bool RemoteDesktop_IsDisconnected_CanExecute(object view)
    {
        if (view is RemoteDesktopControl control)
            return !control.IsConnected;

        return false;
    }

    public ICommand RemoteDesktop_DisconnectCommand =>
        new RelayCommand(RemoteDesktop_DisconnectAction, RemoteDesktop_IsConnected_CanExecute);

    private void RemoteDesktop_DisconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
            if (control.DisconnectCommand.CanExecute(null))
                control.DisconnectCommand.Execute(null);
    }

    public ICommand RemoteDesktop_ReconnectCommand =>
        new RelayCommand(RemoteDesktop_ReconnectAction, RemoteDesktop_IsDisconnected_CanExecute);

    private void RemoteDesktop_ReconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand RemoteDesktop_FullscreenCommand =>
        new RelayCommand(RemoteDesktop_FullscreenAction, RemoteDesktop_IsConnected_CanExecute);

    private void RemoteDesktop_FullscreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.FullScreen();
    }

    public ICommand RemoteDesktop_AdjustScreenCommand =>
        new RelayCommand(RemoteDesktop_AdjustScreenAction, RemoteDesktop_IsConnected_CanExecute);

    private void RemoteDesktop_AdjustScreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.AdjustScreen();
    }

    public ICommand RemoteDesktop_SendCtrlAltDelCommand =>
        new RelayCommand(RemoteDesktop_SendCtrlAltDelAction, RemoteDesktop_IsConnected_CanExecute);

    private async void RemoteDesktop_SendCtrlAltDelAction(object view)
    {
        if (view is not RemoteDesktopControl control)
            return;

        try
        {
            control.SendKey(Keystroke.CtrlAltDel);
        }
        catch (Exception ex)
        {
            ConfigurationManager.OnDialogOpen();

            await this.ShowMessageAsync(Strings.Error,
                string.Format("{0}\n\nMessage:\n{1}",
                    Strings.CouldNotSendKeystroke, ex.Message));

            ConfigurationManager.OnDialogClose();
        }
    }

    #endregion

    #region PowerShell commands

    private bool PowerShell_IsConnected_CanExecute(object view)
    {
        if (view is PowerShellControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand PowerShell_ReconnectCommand => new RelayCommand(PowerShell_ReconnectAction);

    private void PowerShell_ReconnectAction(object view)
    {
        if (view is PowerShellControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand PowerShell_ResizeWindowCommand =>
        new RelayCommand(PowerShell_ResizeWindowAction, PowerShell_IsConnected_CanExecute);

    private void PowerShell_ResizeWindowAction(object view)
    {
        if (view is PowerShellControl control)
            control.ResizeEmbeddedWindow();
    }

    #endregion

    #region PuTTY commands

    private bool PuTTY_IsConnected_CanExecute(object view)
    {
        if (view is PuTTYControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand PuTTY_ReconnectCommand => new RelayCommand(PuTTY_ReconnectAction);

    private void PuTTY_ReconnectAction(object view)
    {
        if (view is PuTTYControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand PuTTY_ResizeWindowCommand =>
        new RelayCommand(PuTTY_ResizeWindowAction, PuTTY_IsConnected_CanExecute);

    private void PuTTY_ResizeWindowAction(object view)
    {
        if (view is PuTTYControl control)
            control.ResizeEmbeddedWindow();
    }

    public ICommand PuTTY_RestartSessionCommand =>
        new RelayCommand(PuTTY_RestartSessionAction, PuTTY_IsConnected_CanExecute);

    private void PuTTY_RestartSessionAction(object view)
    {
        if (view is PuTTYControl control)
            control.RestartSession();
    }

    #endregion

    #region AWSSessionManager commands

    private bool AWSSessionManager_IsConnected_CanExecute(object view)
    {
        if (view is AWSSessionManagerControl control)
            return control.IsConnected;

        return false;
    }

    public ICommand AWSSessionManager_ReconnectCommand => new RelayCommand(AWSSessionManager_ReconnectAction);

    private void AWSSessionManager_ReconnectAction(object view)
    {
        if (view is AWSSessionManagerControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    public ICommand AWSSessionManager_ResizeWindowCommand => new RelayCommand(AWSSessionManager_ResizeWindowAction,
        AWSSessionManager_IsConnected_CanExecute);

    private void AWSSessionManager_ResizeWindowAction(object view)
    {
        if (view is AWSSessionManagerControl control)
            control.ResizeEmbeddedWindow();
    }

    #endregion

    #region TigerVNC commands

    public ICommand TigerVNC_ReconnectCommand => new RelayCommand(TigerVNC_ReconnectAction);

    private void TigerVNC_ReconnectAction(object view)
    {
        if (view is TigerVNCControl control)
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
    }

    #endregion

    #region WebConsole commands

    public ICommand WebConsole_ReloadCommand => new RelayCommand(WebConsole_RefreshAction);

    private void WebConsole_RefreshAction(object view)
    {
        if (view is WebConsoleControl control)
            if (control.ReloadCommand.CanExecute(null))
                control.ReloadCommand.Execute(null);
    }

    #endregion

    #endregion

    #region Events

    private void MetroWindow_Activated(object sender, EventArgs e)
    {
        FocusEmbeddedWindow();
    }

    private void DragablzTabHostWindow_OnClosing(object sender, CancelEventArgs e)
    {
        // Close all tabs properly when the window is closing
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window == null)
            return;

        // Find all TabablzControl in the active window
        foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
        foreach (var tabItem in tabablzControl.Items.OfType<DragablzTabItem>())
            ((IDragablzTabItem)tabItem.View).CloseTab();

        // Reset the dragging state
        switch (ApplicationName)
        {
            case ApplicationName.RemoteDesktop:
                ConfigurationManager.Current.IsRemoteDesktopWindowDragging = false;
                break;
            case ApplicationName.PowerShell:
                ConfigurationManager.Current.IsPowerShellWindowDragging = false;
                break;
            case ApplicationName.PuTTY:
                ConfigurationManager.Current.IsPuTTYWindowDragging = false;
                break;
            case ApplicationName.AWSSessionManager:
                ConfigurationManager.Current.IsAWSSessionManagerWindowDragging = false;
                break;
            case ApplicationName.TigerVNC:
                ConfigurationManager.Current.IsTigerVNCWindowDragging = false;
                break;
            case ApplicationName.WebConsole:
                ConfigurationManager.Current.IsWebConsoleWindowDragging = false;
                break;
        }
    }

    private void TabablzControl_OnIsDraggingWindowChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
    {
        // Set the dragging state
        switch (ApplicationName)
        {
            case ApplicationName.RemoteDesktop:
                ConfigurationManager.Current.IsRemoteDesktopWindowDragging = e.NewValue;
                break;
            case ApplicationName.PowerShell:
                ConfigurationManager.Current.IsPowerShellWindowDragging = e.NewValue;
                break;
            case ApplicationName.PuTTY:
                ConfigurationManager.Current.IsPuTTYWindowDragging = e.NewValue;
                break;
            case ApplicationName.AWSSessionManager:
                ConfigurationManager.Current.IsAWSSessionManagerWindowDragging = e.NewValue;
                break;
            case ApplicationName.TigerVNC:
                ConfigurationManager.Current.IsTigerVNCWindowDragging = e.NewValue;
                break;
            case ApplicationName.WebConsole:
                ConfigurationManager.Current.IsWebConsoleWindowDragging = e.NewValue;
                break;
        }
    }

    #endregion
}