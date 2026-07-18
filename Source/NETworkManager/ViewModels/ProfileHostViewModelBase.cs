using NETworkManager.Controls;
using NETworkManager.Models;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
///     Base class for view models that host the shared profile panel (search, tag filter, group list) next to a
///     tool-specific view. Holds all profile-panel state/commands that are identical across tools; derived
///     classes only need to describe how their tool's profiles are identified and searched.
/// </summary>
public abstract class ProfileHostViewModelBase : ViewModelBase, IProfileHostViewModel
{
    #region Variables

    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private bool _searchDisabled;
    private bool _isViewActive = true;

    /// <summary>
    ///     Gets the application name used for profile dialogs (add/edit/copy-as/delete) of this tool.
    /// </summary>
    protected abstract ApplicationName ApplicationName { get; }

    /// <summary>
    ///     Determines whether the given profile is enabled for this tool.
    /// </summary>
    protected abstract bool IsProfileEnabled(ProfileInfo profile);

    /// <summary>
    ///     Gets the tool-specific field of the given profile that should also be matched against the search text
    ///     (in addition to the profile name).
    /// </summary>
    protected abstract string GetSearchableField(ProfileInfo profile);

    /// <summary>
    ///     Gets the view for the profiles.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets the selected profile. Virtual so tools that need to react to a profile selection (e.g.
    ///     pre-filling other fields from the profile) can override it and still participate in the base class's
    ///     own internal re-selection logic (e.g. in <see cref="SetProfilesView" />).
    /// </summary>
    public virtual ProfileInfo SelectedProfile
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

    /// <summary>
    ///     Gets or sets the search text.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets a value indicating whether a search is in progress.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets a value indicating whether the profile filter popup is open.
    /// </summary>
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

    /// <summary>
    ///     Gets the view for the profile filter tags.
    /// </summary>
    public ICollectionView ProfileFilterTagsView { get; private set; }

