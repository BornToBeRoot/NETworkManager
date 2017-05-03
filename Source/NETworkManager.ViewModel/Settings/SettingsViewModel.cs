using NETworkManager.Settings;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using NETworkManager.ViewModel.Applications;
using MahApps.Metro.Controls;
using NETworkManager.Utilities.Common;
using System;

namespace NETworkManager.ViewModel.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;

        private bool _isLoading = true;

        public bool HotKeysChanged { get; set; }

        public SettingsViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // General
            if (NETworkManager.Settings.Properties.Settings.Default.DeveloperMode)
                ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(ApplicationView.List.OrderBy(a => a.Name));
            else
                ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(ApplicationView.List.Where(a => a.IsDev == false).OrderBy(a => a.Name));

            DefaultApplicationViewSelectedItem = ApplicationViewCollection.FirstOrDefault(x => x.Name == (ApplicationView.Name)Enum.Parse(typeof(ApplicationView.Name), NETworkManager.Settings.Properties.Settings.Default.Application_DefaultApplicationViewName));

            StartMaximized = NETworkManager.Settings.Properties.Settings.Default.Window_StartMaximized;
            AlwaysShowIconInTray = NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon;
            MinimizeInsteadOfTerminating = NETworkManager.Settings.Properties.Settings.Default.Window_MinimizeInsteadOfTerminating;
            ConfirmClose = NETworkManager.Settings.Properties.Settings.Default.Window_ConfirmClose;
            MinimizeToTrayInsteadOfTaskbar = NETworkManager.Settings.Properties.Settings.Default.Window_MinimizeToTrayInsteadOfTaskbar;

            // Appearance
            AppThemeSelectedItem = ThemeManager.DetectAppStyle().Item1;
            AccentSelectedItem = ThemeManager.DetectAppStyle().Item2;

            // Language
            LanguageCollection = new ObservableCollection<LocalizationInfo>(Localization.List);
            LocalizationSelectedItem = LanguageCollection.FirstOrDefault(x => x.Code == Localization.Current.Code);

            // HotKeys
            HotKeyShowWindowEnabled = NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowEnabled;
            HotKeyShowWindow = new HotKey(HotKeysHelper.FormsKeysToWpfKey(NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowKey), HotKeysHelper.GetModifierKeysFromInt(NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowModifier));

            // Autostart
            StartWithWindows = Autostart.IsEnabled;
            StartMinimizedInTray = NETworkManager.Settings.Properties.Settings.Default.Autostart_StartMinimizedInTray;

            // Settings
            SettingsLocationSelectedPath = SettingsManager.SettingsLocationNotPortable;
            SettingsPortable = SettingsManager.IsPortable;

            // Developer
            DeveloperMode = NETworkManager.Settings.Properties.Settings.Default.DeveloperMode;

            // About
            if (AssemblyManager.Current == null)
                AssemblyManager.Load();

            CopyrightAndAuthor = string.Format("{0} {1}.", AssemblyManager.Current.Copyright, AssemblyManager.Current.Company);

            Version = AssemblyManager.Current.AssemblyVersion.ToString();

            _isLoading = false;
        }

        public void OnClosing()
        {
            if (SettingsManager.SettingsChanged)
                SettingsManager.SaveSettings();
        }

        #region General
        public ObservableCollection<ApplicationViewInfo> ApplicationViewCollection { get; set; }

        private ApplicationViewInfo _defaultApplicationViewSelectedItem;
        public ApplicationViewInfo DefaultApplicationViewSelectedItem
        {
            get { return _defaultApplicationViewSelectedItem; }
            set
            {
                if (value == _defaultApplicationViewSelectedItem)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Application_DefaultApplicationViewName = value.Name.ToString();

                    SettingsManager.SettingsChanged = true;
                }

                _defaultApplicationViewSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private bool _startMaximized;
        public bool StartMaximized
        {
            get { return _startMaximized; }
            set
            {
                if (value == _startMaximized)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Window_StartMaximized = value;

                    SettingsManager.SettingsChanged = true;
                }

                _startMaximized = value;
                OnPropertyChanged();
            }
        }

        private bool _minimizeInsteadOfTerminating;
        public bool MinimizeInsteadOfTerminating
        {
            get { return _minimizeInsteadOfTerminating; }
            set
            {
                if (value == _minimizeInsteadOfTerminating)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Window_MinimizeInsteadOfTerminating = value;

                    SettingsManager.SettingsChanged = true;
                }

                _minimizeInsteadOfTerminating = value;
                OnPropertyChanged();
            }
        }

        private bool _minimizeToTrayInsteadOfTaskbar;
        public bool MinimizeToTrayInsteadOfTaskbar
        {
            get { return _minimizeToTrayInsteadOfTaskbar; }
            set
            {
                if (value == _minimizeToTrayInsteadOfTaskbar)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Window_MinimizeToTrayInsteadOfTaskbar = value;

                    SettingsManager.SettingsChanged = true;
                }

                _minimizeToTrayInsteadOfTaskbar = value;
                OnPropertyChanged();
            }
        }

        private bool _confirmClose;
        public bool ConfirmClose
        {
            get { return _confirmClose; }
            set
            {
                if (value == _confirmClose)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Window_ConfirmClose = value;

                    SettingsManager.SettingsChanged = true;
                }

                OnPropertyChanged();
                _confirmClose = value;
            }
        }

        private bool _alwaysShowIconInTray;
        public bool AlwaysShowIconInTray
        {
            get { return _alwaysShowIconInTray; }
            set
            {
                if (value == _alwaysShowIconInTray)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.TrayIcon_AlwaysShowIcon = value;

                    SettingsManager.SettingsChanged = true;
                }

                _alwaysShowIconInTray = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Appearance
        private AppTheme _appThemeSelectedItem;
        public AppTheme AppThemeSelectedItem
        {
            get { return _appThemeSelectedItem; }
            set
            {
                if (value == _appThemeSelectedItem)
                    return;

                if (!_isLoading)
                {
                    Appearance.ChangeAppTheme(value.Name);

                    NETworkManager.Settings.Properties.Settings.Default.Appearance_AppTheme = value.Name;

                    SettingsManager.SettingsChanged = true;
                }

                _appThemeSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private Accent _accentSelectedItem;
        public Accent AccentSelectedItem
        {
            get { return _accentSelectedItem; }
            set
            {
                if (value == _accentSelectedItem)
                    return;

                if (!_isLoading)
                {
                    Appearance.ChangeAccent(value.Name);

                    NETworkManager.Settings.Properties.Settings.Default.Appearance_Accent = value.Name;

                    SettingsManager.SettingsChanged = true;
                }

                _accentSelectedItem = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Language
        public ObservableCollection<LocalizationInfo> LanguageCollection { get; set; }

        private LocalizationInfo _localizationSelectedItem;
        public LocalizationInfo LocalizationSelectedItem
        {
            get { return _localizationSelectedItem; }
            set
            {
                if (value == _localizationSelectedItem)
                    return;

                if (!_isLoading)
                {
                    Localization.Change(value);

                    NETworkManager.Settings.Properties.Settings.Default.Localization_CultureCode = value.Code;

                    SettingsManager.RestartRequired = true;
                    SettingsManager.SettingsChanged = true;
                }

                _localizationSelectedItem = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Hotkeys
        private bool _hotKeyShowWindowEnabled;
        public bool HotKeyShowWindowEnabled
        {
            get { return _hotKeyShowWindowEnabled; }
            set
            {
                if (value == _hotKeyShowWindowEnabled)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowEnabled = value;

                    HotKeysChanged = true;
                    SettingsManager.SettingsChanged = true;
                }

                _hotKeyShowWindowEnabled = value;
                OnPropertyChanged();
            }
        }

        private HotKey _hotKeyShowWindow;
        public HotKey HotKeyShowWindow
        {
            get { return _hotKeyShowWindow; }
            set
            {
                if (value == _hotKeyShowWindow)
                    return;

                if (!_isLoading && value != null)
                {
                    NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowKey = (int)HotKeysHelper.WpfKeyToFormsKeys(value.Key);
                    NETworkManager.Settings.Properties.Settings.Default.HotKey_ShowWindowModifier = HotKeysHelper.GetModifierKeysSum(value.ModifierKeys);

                    HotKeysChanged = true;
                    SettingsManager.SettingsChanged = true;
                }

                _hotKeyShowWindow = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Autostart
        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get { return _startWithWindows; }
            set
            {
                if (value == _startWithWindows)
                    return;

                if (!_isLoading)
                {
                    if (value)
                        Autostart.Enable();
                    else
                        Autostart.Disable();
                }

                _startWithWindows = value;
                OnPropertyChanged();
            }
        }

        private bool _startMinimizedInTray;
        public bool StartMinimizedInTray
        {
            get { return _startMinimizedInTray; }
            set
            {
                if (value == _startMinimizedInTray)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.Autostart_StartMinimizedInTray = value;

                    SettingsManager.SettingsChanged = true;
                }

                _startMinimizedInTray = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Settings
        private string _settingsLocationSelectedPath;
        public string SettingsLocationSelectedPath
        {
            get { return _settingsLocationSelectedPath; }
            set
            {
                if (value == _settingsLocationSelectedPath)
                    return;

                _settingsLocationSelectedPath = value;
                OnPropertyChanged();
            }
        }

        private bool _settingsPortable;
        public bool SettingsPortable
        {
            get { return _settingsPortable; }
            set
            {
                if (value == _settingsPortable)
                    return;

                if (!_isLoading)
                {
                    // Save settings before moving them
                    if (SettingsManager.SettingsChanged)
                    {
                        SettingsManager.SaveSettings();
                        SettingsManager.SettingsChanged = false;
                    }

                    SettingsManager.MakeSettingsPortable(value, true);

                    if (!SettingsManager.IsPortable)
                    {
                        SettingsLocationSelectedPath = SettingsManager.SettingsLocationNotPortable;

                        NETworkManager.Settings.Properties.Settings.Default.Settings_Location = SettingsLocationSelectedPath;

                        SettingsManager.SettingsChanged = true;
                    }
                }

                _settingsPortable = value;
                OnPropertyChanged();
            }
        }

        public ICommand BrowseFolderCommand
        {
            get { return new RelayCommand(p => BrowseFolderAction()); }
        }

        private void BrowseFolderAction()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(SettingsLocationSelectedPath))
                dialog.SelectedPath = SettingsLocationSelectedPath;

            System.Windows.Forms.DialogResult dialogResult = dialog.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
                SettingsLocationSelectedPath = dialog.SelectedPath;
        }

        public ICommand ChangeSettingsCommand
        {
            get { return new RelayCommand(p => ChangeSettingsAction()); }
        }

        private void ChangeSettingsAction()
        {
            SettingsManager.ChangeSettingsLocation(SettingsLocationSelectedPath, true);

            NETworkManager.Settings.Properties.Settings.Default.Settings_Location = SettingsLocationSelectedPath;
            SettingsManager.SettingsChanged = true;
        }

        public ICommand RestoreDefaultSettingsLocationCommand
        {
            get { return new RelayCommand(p => RestoreDefaultSettingsLocationAction()); }
        }

        private void RestoreDefaultSettingsLocationAction()
        {
            SettingsLocationSelectedPath = SettingsManager.DefaultSettingsLocation;
        }
        #endregion

        #region Developer
        private bool _developerMode;
        public bool DeveloperMode
        {
            get { return _developerMode; }
            set
            {
                if (value == _developerMode)
                    return;

                if (!_isLoading)
                {
                    NETworkManager.Settings.Properties.Settings.Default.DeveloperMode = value;

                    SettingsManager.RestartRequired = true;
                    SettingsManager.SettingsChanged = true;
                }

                _developerMode = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region About
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
                OnPropertyChanged();
            }
        }

        private string _copyrightAndAuthor;
        public string CopyrightAndAuthor
        {
            get { return _copyrightAndAuthor; }
            set
            {
                if (value == _copyrightAndAuthor)
                    return;

                _copyrightAndAuthor = value;
                OnPropertyChanged();
            }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                if (value == _version)
                    return;

                _version = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenWebsiteLibaryMahAppsMetroCommand
        {
            get { return new RelayCommand(p => OpenWebsiteLibaryMahAppsMetroAction()); }
        }

        private void OpenWebsiteLibaryMahAppsMetroAction()
        {
            Process.Start(NETworkManager.Settings.Properties.Resources.Libary_MahAppsMetro_Url);
        }

        public ICommand OpenWebsiteLibaryMahAppsMetroIconPacksCommand
        {
            get { return new RelayCommand(p => OpenWebsiteLibaryMahAppsMetroIconPacksAction()); }
        }

        private void OpenWebsiteLibaryMahAppsMetroIconPacksAction()
        {
            Process.Start(NETworkManager.Settings.Properties.Resources.Libary_MahAppsMetroIconPacks_Url);
        }
        #endregion
    }
}