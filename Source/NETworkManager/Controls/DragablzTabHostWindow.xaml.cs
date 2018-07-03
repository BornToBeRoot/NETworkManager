using Dragablz;
using NETworkManager.Models.Settings;
using NETworkManager.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

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
        private readonly ApplicationViewManager.Name _applicationName;

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

        public bool ShowCurrentApplicationTitle => SettingsManager.Current.Window_ShowCurrentApplicationTitle;
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
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            // Switch between application identifiert...
            switch (_applicationName)
            {
                case ApplicationViewManager.Name.IPScanner:
                    ((IPScannerView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    ((PortScannerView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.Ping:
                    ((PingView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    ((TracerouteView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.DNSLookup:
                    ((DNSLookupView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.RemoteDesktop:
                    ((RemoteDesktopControl) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.PuTTY:
                    ((PuttyControl) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.SNMP:
                    ((TracerouteView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
                    break;
                case ApplicationViewManager.Name.HTTPHeaders:
                    ((HTTPHeadersView) ((DragablzTabItem) args.DragablzItem.Content).View).CloseTab();
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