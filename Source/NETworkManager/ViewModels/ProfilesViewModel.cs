using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;
using System;
using NETworkManager.Utilities;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Threading;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using System.Diagnostics;
using System.Windows;

namespace NETworkManager.ViewModels
{
    public class ProfilesViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();

        public ICollectionView _groups;
        public ICollectionView Groups
        {
            get => _groups;
            set
            {
                if (value == _groups)
                    return;

                _groups = value;
                OnPropertyChanged();
            }
        }

        private GroupInfo _selectedGroup = new GroupInfo();
        public GroupInfo SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (value == _selectedGroup)
                    return;

                // NullReferenceException occurs if profile file is changed
                if (value == null)
                    Profiles = null;
                else
                    SetProfilesView(value.Name);

                _selectedGroup = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView _profiles;
        public ICollectionView Profiles
        {
            get => _profiles;
            set
            {
                if (value == _profiles)
                    return;

                _profiles = value;
                OnPropertyChanged();
            }
        }

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

        private IList _selectedProfiles = new ArrayList();
        public IList SelectedProfiles
        {
            get => _selectedProfiles;
            set
            {
                if (Equals(value, _selectedProfiles))
                    return;

                _selectedProfiles = value;
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
        #endregion

        #region Constructor
        public ProfilesViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            SetGroupView();

            ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;
        }

        public void SetGroupView()
        {
            Groups = new CollectionViewSource { Source = ProfileManager.Groups.Where(x => !x.IsDynamic) }.View;

            Groups.SortDescriptions.Add(new SortDescription(nameof(GroupInfo.Name), ListSortDirection.Ascending));

            SelectedGroup = Groups.SourceCollection.Cast<GroupInfo>().OrderBy(x => x.Name).FirstOrDefault();
        }

        public void SetProfilesView(string groupName)
        {
            Profiles = new CollectionViewSource { Source = ProfileManager.Groups.FirstOrDefault(x => x.Name.Equals(groupName)).Profiles.Where(x => !x.IsDynamic) }.View;

            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                if (o is not ProfileInfo info)
                    return false;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                /*
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
                */

                // Search by: Name
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            // Select first profile, or the last selected profile
                SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().OrderBy(x => x.Name).FirstOrDefault();

            SelectedProfiles = new List<ProfileInfo> { SelectedProfile }; // Fix --> Count need to be 1 for EditProfile_CanExecute
        }
        #endregion

        #region Commands & Actions
        public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

        private void AddProfileAction()
        {
            ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator, SelectedGroup?.Name);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction(), EditProfile_CanExecute);

        private bool EditProfile_CanExecute(object paramter) => SelectedProfiles.Count == 1;

        private void EditProfileAction()
        {
            ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        private bool ModifyProfile_CanExecute(object obj) => SelectedProfile != null && !SelectedProfile.IsDynamic;

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction(), ModifyProfile_CanExecute);

        private void CopyAsProfileAction()
        {
            ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction(), ModifyProfile_CanExecute);

        private void DeleteProfileAction()
        {
            ProfileDialogManager.ShowDeleteProfileDialog(this, _dialogCoordinator, new List<ProfileInfo>(SelectedProfiles.Cast<ProfileInfo>()));
        }

        public ICommand AddGroupCommand => new RelayCommand(p => AddGroupAction());

        private void AddGroupAction()
        {
            ProfileDialogManager.ShowAddGroupDialog(this, _dialogCoordinator);
        }

        public ICommand EditGroupCommand => new RelayCommand(p => EditGroupAction());

        private void EditGroupAction()
        {
            ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, SelectedGroup);
        }

        public ICommand DeleteGroupCommand => new RelayCommand(p => DeleteGroupAction());

        private void DeleteGroupAction()
        {
            ProfileDialogManager.ShowDeleteGroupDialog(this, _dialogCoordinator, SelectedGroup);
        }
        #endregion

        #region Methods
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

        public void RefreshProfiles()
        {
            Debug.WriteLine("ProfilesViewModel: Refresh profiles...");

            if (SelectedGroup == null)
            {
                Debug.WriteLine("ProfilesViewModel: SelectedGroup is null, try to set");
                SetGroupView();
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                Groups?.Refresh();
                Profiles?.Refresh();
            }));
        }

        public void OnProfileDialogOpen()
        {

        }

        public void OnProfileDialogClose()
        {

        }
        #endregion

        #region Event
        private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
        {
            // Update group view (and profile view) when the profile file has changed            
            RefreshProfiles();
        }

        private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            StopDelayedSearch();
        }
        #endregion
    }
}