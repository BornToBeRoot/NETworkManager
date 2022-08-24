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
using NETworkManager.Localization.Translators;
using NETworkManager.Models;

namespace NETworkManager.Controls
{
    public partial class DragablzTabHostWindow : INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
        #endregion

        #region Constructor
        public DragablzTabHostWindow(ApplicationName applicationName)
        {
            InitializeComponent();
            DataContext = this;

            ApplicationName = applicationName;

            InterTabClient = new DragablzInterTabClient(applicationName);

            InterTabController.Partition = applicationName.ToString();

            Title = $"NETworkManager {AssemblyManager.Current.Version.Major}.{AssemblyManager.Current.Version.Minor}.{AssemblyManager.Current.Version.Build} - {ApplicationNameTranslator.GetInstance().Translate(applicationName)}";

            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            // Switch between application identifiert...
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
                case ApplicationName.TigerVNC:
                    ((TigerVNCControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationName.WebConsole:
                    ((WebConsoleControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationName.SNMP:
                    ((SNMPView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationName.Whois:
                    ((WhoisView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region RemoteDesktop Commands
        private bool RemoteDesktop_Disconnected_CanExecute(object view)
        {
            if (view is RemoteDesktopControl control)
                return !control.IsConnected;

            return false;
        }

        private bool RemoteDesktop_Connected_CanExecute(object view)
        {
            if (view is RemoteDesktopControl control)
                return control.IsConnected;

            return false;
        }

        public ICommand RemoteDesktop_ReconnectCommand => new RelayCommand(RemoteDesktop_ReconnectAction, RemoteDesktop_Disconnected_CanExecute);

        private void RemoteDesktop_ReconnectAction(object view)
        {
            if (view is RemoteDesktopControl control)
            {
                if (control.ReconnectCommand.CanExecute(null))
                    control.ReconnectCommand.Execute(null);
            }
        }

        public ICommand RemoteDesktop_DisconnectCommand => new RelayCommand(RemoteDesktop_DisconnectAction, RemoteDesktop_Connected_CanExecute);

        private void RemoteDesktop_DisconnectAction(object view)
        {
            if (view is RemoteDesktopControl control)
            {
                if (control.DisconnectCommand.CanExecute(null))
                    control.DisconnectCommand.Execute(null);
            }
        }

        public ICommand RemoteDesktop_FullscreenCommand => new RelayCommand(RemoteDesktop_FullscreenAction, RemoteDesktop_Connected_CanExecute);

        private void RemoteDesktop_FullscreenAction(object view)
        {
            if (view is RemoteDesktopControl control)
                control.FullScreen();
        }

        public ICommand RemoteDesktop_AdjustScreenCommand => new RelayCommand(RemoteDesktop_AdjustScreenAction, RemoteDesktop_Connected_CanExecute);

        private void RemoteDesktop_AdjustScreenAction(object view)
        {
            if (view is RemoteDesktopControl control)
                control.AdjustScreen();
        }

        public ICommand RemoteDesktop_SendCtrlAltDelCommand => new RelayCommand(RemoteDesktop_SendCtrlAltDelAction, RemoteDesktop_Connected_CanExecute);

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
                    ConfigurationManager.Current.FixAirspace = true;

                    await this.ShowMessageAsync(Localization.Resources.Strings.Error, string.Format("{0}\n\nMessage:\n{1}", NETworkManager.Localization.Resources.Strings.CouldNotSendKeystroke, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog));

                    ConfigurationManager.Current.FixAirspace = false;
                }
            }
        }
        #endregion

        #region PowerShell Commands
        private bool PowerShell_Disconnected_CanExecute(object view)
        {
            if (view is PowerShellControl control)
                return !control.IsConnected;

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

        public ICommand PowerShell_ResizeWindowCommand => new RelayCommand(PowerShell_ResizeWindowAction, PowerShell_Disconnected_CanExecute);

        private void PowerShell_ResizeWindowAction(object view)
        {
            if (view is PowerShellControl control)
                control.ResizeEmbeddedWindow();
        }
        #endregion

        #region PuTTY Commands
        private bool PuTTY_Connected_CanExecute(object view)
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

        public ICommand PuTTY_ResizeWindowCommand => new RelayCommand(PuTTY_ResizeWindowAction, PuTTY_Connected_CanExecute);

        private void PuTTY_ResizeWindowAction(object view)
        {
            if (view is PuTTYControl control)
                control.ResizeEmbeddedWindow();
        }

        public ICommand PuTTY_RestartSessionCommand => new RelayCommand(PuTTY_RestartSessionAction, PuTTY_Connected_CanExecute);

        private void PuTTY_RestartSessionAction(object view)
        {
            if (view is PuTTYControl control)
                control.RestartSession();
        }
        #endregion

        #region TigerVNC Commands
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

        #region WebConsole Commands
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

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
        #endregion

        private void MetroWindow_Activated(object sender, EventArgs e)
        {
            
        }
    }
}