    private ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; } = [];

    /// <summary>
    ///     Gets or sets a value indicating whether to match any profile filter tag.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets a value indicating whether to match all profile filter tags.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets a value indicating whether a profile filter is currently applied.
    /// </summary>
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

    /// <summary>
    ///     Gets the group expander state store for the profile list.
    /// </summary>
    public GroupExpanderStateStore GroupExpanderStateStore { get; } = new();

    #endregion

    #region Constructor

    /// <summary>
    ///     Initializes the shared profile-panel state. Must be called by derived constructors after their own
    ///     tool-specific initialization (and while <c>_isLoading</c>-equivalent guards, if any, are still active).
    /// </summary>
    protected void InitializeProfileHost()
    {
        CreateTags();

        ProfileFilterTagsView = CollectionViewSource.GetDefaultView(ProfileFilterTags);
        ProfileFilterTagsView.SortDescriptions.Add(new SortDescription(nameof(ProfileFilterTagsInfo.Name),
            ListSortDirection.Ascending));

        SetProfilesView(new ProfileFilterInfo());

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    ///     Gets the command to add a new profile.
    /// </summary>
    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    private void AddProfileAction()
    {
        _ = ProfileDialogManager
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    /// <summary>
    ///     Gets the command to edit the selected profile.
    /// </summary>
    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    private void EditProfileAction()
    {
        _ = ProfileDialogManager.ShowEditProfileDialog(Application.Current.MainWindow, this, SelectedProfile);
    }

    /// <summary>
    ///     Gets the command to copy the selected profile as a new profile.
    /// </summary>
    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    private void CopyAsProfileAction()
    {
        _ = ProfileDialogManager.ShowCopyAsProfileDialog(Application.Current.MainWindow, this, SelectedProfile);
    }

    /// <summary>
    ///     Gets the command to delete the selected profile.
    /// </summary>
    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    private void DeleteProfileAction()
    {
        _ = ProfileDialogManager
            .ShowDeleteProfileDialog(Application.Current.MainWindow, this, new List<ProfileInfo> { SelectedProfile });
    }

    /// <summary>
    ///     Gets the command to edit a group.
    /// </summary>
    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        _ = ProfileDialogManager
            .ShowEditGroupDialog(Application.Current.MainWindow, this, ProfileManager.GetGroupByName($"{group}"));
    }

    /// <summary>
    ///     Gets the command to open the profile filter.
    /// </summary>
    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    private void OpenProfileFilterAction()
    {
        ProfileFilterIsOpen = true;
    }

    /// <summary>
    ///     Gets the command to apply the profile filter.
    /// </summary>
    public ICommand ApplyProfileFilterCommand => new RelayCommand(_ => ApplyProfileFilterAction());

    private void ApplyProfileFilterAction()
    {
        RefreshProfiles();

        ProfileFilterIsOpen = false;
    }

    /// <summary>
    ///     Gets the command to clear the profile filter.
    /// </summary>
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

    /// <summary>
    ///     Gets the command to expand all profile groups.
    /// </summary>
    public ICommand ExpandAllProfileGroupsCommand => new RelayCommand(_ => ExpandAllProfileGroupsAction());

    private void ExpandAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(true);
    }

    /// <summary>
    ///     Gets the command to collapse all profile groups.
    /// </summary>
    public ICommand CollapseAllProfileGroupsCommand => new RelayCommand(_ => CollapseAllProfileGroupsAction());

    private void CollapseAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(false);
    }

    #endregion

    #region Methods

    private void SetIsExpandedForAllProfileGroups(bool isExpanded)
    {
        foreach (var group in Profiles.Groups.Cast<CollectionViewGroup>())
            GroupExpanderStateStore[group.Name.ToString()] = isExpanded;
    }

    /// <summary>
    ///     Called when the view becomes visible.
    /// </summary>
    public virtual void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();
    }

    /// <summary>
    ///     Called when the view is hidden.
    /// </summary>
    public virtual void OnViewHide()
    {
        _isViewActive = false;
    }

    /// <summary>
    ///     Called when a dialog in the <see cref="Profiles.ProfileManager" /> is opened. Virtual so tools with an
    ///     embedded native window (e.g. PowerShell, PuTTY, RemoteDesktop, TigerVNC, WebConsole) can override it to
    ///     enable <see cref="ConfigurationManager.FixAirspace" /> while the dialog is shown.
    /// </summary>
    public virtual void OnProfileManagerDialogOpen()
    {
    }

    /// <summary>
    ///     Called when a dialog in the <see cref="Profiles.ProfileManager" /> is closed.
    /// </summary>
    public virtual void OnProfileManagerDialogClose()
    {
    }

    private void CreateTags()
    {
        var tags = ProfileManager.LoadedProfileFileData.Groups.SelectMany(x => x.Profiles).Where(IsProfileEnabled)
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

    private void SetProfilesView(ProfileFilterInfo filter, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.LoadedProfileFileData.Groups.SelectMany(x => x.Profiles).Where(x => IsProfileEnabled(x) && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                    GetSearchableField(x).IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
                    // If no tags are selected, show all profiles
                    (!filter.Tags.Any()) ||
                    // Any tag can match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.Any &&
                     filter.Tags.Any(tag => x.TagsCollection.Contains(tag))) ||
                    // All tags must match
                    (filter.TagsFilterMatch == ProfileFilterTagsMatch.All &&
                     filter.Tags.All(tag => x.TagsCollection.Contains(tag))))
            ).OrderBy(x => x.Group).ThenBy(x => x.Name)
        }.View;

        Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));

        // Set specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                              Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();
    }

    private void RefreshProfiles()
    {
        if (!_isViewActive)
            return;

        var filter = new ProfileFilterInfo
        {
            Search = Search,
            Tags = [.. ProfileFilterTags.Where(x => x.IsSelected).Select(x => x.Name)],
            TagsFilterMatch = ProfileFilterTagsMatchAny ? ProfileFilterTagsMatch.Any : ProfileFilterTagsMatch.All
        };

        SetProfilesView(filter, SelectedProfile);

        IsProfileFilterSet = !string.IsNullOrEmpty(filter.Search) || filter.Tags.Any();
    }

    #endregion

    #region Event

    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        CreateTags();

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
