using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class ProfilesViewModel : ViewModelBase, IProfileManager
{
    #region Constructor

    public ProfilesViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

        SetGroupsView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;
    }

    #endregion

    #region Variables

    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly DispatcherTimer _searchDispatcherTimer = new();

    private bool _isViewActive = true;

    private ICollectionView _groups;

    public ICollectionView Groups
    {
        get => _groups;
        private set
        {
            if (value == _groups)
                return;

            _groups = value;
            OnPropertyChanged();
        }
    }

    private ProfileInfo _lastSelectedProfileOnRefresh;

    private GroupInfo _selectedGroup = new();

    public GroupInfo SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (value == _selectedGroup)
                return;

            _selectedGroup = value;

            // NullReferenceException occurs if profile file is changed            
            if (value != null)
                SetProfilesView(value, _lastSelectedProfileOnRefresh);
            else
                Profiles = null;

            OnPropertyChanged();
        }
    }

    private ICollectionView _profiles;

    public ICollectionView Profiles
    {
        get => _profiles;
        private set
        {
            if (value == _profiles)
                return;

            _profiles = value;
            OnPropertyChanged();
        }
    }

    private ProfileInfo _selectedProfile = new();

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

            // Start searching...
            IsSearching = true;
            _searchDispatcherTimer.Start();

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

    #region Commands & Actions

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager.ShowAddProfileDialog(this, this, _dialogCoordinator, null, SelectedGroup?.Name)
            .ConfigureAwait(false);
    }

    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), EditProfile_CanExecute);

    private bool EditProfile_CanExecute(object parameter)
    {
        return SelectedProfiles.Count == 1;
    }

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(this, _dialogCoordinator,
                new List<ProfileInfo>(SelectedProfiles.Cast<ProfileInfo>())).ConfigureAwait(false);
    }

    public ICommand AddGroupCommand => new RelayCommand(_ => AddGroupAction());

    private void AddGroupAction()
    {
        ProfileDialogManager.ShowAddGroupDialog(this, _dialogCoordinator).ConfigureAwait(false);
    }

    public ICommand EditGroupCommand => new RelayCommand(_ => EditGroupAction());

    private void EditGroupAction()
    {
        ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, SelectedGroup).ConfigureAwait(false);
    }

    public ICommand DeleteGroupCommand => new RelayCommand(_ => DeleteGroupAction());

    private void DeleteGroupAction()
    {
        ProfileDialogManager.ShowDeleteGroupDialog(this, _dialogCoordinator, SelectedGroup).ConfigureAwait(false);
    }

    #endregion

    #region Methods

    public void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }

    private void SetGroupsView(GroupInfo group = null)
    {
        Groups = new CollectionViewSource
            { Source = ProfileManager.Groups.Where(x => !x.IsDynamic).OrderBy(x => x.Name) }.View;

        // Set specific group or first if null
        SelectedGroup = null;

        if (group != null)
            SelectedGroup = Groups.SourceCollection.Cast<GroupInfo>().FirstOrDefault(x => x.Equals(group)) ??
                            Groups.SourceCollection.Cast<GroupInfo>().MinBy(x => x.Name);
        else
            SelectedGroup = Groups.SourceCollection.Cast<GroupInfo>().MinBy(x => x.Name);
    }

    private void SetProfilesView(GroupInfo group, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.FirstOrDefault(x => x.Equals(group))?.Profiles.Where(x => !x.IsDynamic)
                .OrderBy(x => x.Name)
        }.View;

        Profiles.Filter = o =>
        {
            if (o is not ProfileInfo info)
                return false;

            if (string.IsNullOrEmpty(Search))
                return true;

            var search = Search.Trim();

            // Search by: Tag=xxx (exact match, ignore case)
            /*
            if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                return !string.IsNullOrEmpty(info.Tags) && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));
            */

            // Search by: Name, Host
            return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 ||
                   info.Host.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Set specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                              Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();

        SelectedProfiles = new List<ProfileInfo>
            { SelectedProfile }; // Fix --> Count need to be 1 for EditProfile_CanExecute
    }

    private void RefreshProfiles()
    {
        if (!_isViewActive)
            return;

        _lastSelectedProfileOnRefresh = SelectedProfile;

        SetGroupsView(SelectedGroup);

        _lastSelectedProfileOnRefresh = null;
    }

    #endregion

    #region Event

    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        RefreshProfiles();
    }

    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    #endregion
}