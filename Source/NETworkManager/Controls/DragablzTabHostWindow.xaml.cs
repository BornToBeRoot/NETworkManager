using Dragablz;
using MahApps.Metro.Controls;
using NETworkManager.Models.Settings;
using NETworkManager.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NETworkManager.Controls
{
    public partial class DragablzTabHostWindow : MetroWindow, INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        public IInterTabClient InterTabClient { get; private set; }
        private ApplicationViewManager.Name _applicationName;

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get { return _applicationTitle; }
            set
            {
                if (value == _applicationTitle)
                    return;

                _applicationTitle = value;
                OnPropertyChanged();
            }
        }

        public bool ShowCurrentApplicationTitle
        {
            get { return SettingsManager.Current.Window_ShowCurrentApplicationTitle; }
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

            _applicationName = applicationName;

            InterTabClient = new DragablzInterTabClient(applicationName);

            InterTabController.Partition = applicationName.ToString();

            ApplicationTitle = ApplicationViewManager.GetTranslatedNameByName(applicationName);

            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            // Switch between application identifiert...
            switch (_applicationName)
            {
                case ApplicationViewManager.Name.IPScanner:
                    ((args.DragablzItem.Content as DragablzTabItem).View as IPScannerView).CloseTab();
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    ((args.DragablzItem.Content as DragablzTabItem).View as PortScannerView).CloseTab();
                    break;
                case ApplicationViewManager.Name.Ping:
                    ((args.DragablzItem.Content as DragablzTabItem).View as PingView).CloseTab();
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    ((args.DragablzItem.Content as DragablzTabItem).View as TracerouteView).CloseTab();
                    break;
                case ApplicationViewManager.Name.DNSLookup:
                    ((args.DragablzItem.Content as DragablzTabItem).View as DNSLookupView).CloseTab();
                    break;
                case ApplicationViewManager.Name.RemoteDesktop:
                    ((args.DragablzItem.Content as DragablzTabItem).View as RemoteDesktopControl).CloseTab();
                    break;
                case ApplicationViewManager.Name.PuTTY:
                    ((args.DragablzItem.Content as DragablzTabItem).View as PuTTYControl).CloseTab();
                    break;
                case ApplicationViewManager.Name.SNMP:
                    ((args.DragablzItem.Content as DragablzTabItem).View as TracerouteView).CloseTab();
                    break;
                case ApplicationViewManager.Name.HTTPHeaders:
                    ((args.DragablzItem.Content as DragablzTabItem).View as HTTPHeadersView).CloseTab();
                    break;
            }

        }
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.Window_ShowCurrentApplicationTitle))
            {
                OnPropertyChanged(nameof(ShowCurrentApplicationTitle));
                Debug.WriteLine(e.PropertyName);
            }

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