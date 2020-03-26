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
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Models.EventSystem;
using NETworkManager.Models.Profile;
using NETworkManager.Models.WebConsole;
using System.Windows.Threading;

namespace NETworkManager.ViewModels
{
    public class WebConsoleHostViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();

        public IInterTabClient InterTabClient { get; }
        public ObservableCollection<DragablzTabItem> TabItems { get; }

        private readonly bool _isLoading;

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

                StartDelayedSearch();

                OnPropertyChanged();
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (value == _isSearching)
                    return;

                _isSearching = value;
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
                    SettingsManager.Current.WebConsole_ExpandProfileView = value;

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
                    SettingsManager.Current.WebConsole_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(true);

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public WebConsoleHostViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            InterTabClient = new DragablzInterTabClient(Models.Application.ApplicationName.WebConsole);

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
                    return info.WebConsole_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.WebConsole_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, WebConsole_Url
                return info.WebConsole_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.WebConsole_Url.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.WebConsole_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

            LoadSettings();

            SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

            _isLoading = false;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.WebConsole_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.WebConsole_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.WebConsole_ProfileWidth;
        }
        #endregion

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand => CloseItemAction;

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzTabItem)?.View as WebConsoleControl)?.CloseTab();
        }
        
        public ICommand WebConsole_RefreshCommand => new RelayCommand(WebConsole_RefreshAction);

        private void WebConsole_RefreshAction(object view)
        {
            if (view is WebConsoleControl control)
            {
                if (control.RefreshCommand.CanExecute(null))
                    control.RefreshCommand.Execute(null);
            }
        }

        public ICommand ConnectCommand => new RelayCommand(p => ConnectAction());

        private void ConnectAction()
        {
            Connect();
        }

        public ICommand ConnectProfileCommand => new RelayCommand(p => ConnectProfileAction(), ConnectProfile_CanExecute);

        private bool ConnectProfile_CanExecute(object obj)
        {
            return SelectedProfile != null;
        }

        private void ConnectProfileAction()
        {
            ConnectProfile();
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
        private async void Connect()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Connect
            };

            var connectViewModel = new WebConsoleConnectViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                ConfigurationManager.Current.FixAirspace = false;

                // Add host to history
                AddUrlToHistory(instance.Url);

                // Create Profile info
                var info = new WebConsoleSessionInfo
                {
                    Url = instance.Url
                };

                // Connect
                Connect(info);
            }, async instance =>
             {
                 await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                 ConfigurationManager.Current.FixAirspace = false;
             });

            customDialog.Content = new WebConsoleConnectDialog
            {
                DataContext = connectViewModel
            };

            ConfigurationManager.Current.FixAirspace = true;
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        private void ConnectProfile()
        {
            Connect(Models.WebConsoleTMP.WebConsole.CreateSessionInfo(SelectedProfile), SelectedProfile.Name);
        }

        private void Connect(WebConsoleSessionInfo sessionInfo, string header = null)
        {
            TabItems.Add(new DragablzTabItem(header ?? sessionInfo.Url, new WebConsoleControl(sessionInfo)));

            SelectedTabIndex = TabItems.Count - 1;
        }

        // Modify history list
        private static void AddUrlToHistory(string url)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.WebConsole_UrlHistory.ToList(), url, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.WebConsole_UrlHistory.Clear();

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.WebConsole_UrlHistory.Add(x));
        }

        private void StartDelayedSearch()
        {
            if (!IsSearching)
            {
                IsSearching = true;

                _searchDispatcherTimer.Start();
            }
            else
            {
                _searchDispatcherTimer.Stop();
                _searchDispatcherTimer.Start();
            }
        }

        private void StopDelayedSearch()
        {
            _searchDispatcherTimer.Stop();

            RefreshProfiles();

            IsSearching = false;
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

        #region Event
        private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            StopDelayedSearch();
        }
        #endregion
    }
}