using System;
using Dragablz;
using NETworkManager.Settings;
using NETworkManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NETworkManager.Utilities;
using NETworkManager.Models.RemoteDesktop;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models;
using System.Threading.Tasks;
using System.Windows.Forms;
using NETworkManager.Localization;

namespace NETworkManager.Controls;

public sealed partial class DragablzTabHostWindow : INotifyPropertyChanged
{
    #region PropertyChangedEventHandler
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Variables
    public IInterTabClient InterTabClient { get; }
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

    #region Constructor
    public DragablzTabHostWindow(ApplicationName applicationName)
    {
        InitializeComponent();
        DataContext = this;

        ApplicationName = applicationName;

        InterTabClient = new DragablzInterTabClient(applicationName);

        InterTabController.Partition = applicationName.ToString();

        Title = $"NETworkManager {AssemblyManager.Current.Version} - {ResourceTranslator.Translate(ResourceIdentifier.ApplicationName, applicationName)}";

        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }
    #endregion

    #region ICommand & Actions
    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        // Switch between application identifiers...
        switch (_applicationName)
        {
            case ApplicationName.None:
                break;
            case ApplicationName.IPScanner:
                ((IPScannerView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.PortScanner:
                ((PortScannerView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.Traceroute:
                ((TracerouteView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.DNSLookup:
                ((DNSLookupView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.RemoteDesktop:
                ((RemoteDesktopControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.PowerShell:
                ((PowerShellControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.PuTTY:
                ((PuTTYControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.AWSSessionManager:
                ((AWSSessionManagerControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.TigerVNC:
                ((TigerVNCControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.WebConsole:
                ((WebConsoleControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.SNMP:
                ((SNMPView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.SNTPLookup:
                ((SNTPLookupView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.Whois:
                ((WhoisView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                break;
            case ApplicationName.Dashboard:
            case ApplicationName.NetworkInterface:
            case ApplicationName.WiFi:
            case ApplicationName.PingMonitor:
            case ApplicationName.DiscoveryProtocol:
            case ApplicationName.WakeOnLAN:
            case ApplicationName.SubnetCalculator:
            case ApplicationName.BitCalculator:
            case ApplicationName.Lookup:
            case ApplicationName.Connections:
            case ApplicationName.Listeners:
            case ApplicationName.ARPTable:
            default:
                break;
        }
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

    public ICommand RemoteDesktop_DisconnectCommand => new RelayCommand(RemoteDesktop_DisconnectAction, RemoteDesktop_IsConnected_CanExecute);

    private void RemoteDesktop_DisconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
        {
            if (control.DisconnectCommand.CanExecute(null))
                control.DisconnectCommand.Execute(null);
        }
    }

    public ICommand RemoteDesktop_ReconnectCommand => new RelayCommand(RemoteDesktop_ReconnectAction, RemoteDesktop_IsDisconnected_CanExecute);

    private void RemoteDesktop_ReconnectAction(object view)
    {
        if (view is RemoteDesktopControl control)
        {
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
        }
    }

    public ICommand RemoteDesktop_FullscreenCommand => new RelayCommand(RemoteDesktop_FullscreenAction, RemoteDesktop_IsConnected_CanExecute);

    private void RemoteDesktop_FullscreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.FullScreen();
    }

    public ICommand RemoteDesktop_AdjustScreenCommand => new RelayCommand(RemoteDesktop_AdjustScreenAction, RemoteDesktop_IsConnected_CanExecute);

    private void RemoteDesktop_AdjustScreenAction(object view)
    {
        if (view is RemoteDesktopControl control)
            control.AdjustScreen();
    }

    public ICommand RemoteDesktop_SendCtrlAltDelCommand => new RelayCommand(RemoteDesktop_SendCtrlAltDelAction, RemoteDesktop_IsConnected_CanExecute);

    private async void RemoteDesktop_SendCtrlAltDelAction(object view)
    {
        if (view is RemoteDesktopControl control)
        {
            try
            {
                control.SendKey(Keystroke.CtrlAltDel);
            }
            catch (Exception ex)
            {
                ConfigurationManager.OnDialogOpen();

                await this.ShowMessageAsync(Localization.Resources.Strings.Error, string.Format("{0}\n\nMessage:\n{1}", NETworkManager.Localization.Resources.Strings.CouldNotSendKeystroke, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog));

                ConfigurationManager.OnDialogClose();
            }
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
        {
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
        }
    }

    public ICommand PowerShell_ResizeWindowCommand => new RelayCommand(PowerShell_ResizeWindowAction, PowerShell_IsConnected_CanExecute);

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
        {
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
        }
    }

    public ICommand PuTTY_ResizeWindowCommand => new RelayCommand(PuTTY_ResizeWindowAction, PuTTY_IsConnected_CanExecute);

    private void PuTTY_ResizeWindowAction(object view)
    {
        if (view is PuTTYControl control)
            control.ResizeEmbeddedWindow();
    }

    public ICommand PuTTY_RestartSessionCommand => new RelayCommand(PuTTY_RestartSessionAction, PuTTY_IsConnected_CanExecute);

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
        {
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
        }
    }

    public ICommand AWSSessionManager_ResizeWindowCommand => new RelayCommand(AWSSessionManager_ResizeWindowAction, AWSSessionManager_IsConnected_CanExecute);

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
        {
            if (control.ReconnectCommand.CanExecute(null))
                control.ReconnectCommand.Execute(null);
        }
    }
    #endregion

    #region WebConsole commands
    public ICommand WebConsole_ReloadCommand => new RelayCommand(WebConsole_RefreshAction);

    private void WebConsole_RefreshAction(object view)
    {
        if (view is WebConsoleControl control)
        {
            if (control.ReloadCommand.CanExecute(null))
                control.ReloadCommand.Execute(null);
        }
    }
    #endregion
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

        // Switch by name
        switch (ApplicationName)
        {
            case ApplicationName.PowerShell:
                ((PowerShellControl)((DragablzTabItem)TabsContainer?.SelectedItem)?.View)?.FocusEmbeddedWindow();
                break;
            case ApplicationName.PuTTY:
                ((PuTTYControl)((DragablzTabItem)TabsContainer?.SelectedItem)?.View)?.FocusEmbeddedWindow();
                break;
            case ApplicationName.AWSSessionManager:
                ((AWSSessionManagerControl)((DragablzTabItem)TabsContainer?.SelectedItem)?.View)?.FocusEmbeddedWindow();
                break;
        }
    }
    #endregion

    #region Events
    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {

    }

    private void MetroWindow_Activated(object sender, EventArgs e)
    {
        FocusEmbeddedWindow();
    }
    #endregion
}