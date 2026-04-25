using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

public class ProfilesViewModel : ViewModelBase, IProfileManager
{
    #region Constructor

    public ProfilesViewModel()
    {
        ProfileFilterTagsView = CollectionViewSource.GetDefaultView(ProfileFilterTags);
        ProfileFilterTagsView.SortDescriptions.Add(new SortDescription(nameof(ProfileFilterTagsInfo.Name),
            ListSortDirection.Ascending));

        SetGroupsView();

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;
    }

    #endregion

    #region Variables

    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private bool _searchDisabled;

    private bool _isViewActive = true;

    public ICollectionView Groups
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    private bool _disableProfileRefresh;

    private ProfileInfo _lastSelectedProfileOnRefresh;

    public GroupInfo SelectedGroup
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            // Check for null, because a NullReferenceException can occur when a profile file is changed
            // Temporarily disable profile refresh to avoid multiple refreshes and prevent the filter from being reset.
            if (value != null && !_disableProfileRefresh)
            {
                // Set/update tags based on current group
                CreateTags();

                var filter = new ProfileFilterInfo
                {
                    Search = Search,
                    Tags = [.. ProfileFilterTags.Where(x => x.IsSelected).Select(x => x.Name)],
                    TagsFilterMatch = ProfileFilterTagsMatchAny
                        ? ProfileFilterTagsMatch.Any
                        : ProfileFilterTagsMatch.All
                };

                SetProfilesView(filter, value, _lastSelectedProfileOnRefresh);

                IsProfileFilterSet = !string.IsNullOrEmpty(filter.Search) || filter.Tags.Any();
            }
            else
            {
                Profiles = null;
            }

            OnPropertyChanged();
        }
    } = new();

    public ICollectionView Profiles
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ProfileInfo SelectedProfile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public IList SelectedProfiles
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    public string Search
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            // Start searching...
            if (!_searchDisabled)
            {
                IsSearching = true;
                _searchDispatcherTimer.Start();
            }

            OnPropertyChanged();
        }
    }

    public bool IsSearching
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ProfileFilterIsOpen
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ProfileFilterTagsView { get; }

    private ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; } = [];

    public bool ProfileFilterTagsMatchAny
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = GlobalStaticConfiguration.Profile_TagsMatchAny;

    public bool ProfileFilterTagsMatchAll
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsProfileFilterSet
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Commands & Actions

    public ICommand AddGroupCommand => new RelayCommand(_ => AddGroupAction());

    private void AddGroupAction()
    {
        ProfileDialogManager.ShowAddGroupDialog(Application.Current.MainWindow, this).ConfigureAwait(false);
    }

    public ICommand EditGroupCommand => new RelayCommand(_ => EditGroupAction());

    private void EditGroupAction()
    {
        ProfileDialogManager.ShowEditGroupDialog(Application.Current.MainWindow, this, SelectedGroup).ConfigureAwait(false);
    }

    public ICommand DeleteGroupCommand => new RelayCommand(_ => DeleteGroupAction());

    private void DeleteGroupAction()
    {
        ProfileDialogManager.ShowDeleteGroupDialog(Application.Current.MainWindow, this, SelectedGroup).ConfigureAwait(false);
    }

    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        ProfileDialogManager.ShowAddProfileDialog(Application.Current.MainWindow, this, null, SelectedGroup?.Name)
            .ConfigureAwait(false);
    }

    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), EditProfile_CanExecute);

    private bool EditProfile_CanExecute(object parameter)
    {
        return SelectedProfiles.Count == 1;
    }

    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(Application.Current.MainWindow, this, SelectedProfile).ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(Application.Current.MainWindow, this, SelectedProfile).ConfigureAwait(false);
    }

    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(Application.Current.MainWindow, this, [.. SelectedProfiles.Cast<ProfileInfo>()])
            .ConfigureAwait(false);
    }

    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    private void OpenProfileFilterAction()
    {
        ProfileFilterIsOpen = true;
    }

    public ICommand ApplyProfileFilterCommand => new RelayCommand(_ => ApplyProfileFilterAction());

    private void ApplyProfileFilterAction()
    {
        RefreshProfiles();

        ProfileFilterIsOpen = false;
    }

    public ICommand ClearProfileFilterCommand => new RelayCommand(_ => ClearProfileFilterAction());

    private void ClearProfileFilterAction()
    {
        _searchDisabled = true;
        Search = string.Empty;
        _searchDisabled = false;

        foreach (var tag in ProfileFilterTags)
            tag.IsSelected = false;

        RefreshProfiles();

        IsProfileFilterSet = false;
        ProfileFilterIsOpen = false;
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
        _disableProfileRefresh = true;

        Groups = new CollectionViewSource
        {
            Source = ProfileManager.LoadedProfileFileData.Groups.Where(x => !x.IsDynamic).OrderBy(x => x.Name)
        }.View;

        // Set to null, so even when the same group is selected, the profiles get refreshed
        SelectedGroup = null;

        _disableProfileRefresh = false;

        // Set specific group or first if null
        if (group != null)
            SelectedGroup = Groups.SourceCollection.Cast<GroupInfo>().FirstOrDefault(x => x.Equals(group)) ??
                            Groups.SourceCollection.Cast<GroupInfo>().MinBy(x => x.Name);
        else
            SelectedGroup = Groups.SourceCollection.Cast<GroupInfo>().MinBy(x => x.Name);
    }

    private void CreateTags()
    {
        // Get all tags from profiles in the selected group
        var tags = ProfileManager.LoadedProfileFileData.Groups.First(x => x.Name == SelectedGroup.Name).Profiles
            .SelectMany(x => x.TagsCollection).Distinct().ToList();

        var tagSet = new HashSet<string>(tags);

        for (var i = ProfileFilterTags.Count - 1; i >= 0; i--)
        {
            if (!tagSet.Contains(ProfileFilterTags[i].Name))
                ProfileFilterTags.RemoveAt(i);
        }

        var existingTagNames = new HashSet<string>(ProfileFilterTags.Select(ft => ft.Name));

        foreach (var tag in tags.Where(tag => !existingTagNames.Contains(tag)))
        {
            ProfileFilterTags.Add(new ProfileFilterTagsInfo(false, tag));
        }
    }

    private void SetProfilesView(ProfileFilterInfo filter, GroupInfo group, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.LoadedProfileFileData.Groups.FirstOrDefault(x => x.Equals(group))?.Profiles.Where(x => !x.IsDynamic && (
                string.IsNullOrEmpty(Search) || x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
                    // If no tags are selected, show all profiles
                    (!filter.Tags.Any()) ||
                    // Any tag can match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.Any &&
                     filter.Tags.Any(tag => x.TagsCollection.Contains(tag))) ||
                    // All tags must match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.All &&
                     filter.Tags.All(tag => x.TagsCollection.Contains(tag))))
                ).OrderBy(x => x.Name)
        }.View;

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