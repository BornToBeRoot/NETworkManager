using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Markup;
using NETworkManager.Views.Applications;
using NETworkManager.Views.Settings;
using NETworkManager.Models.Settings;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;
using NETworkManager.Views;

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

        private bool _applicationView_Expand;
        public bool ApplicationView_Expand
        {
            get { return _applicationView_Expand; }
            set
            {
                if (value == _applicationView_Expand)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.ApplicationView_Expand = value;

                _applicationView_Expand = value;
                OnPropertyChanged("ApplicationView_Expand");
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
            InitializeComponent();
            DataContext = this;

            // Load settings
            SettingsManager.Load();

            // Load localization
            LocalizationManager.Load();
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(LocalizationManager.Culture.IetfLanguageTag)));

            // Load appearance
            AppearanceManager.Load();

            // Autostart & Window start
            if (CommandLineManager.Current.Autostart && SettingsManager.Current.Autostart_StartMinimizedInTray || SettingsManager.Current.TrayIcon_AlwaysShowIcon)
                InitNotifyIcon();

            if (CommandLineManager.Current.Autostart && SettingsManager.Current.Autostart_StartMinimizedInTray)
                HideWindowToTray();
            else if (SettingsManager.Current.Window_StartMaximized)
                WindowState = WindowState.Maximized;

            // Set windows title if admin
            if (ConfigurationManager.Current.IsAdmin)
                Title = string.Format("[{0}] {1}", System.Windows.Application.Current.Resources["String_Administrator"] as string, Title);

            // Load application list, filter, sort
            LoadApplicationList();

            // Load settings
            ApplicationView_Expand = SettingsManager.Current.ApplicationView_Expand;

            // Load templates
            TemplateManager.LoadNetworkInterfaceConfigTemplates();
            TemplateManager.LoadWakeOnLANTemplates();

            _isLoading = false;
        }

        private void LoadApplicationList()
        {
            _applicationViewCollectionSource = new CollectionViewSource();

            // Developer features
            if (SettingsManager.Current.DeveloperMode)
                _applicationViewCollectionSource.Source = ApplicationView.List;
            else
                _applicationViewCollectionSource.Source = ApplicationView.List.Where(x => x.IsDev == false);

            _applicationViewCollectionSource.Filter += ApplicationView_Search;
            _applicationViewCollectionSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void MetroWindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedApplicationViewInfo = ApplicationViewCollection.SourceCollection.Cast<ApplicationViewInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.Application_DefaultApplicationViewName);
        }

        private async void MetroWindowMain_Closing(object sender, CancelEventArgs e)
        {
            // Hide the application to tray
            if (!_closeApplication && SettingsManager.Current.Window_MinimizeInsteadOfTerminating)
            {
                e.Cancel = true;

                WindowState = WindowState.Minimized;

                return;
            }

            // Confirm close
            if (!_closeApplication && SettingsManager.Current.Window_ConfirmClose)
            {
                e.Cancel = true;

                MetroDialogSettings dialogSettings = new MetroDialogSettings()
                {
                    CustomResourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
                    },

                    AffirmativeButtonText = System.Windows.Application.Current.Resources["String_Button_Close"] as string,
                    NegativeButtonText = System.Windows.Application.Current.Resources["String_Button_Cancel"] as string,

                    DefaultButtonFocus = MessageDialogResult.Affirmative
                };

                if (await this.ShowMessageAsync(System.Windows.Application.Current.Resources["String_Header_Confirm"] as string, System.Windows.Application.Current.Resources["String_ConfirmCloseQuesiton"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings) == MessageDialogResult.Affirmative)
                {
                    _closeApplication = true;
                    Close();
                }

                return;
            }

            // Save templates
            if (TemplateManager.NetworkInterfaceConfigTemplatesChanged && !ImportExportManager.ForceRestart)
                TemplateManager.SaveNetworkInterfaceConfigTemplates();

            if (TemplateManager.WakeOnLANTemplatesChanged && !ImportExportManager.ForceRestart)
                TemplateManager.SaveWakeOnLANTemplates();

            // Save settings
            if (SettingsManager.Current.SettingsChanged && !ImportExportManager.ForceRestart)
                SettingsManager.Save();

            // Unregister HotKeys
            if (RegisteredHotKeys.Count > 0)
                UnregisterHotKeys();

            // Dispose the notify icon to prevent errors
            if (notifyIcon != null)
                notifyIcon.Dispose();
        }
        #endregion

        #region Application Views
        NetworkInterfaceView networkInterfaceView;
        IPScannerView ipScannerView;
        PortScannerView portScannerView;
        SubnetCalculatorView subnetCalculatorView;
        WakeOnLANView wakeOnLANView;
        PingView pingView;
        TracerouteView tracerouteView;
        DNSLookupView dnsLookupView;
        LookupView lookupView;

        private ApplicationView.Name? currentApplicationViewName = null;

        private void ChangeApplicationView(ApplicationView.Name name)
        {
            if (currentApplicationViewName == name)
                return;

            switch (name)
            {
                case ApplicationView.Name.NetworkInterface:
                    if (networkInterfaceView == null)
                        networkInterfaceView = new NetworkInterfaceView();

                    contentControlApplication.Content = networkInterfaceView;
                    break;
                case ApplicationView.Name.IPScanner:
                    if (ipScannerView == null)
                        ipScannerView = new IPScannerView();

                    contentControlApplication.Content = ipScannerView;
                    break;
                case ApplicationView.Name.PortScanner:
                    if (portScannerView == null)
                        portScannerView = new PortScannerView();

                    contentControlApplication.Content = portScannerView;
                    break;
                case ApplicationView.Name.SubnetCalculator:
                    if (subnetCalculatorView == null)
                        subnetCalculatorView = new SubnetCalculatorView();

                    contentControlApplication.Content = subnetCalculatorView;
                    break;
                case ApplicationView.Name.WakeOnLAN:
                    if (wakeOnLANView == null)
                        wakeOnLANView = new WakeOnLANView();

                    contentControlApplication.Content = wakeOnLANView;
                    break;
                case ApplicationView.Name.Ping:
                    if (pingView == null)
                        pingView = new PingView();

                    contentControlApplication.Content = pingView;
                    break;
                case ApplicationView.Name.Traceroute:
                    if (tracerouteView == null)
                        tracerouteView = new TracerouteView();

                    contentControlApplication.Content = tracerouteView;
                    break;
                case ApplicationView.Name.DNSLookup:
                    if (dnsLookupView == null)
                        dnsLookupView = new DNSLookupView();

                    contentControlApplication.Content = dnsLookupView;
                    break;
                case ApplicationView.Name.Lookup:
                    if (lookupView == null)
                        lookupView = new LookupView();

                    contentControlApplication.Content = lookupView;
                    break;
            }

            currentApplicationViewName = name;
        }

        #endregion

        #region ListView Search
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

        protected override void OnSourceInitialized(System.EventArgs e)
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
            if (SettingsManager.Current.HotKey_ShowWindowEnabled)
            {
                RegisterHotKey(new WindowInteropHelper(this).Handle, 1, SettingsManager.Current.HotKey_ShowWindowModifier, SettingsManager.Current.HotKey_ShowWindowKey);
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
            notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
        }

        private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Windows.Controls.ContextMenu trayMenu = (System.Windows.Controls.ContextMenu)FindResource("ContextMenuNotifyIcon");
                trayMenu.IsOpen = true;
            }
        }

        private void NotifyIcon_DoubleClick(object sender, System.EventArgs e)
        {
            ShowWindowAction();
        }

        private void MetroWindowMain_StateChanged(object sender, System.EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar)
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

            notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
        }

        private void BringWindowToFront()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();
        }
        #endregion

        #region ICommands & Actions
        public ICommand OpenGithubProjectCommand
        {
            get { return new RelayCommand(p => OpenGithubProjectAction()); }
        }

        private void OpenGithubProjectAction()
        {
            Process.Start(Properties.Resources.Project_GitHub_Url);
        }

        public ICommand OpenApplicationListCommand
        {
            get { return new RelayCommand(p => OpenApplicationListAction()); }
        }

        private void OpenApplicationListAction()
        {
            OpenApplicationList = true;
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
            }
            else
            {
                settingsWindow.Owner = this;
                MetroWindowMain.ShowOverlay();
            }

            settingsWindow.ShowDialog();

            if (!_isInTray)
            {
                if (SettingsManager.Current.TrayIcon_AlwaysShowIcon && notifyIcon == null)
                    InitNotifyIcon();

                if (notifyIcon != null)
                    notifyIcon.Visible = SettingsManager.Current.TrayIcon_AlwaysShowIcon;

                MetroWindowMain.HideOverlay();
            }

            // Force restart (if user has reset the settings or import them)
            if (SettingsManager.ForceRestart || ImportExportManager.ForceRestart)
            {
                RestartApplication();
                return;
            }

            // Ask the user to restart (if he has changed the language or enables the developer mode)
            if (SettingsManager.RestartRequired)
            {
                ShowWindowAction();

                MetroDialogSettings dialogSettings = new MetroDialogSettings()
                {
                    CustomResourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
                    },

                    AffirmativeButtonText = System.Windows.Application.Current.Resources["String_Button_RestartNow"] as string,
                    NegativeButtonText = System.Windows.Application.Current.Resources["String_Button_OK"] as string,

                    DefaultButtonFocus = MessageDialogResult.Affirmative
                };

                if (await this.ShowMessageAsync(System.Windows.Application.Current.Resources["String_RestartRequired"] as string, System.Windows.Application.Current.Resources["String_RestartRequiredAfterSettingsChanged"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings) == MessageDialogResult.Affirmative)
                {
                    RestartApplication();
                    return;
                }
            }

            if (SettingsManager.HotKeysChanged)
            {
                UnregisterHotKeys();
                RegisterHotKeys();

                SettingsManager.HotKeysChanged = false;
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
            Process.Start(ConfigurationManager.Current.ApplicationFullName);

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
