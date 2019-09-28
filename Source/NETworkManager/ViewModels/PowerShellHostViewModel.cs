using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using System.Linq;
using NETworkManager.Views;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Diagnostics;
using System.IO;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Models.PowerShell;
using NETworkManager.Models.EventSystem;
using MahApps.Metro.Controls;

namespace NETworkManager.ViewModels
{
    public class PowerShellHostViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading;

        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            set
            {
                if (value == _isConfigured)
                    return;

                _isConfigured = value;
                OnPropertyChanged();
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (value == _selectedTabIndex)
                    return;

                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewPowerShellWindowAvailable => ConfigurationManager.Current.OSVersion.Major >= 10;
        #region Profiles

        public ICollectionView Profiles { get; }

        private ProfileInfo _selectedProfile = new ProfileInfo();
        public ProfileInfo SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (value == _selectedProfile)
                    return;

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                RefreshProfiles();

                OnPropertyChanged();
            }
        }

        private bool _canProfileWidthChange = true;
        private double _tempProfileWidth;

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get => _expandProfileView;
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PowerShell_ExpandProfileView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeProfile(false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get => _profileWidth;
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                    SettingsManager.Current.PowerShell_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public PowerShellHostViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            // Check if putty is available...
            CheckIfConfigured();

            InterTabClient = new DragablzInterTabClient(ApplicationViewManager.Name.PowerShell);

            TabItems = new ObservableCollection<DragablzTabItem>();

            Profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.PowerShell_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.PowerShell_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, TigerVNC_Host
                return info.PowerShell_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.PowerShell_Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.PowerShell_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.PowerShell_ApplicationFilePath))
                CheckIfConfigured();
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.PowerShell_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.PowerShell_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.PowerShell_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as PowerShellControl)?.CloseTab();
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

        public ICommand ConnectCommand => new RelayCommand(p => ConnectAction(), Connect_CanExecute);

        private bool Connect_CanExecute(object obj) => IsConfigured && !ConfigurationManager.Current.IsTransparencyEnabled;

        private void ConnectAction()
        {
            Connect();
        }

        public ICommand ConnectProfileCommand => new RelayCommand(p => ConnectProfileAction());

        private void ConnectProfileAction()
        {
            ConnectProfile();
        }

        public ICommand ConnectProfileExternalCommand => new RelayCommand(p => ConnectProfileExternalAction());

        private void ConnectProfileExternalAction()
        {
            ConnectProfileExternal();
        }

        public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

        private void AddProfileAction()
        {
            ProfileManager.ShowAddProfileDialog(this, _dialogCoordinator);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private void EditProfileAction()
        {
            ProfileManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private void CopyAsProfileAction()
        {
            ProfileManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private void DeleteProfileAction()
        {
            ProfileManager.ShowDeleteProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            ProfileManager.ShowEditGroupDialog(this, _dialogCoordinator, group.ToString());
        }

        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ICommand OpenSettingsCommand => new RelayCommand(p => OpenSettingsAction());

        private static void OpenSettingsAction()
        {
            EventSystem.RedirectToSettings();
        }
        #endregion

        #region Methods
        private void CheckIfConfigured()
        {
            IsConfigured = !string.IsNullOrEmpty(SettingsManager.Current.PowerShell_ApplicationFilePath) && File.Exists(SettingsManager.Current.PowerShell_ApplicationFilePath);
        }

        private async void Connect(string host = null)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Connect
            };

            var connectViewModel = new PowerShellConnectViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddHostToHistory(instance.Host);

                // Create Profile info
                var info = new PowerShellSessionInfo
                {
                    EnableRemoteConsole = instance.EnableRemoteConsole,
                    Host = instance.Host,
                    AdditionalCommandLine = instance.AdditionalCommandLine,
                    ExecutionPolicy = instance.ExecutionPolicy
                };

                // Connect
                Connect(info);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;
            }, host);

            customDialog.Content = new PowerShellConnectDialog
            {
                DataContext = connectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);            
        }

        private void ConnectProfile()
        {
            Connect(PowerShell.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
        }

        private void ConnectProfileExternal()
        {
            var info = new ProcessStartInfo
            {
                FileName = SettingsManager.Current.PowerShell_ApplicationFilePath,
                Arguments = PowerShell.BuildCommandLine(PowerShell.CreateSessionInfo(SelectedProfile))
            };

            Process.Start(info);
        }

        private void Connect(PowerShellSessionInfo sessionInfo, string header = null)
        {
            sessionInfo.ApplicationFilePath = SettingsManager.Current.PowerShell_ApplicationFilePath;

            TabItems.Add(new DragablzTabItem(header ?? (sessionInfo.EnableRemoteConsole ? sessionInfo.Host : Resources.Localization.Strings.PowerShell), new PowerShellControl(sessionInfo)));

            SelectedTabIndex = TabItems.Count - 1;
        }

        public void AddTab(string host)
        {
            Connect(host);
        }

        // Modify history list
        private static void AddHostToHistory(string host)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.PowerShell_HostHistory.ToList(), host, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.PowerShell_HostHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.PowerShell_HostHistory.Add(x));
        }

        private void ResizeProfile(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix;
            }
            else
            {
                if (ExpandProfileView)
                {
                    ProfileWidth = Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) < GlobalStaticConfiguration.FloatPointFix ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded) : new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
                }
            }

            _canProfileWidthChange = true;
        }

        public void OnViewVisible()
        {
            RefreshProfiles();
        }

        public void OnViewHide()
        {

        }

        public void RefreshProfiles()
        {
            Profiles.Refresh();
        }

        public void OnProfileDialogOpen()
        {
            ConfigurationManager.Current.FixAirspace = true;
        }

        public void OnProfileDialogClose()
        {
            ConfigurationManager.Current.FixAirspace = false;
        }
        #endregion
    }
}