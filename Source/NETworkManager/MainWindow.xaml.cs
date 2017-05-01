using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Markup;
using NETworkManager.Views.Applications;
using NETworkManager.Views.Settings;
using NETworkManager.Settings;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;

namespace NETworkManager
{
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables        
        NotifyIcon notifyIcon;

        private bool _isLoading = true;

        private bool _isInTray;
        private bool _closeApplication;

        private bool _pinApplicationList;
        public bool PinApplicationList
        {
            get { return _pinApplicationList; }
            set
            {
                if (value == _pinApplicationList)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Window_PinApplicationList = value;

                    SettingsManager.SettingsChanged = true;
                }

                _pinApplicationList = value;
                OnPropertyChanged("PinApplicationList");
            }
        }

        private bool _isTextBoxSearchFocused;
        public bool IsTextBoxSearchFocused
        {
            get { return _isTextBoxSearchFocused; }
            set
            {
                if (value == _isTextBoxSearchFocused)
                    return;

                _isTextBoxSearchFocused = value;
                OnPropertyChanged("IsTextBoxSearchFocused");
            }
        }

        private bool _openApplicationList;
        public bool OpenApplicationList
        {
            get { return _openApplicationList; }
            set
            {
                if (value == _openApplicationList)
                    return;

                _openApplicationList = value;
                OnPropertyChanged("OpenApplicationList");
            }
        }

        private bool _isMouseOverApplicationList;
        public bool IsMouseOverApplicationList
        {
            get { return _isMouseOverApplicationList; }
            set
            {
                if (value == _isMouseOverApplicationList)
                    return;

                _isMouseOverApplicationList = value;
                OnPropertyChanged("IsMouseOverApplicationList");
            }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText)
                    return;

                _searchText = value;
                _applicationViewCollectionSource.View.Refresh();
                OnPropertyChanged("SearchText");
            }
        }

        private CollectionViewSource _applicationViewCollectionSource;
        public ICollectionView ApplicationViewCollection
        {
            get { return _applicationViewCollectionSource.View; }
        }

        private ApplicationViewInfo _selectedApplicationViewInfo;
        public ApplicationViewInfo SelectedApplicationViewInfo
        {
            get { return _selectedApplicationViewInfo; }
            set
            {
                if (value == _selectedApplicationViewInfo)
                    return;

                if (value != null)
                    ChangeApplicationView(value.Name);

                _selectedApplicationViewInfo = value;
                OnPropertyChanged("SelectedApplicationViewInfo");
            }
        }
        #endregion

        #region Constructor, window load and close events
        public MainWindow()
        {
            // Load localization
            Settings.Localization.Load();
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(Settings.Localization.Culture.IetfLanguageTag)));

            // Load appearance
            Settings.Appearance.Load();

            InitializeComponent();
            DataContext = this;

            // Autostart & Window start
            if (CommandLine.Current.Autostart && NETworkManager.Settings.Properties.Settings.Default.Autostart_StartMinimizedInTray || NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon)
                InitNotifyIcon();

            if (CommandLine.Current.Autostart && NETworkManager.Settings.Properties.Settings.Default.Autostart_StartMinimizedInTray)
                HideWindowToTray();
            else if (NETworkManager.Settings.Properties.Settings.Default.Window_StartMaximized)
                WindowState = WindowState.Maximized;

            // Set windows title if admin
            if (Configuration.Current.IsAdmin)
                Title = string.Format("[{0}] {1}", System.Windows.Application.Current.Resources["String_Administrator"] as string, Title);

            // Load application list, filter, sort
            LoadApplicationList();

            // Load settings
            PinApplicationList = NETworkManager.Settings.Properties.Settings.Default.Window_PinApplicationList;

            _isLoading = false;
        }

        private void LoadApplicationList()
        {
            _applicationViewCollectionSource = new CollectionViewSource();

            // Developer features
            if (NETworkManager.Settings.Properties.Settings.Default.DeveloperMode)
                _applicationViewCollectionSource.Source = ApplicationView.List;
            else
                _applicationViewCollectionSource.Source = ApplicationView.List.Where(x => x.IsDev == false);

            _applicationViewCollectionSource.Filter += ApplicationView_Search;
            _applicationViewCollectionSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void MetroWindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedApplicationViewInfo = ApplicationViewCollection.SourceCollection.Cast<ApplicationViewInfo>().FirstOrDefault(x => x.Name == (ApplicationView.Name)Enum.Parse(typeof(ApplicationView.Name), NETworkManager.Settings.Properties.Settings.Default.Application_DefaultApplicationViewName));
        }

        private async void MetroWindowMain_Closing(object sender, CancelEventArgs e)
        {
            // Hide the application to tray
            if (!_closeApplication && NETworkManager.Settings.Properties.Settings.Default.Window_MinimizeInsteadOfTerminating)
            {
                e.Cancel = true;

                WindowState = WindowState.Minimized;

                return;
            }

            // Confirm close
            if (!_closeApplication && NETworkManager.Settings.Properties.Settings.Default.Window_ConfirmClose)
            {
                e.Cancel = true;

                MetroDialogSettings dialogSettings = new MetroDialogSettings();

                dialogSettings.CustomResourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
                };

                dialogSettings.AffirmativeButtonText = System.Windows.Application.Current.Resources["String_Button_Close"] as string;
                dialogSettings.NegativeButtonText = System.Windows.Application.Current.Resources["String_Button_Cancel"] as string;

                dialogSettings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                if (await this.ShowMessageAsync(System.Windows.Application.Current.Resources["String_Header_Confirm"] as string, System.Windows.Application.Current.Resources["String_ConfirmCloseQuesiton"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings) == MessageDialogResult.Affirmative)
                {
                    _closeApplication = true;
                    Close();
                }

                return;
            }

            // Save settings
            if (SettingsManager.SettingsChanged)
                SettingsManager.SaveSettings();

            // Unregister HotKeys
            if (RegisteredHotKeys.Count > 0)
                UnregisterHotKeys();

            // Dispose the notify icon to prevent errors
            if (notifyIcon != null)
                notifyIcon.Dispose();
        }
        #endregion

        #region Application Views
        NetworkInterfaceUserControl networkInterfaceUserControl;
        IPScannerUserControl ipScannerUserControl;
        PortScannerUserControl portScannerUserControl;
        SubnetCalculatorUserControl subnetCalculatorUserControl;
        WakeOnLANUserControl wakeOnLANUserControl;
        PingUserControl pingUserControl;
        TracerouteUserControl tracerouteUserControl;
        DNSLookupUserControl dnsLookupUserControl;

        private ApplicationView.Name? currentApplicationViewName = null;

        private void ChangeApplicationView(ApplicationView.Name name)
        {
            if (currentApplicationViewName == name)
                return;

            switch (name)
            {
                case ApplicationView.Name.NetworkInterface:
                    if (networkInterfaceUserControl == null)
                        networkInterfaceUserControl = new NetworkInterfaceUserControl();

                    mainContentControl.Content = networkInterfaceUserControl;
                    break;
                case ApplicationView.Name.IPScanner:
                    if (ipScannerUserControl == null)
                        ipScannerUserControl = new IPScannerUserControl();

                    mainContentControl.Content = ipScannerUserControl;
                    break;
                case ApplicationView.Name.PortScanner:
                    if (portScannerUserControl == null)
                        portScannerUserControl = new PortScannerUserControl();

                    mainContentControl.Content = portScannerUserControl;
                    break;
                case ApplicationView.Name.SubnetCalculator:
                    if (subnetCalculatorUserControl == null)
                        subnetCalculatorUserControl = new SubnetCalculatorUserControl();

                    mainContentControl.Content = subnetCalculatorUserControl;
                    break;
                case ApplicationView.Name.WakeOnLAN:
                    if (wakeOnLANUserControl == null)
                        wakeOnLANUserControl = new WakeOnLANUserControl();

                    mainContentControl.Content = wakeOnLANUserControl;
                    break;
                case ApplicationView.Name.Ping:
                    if (pingUserControl == null)
                        pingUserControl = new PingUserControl();

                    mainContentControl.Content = pingUserControl;
                    break;
                case ApplicationView.Name.Traceroute:
                    if (tracerouteUserControl == null)
                        tracerouteUserControl = new TracerouteUserControl();

                    mainContentControl.Content = tracerouteUserControl;
                    break;
                case ApplicationView.Name.DNSLookup:
                    if (dnsLookupUserControl == null)
                        dnsLookupUserControl = new DNSLookupUserControl();

                    mainContentControl.Content = dnsLookupUserControl;
                    break;
            }

            currentApplicationViewName = name;
        }

        #endregion

        #region ListView Serach
        private void ApplicationView_Search(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                e.Accepted = true;
                return;
            }

            // Search for application name and description without "-" and " "
            ApplicationViewInfo info = e.Item as ApplicationViewInfo;

            Regex regex = new Regex(@" |-");

            // Try to find the translated application view name first --> it's faster when the language ist different than english and equal when it's english
            if ((regex.Replace(info.TranslatedName, "").IndexOf(regex.Replace(SearchText, ""), StringComparison.OrdinalIgnoreCase) >= 0) || (regex.Replace(info.Name.ToString(), "").IndexOf(regex.Replace(SearchText, ""), StringComparison.OrdinalIgnoreCase) >= 0))
                e.Accepted = true;
            else
                e.Accepted = false;
        }
        #endregion

        #region HotKeys (Initialized / Register / Unregister / HwndHook)
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int WM_HOTKEY = 0x0312;

        private HwndSource hwndSoure;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Hotkeys
            hwndSoure = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSoure.AddHook(HwndHook);

            RegisterHotKeys();
        }

        /* ID | Command
        *  ---|-------------------
        *  1  | ShowWindowAction()
        */

        List<int> RegisteredHotKeys = new List<int>();

        private void RegisterHotKeys()
        {
            if (NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowEnabled)
            {
                RegisterHotKey(new WindowInteropHelper(this).Handle, 1, NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowModifier, NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowKey);
                RegisteredHotKeys.Add(1);
            }
        }

        private void UnregisterHotKeys()
        {
            // Unregister all registred keys
            foreach (int i in RegisteredHotKeys)
                UnregisterHotKey(new WindowInteropHelper(this).Handle, i);

            // Clear list
            RegisteredHotKeys.Clear();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                switch (wParam.ToInt32())
                {
                    case 1:
                        ShowWindowAction();
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }
        #endregion

        #region NotifyIcon
        private void InitNotifyIcon()
        {
            notifyIcon = new NotifyIcon();

            // Get the application icon for the tray
            using (Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Images/NETworkManager.ico")).Stream)
            {
                notifyIcon.Icon = new Icon(iconStream);
            }

            notifyIcon.Text = Title;
            notifyIcon.DoubleClick += new EventHandler(NotifyIcon_DoubleClick);
            notifyIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(NotifyIcon_MouseDown);
            notifyIcon.Visible = NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon;
        }

        private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Windows.Controls.ContextMenu trayMenu = (System.Windows.Controls.ContextMenu)FindResource("ContextMenuNotifyIcon");
                trayMenu.IsOpen = true;
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindowAction();
        }

        private void MetroWindowMain_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (NETworkManager.Settings.Properties.Settings.Default.Window_MinimizeToTrayInsteadOfTaskbar)
                    HideWindowToTray();
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ContextMenu menu = sender as System.Windows.Controls.ContextMenu;
            menu.DataContext = this;
        }

        private void HideWindowToTray()
        {
            if (notifyIcon == null)
                InitNotifyIcon();

            _isInTray = true;

            notifyIcon.Visible = true;

            Hide();
        }


        private void ShowWindowFromTray()
        {
            _isInTray = false;

            Show();

            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            notifyIcon.Visible = NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon;
        }

        private void BringWindowToFront()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();
        }
        #endregion

        #region Commands & Actions
        public ICommand OpenGithubProjectCommand
        {
            get { return new RelayCommand(p => OpenGithubProjectAction()); }
        }

        private void OpenGithubProjectAction()
        {
            Process.Start(Settings.Properties.Resources.Project_GitHub_Url);
        }

        public ICommand OpenSettingsCommand
        {
            get { return new RelayCommand(p => OpenSettingsAction()); }
        }

        private async void OpenSettingsAction()
        {
            SettingsWindow settingsWindow = new SettingsWindow();

            if (_isInTray)
            {
                settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                settingsWindow.Topmost = true;
            }
            else
            {
                settingsWindow.Owner = this;
            }

            settingsWindow.ShowDialog();

            if (!_isInTray)
            {
                if (NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon && notifyIcon == null)
                    InitNotifyIcon();

                if (notifyIcon != null)
                    notifyIcon.Visible = NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon;
            }

            if (SettingsManager.RestartRequired)
            {
                ShowWindowAction();

                MetroDialogSettings dialogSettings = new MetroDialogSettings();

                dialogSettings.CustomResourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
                };

                dialogSettings.AffirmativeButtonText = System.Windows.Application.Current.Resources["String_Button_RestartNow"] as string;
                dialogSettings.NegativeButtonText = System.Windows.Application.Current.Resources["String_Button_OK"] as string;

                dialogSettings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                if (await this.ShowMessageAsync(System.Windows.Application.Current.Resources["String_RestartRequired"] as string, System.Windows.Application.Current.Resources["String_RestartRequiredAfterSettingsChanged"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings) == MessageDialogResult.Affirmative)
                {
                    RestartApplication();
                    return;
                }
            }

            if (settingsWindow.HotKeysChanged)
            {
                UnregisterHotKeys();
                RegisterHotKeys();
            }
        }

        public ICommand ShowWindowCommand
        {
            get { return new RelayCommand(p => ShowWindowAction()); }
        }

        private void ShowWindowAction()
        {
            if (_isInTray)
                ShowWindowFromTray();
            else
                BringWindowToFront();
        }

        public ICommand CloseApplicationCommand
        {
            get { return new RelayCommand(p => CloseApplicationAction()); }
        }

        private void CloseApplicationAction()
        {
            _closeApplication = true;
            Close();
        }

        private void RestartApplication()
        {
            Process.Start(Assembly.GetExecutingAssembly().Location);

            _closeApplication = true;
            Close();
        }

        public ICommand TextBoxSearchCommand
        {
            get { return new RelayCommand(p => TextBoxSearchAction()); }
        }

        private void TextBoxSearchAction()
        {
            if (string.IsNullOrEmpty(SearchText))
                txtSearch.Focus();
            else
                SearchText = string.Empty;
        }

        public ICommand ApplicationListMouseEnterCommand
        {
            get { return new RelayCommand(p => ApplicationListMouseEnterAction()); }
        }

        private void ApplicationListMouseEnterAction()
        {
            IsMouseOverApplicationList = true;
        }

        public ICommand ApplicationListMouseLeaveCommand
        {
            get { return new RelayCommand(p => ApplicationListMouseLeaveAction()); }
        }

        private void ApplicationListMouseLeaveAction()
        {
            // Don't minmize the list, if the user has accidently mouved the mouse while searching
            if (!IsTextBoxSearchFocused)
                OpenApplicationList = false;

            IsMouseOverApplicationList = false;
        }

        public ICommand TextBoxSearchGotKeyboardFocusCommand
        {
            get { return new RelayCommand(p => TextBoxSearchGotKeyboardFocusAction()); }
        }

        private void TextBoxSearchGotKeyboardFocusAction()
        {
            IsTextBoxSearchFocused = true;
        }

        public ICommand TextBoxSearchLostKeyboardFocusCommand
        {
            get { return new RelayCommand(p => TextBoxSearchLostKeyboardFocusAction()); }
        }

        private void TextBoxSearchLostKeyboardFocusAction()
        {
            if (!IsMouseOverApplicationList)
                OpenApplicationList = false;

            IsTextBoxSearchFocused = false;
        }
        #endregion

        #region Bugfixes
        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
        #endregion
    }
}
