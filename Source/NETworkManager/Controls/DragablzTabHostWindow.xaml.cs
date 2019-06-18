using System;
using Dragablz;
using NETworkManager.Models.Settings;
using NETworkManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NETworkManager.Utilities;

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
        private ApplicationViewManager.Name _applicationName;

        public ApplicationViewManager.Name ApplicationName
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

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get => _applicationTitle;
            set
            {
                if (value == _applicationTitle)
                    return;

                _applicationTitle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public DragablzTabHostWindow(ApplicationViewManager.Name applicationName)
        {
            InitializeComponent();
            DataContext = this;

            // Transparency
            if (SettingsManager.Current.Appearance_EnableTransparency)
            {
                AllowsTransparency = true;
                Opacity = SettingsManager.Current.Appearance_Opacity;
            }

            ApplicationName = applicationName;

            InterTabClient = new DragablzInterTabClient(applicationName);

            InterTabController.Partition = applicationName.ToString();

            ApplicationTitle = ApplicationViewManager.GetTranslatedNameByName(applicationName);

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
                case ApplicationViewManager.Name.None:
                    break;
                case ApplicationViewManager.Name.IPScanner:
                    ((IPScannerView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    ((PortScannerView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.Ping:
                    ((PingView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    ((TracerouteView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.DNSLookup:
                    ((DNSLookupView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.RemoteDesktop:
                    ((RemoteDesktopControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.PowerShell:
                    ((PowerShellControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.PuTTY:
                    ((PuTTYControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.TigerVNC:
                    ((TigerVNCControl)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.SNMP:
                    ((SNMPView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.HTTPHeaders:
                    ((HTTPHeadersView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.Whois:
                    ((WhoisView)((DragablzTabItem)args.DragablzItem.Content).View).CloseTab();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region PowerShell Commands
        public ICommand PowerShell_ReconnectCommand => new RelayCommand(PowerShell_ReconnectAction);

        private void PowerShell_ReconnectAction(object view)
        {
            if (view is PowerShellControl control)
            {
                if (control.ReconnectCommand.CanExecute(null))
                    control.ReconnectCommand.Execute(null);
            }
        }

        #endregion

        #region PuTTY Commands
        public ICommand PuTTY_ReconnectCommand => new RelayCommand(PuTTY_ReconnectAction);

        private void PuTTY_ReconnectAction(object view)
        {
            if (view is PuTTYControl puttyControl)
            {
                if (puttyControl.ReconnectCommand.CanExecute(null))
                    puttyControl.ReconnectCommand.Execute(null);
            }
        }

        public ICommand PuTTY_RestartSessionCommand => new RelayCommand(PuTTY_RestartSessionAction);

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
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Window helper
        // Move the window when the user hold the title...
        private void HeaderBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        #endregion 
    }
}