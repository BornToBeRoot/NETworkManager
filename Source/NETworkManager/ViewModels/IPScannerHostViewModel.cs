using Dragablz;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
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
/// View model for the IP scanner host view.
/// </summary>
public class IPScannerHostViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private bool _searchDisabled;

    /// <summary>
    /// Gets the client for inter-tab operations.
    /// </summary>
    public IInterTabClient InterTabClient { get; }

    /// <summary>
    /// Backing field for <see cref="InterTabPartition"/>.
    /// </summary>
    private string _interTabPartition;

    /// <summary>
    /// Gets or sets the inter-tab partition key.
    /// </summary>
    public string InterTabPartition
    {
        get => _interTabPartition;
        set
        {
            if (value == _interTabPartition)
                return;

            _interTabPartition = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection of tab items.
    /// </summary>
    public ObservableCollection<DragablzTabItem> TabItems { get; }

    private readonly bool _isLoading;
    private bool _isViewActive = true;

    /// <summary>
    /// Backing field for <see cref="SelectedTabIndex"/>.
    /// </summary>
    private int _selectedTabIndex;

    /// <summary>
    /// Gets or sets the index of the selected tab.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Profiles"/>.
    /// </summary>
    private ICollectionView _profiles;

    /// <summary>
    /// Gets the collection view of profiles.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="SelectedProfile"/>.
    /// </summary>
    private ProfileInfo _selectedProfile = new();

    /// <summary>
    /// Gets or sets the selected profile.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Search"/>.
    /// </summary>
    private string _search;

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

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
    /// Backing field for <see cref="IsSearching"/>.
    /// </summary>
    private bool _isSearching;

    /// <summary>
    /// Gets or sets a value indicating whether a search is in progress.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="ProfileFilterIsOpen"/>.
    /// </summary>
    private bool _profileFilterIsOpen;

    /// <summary>
    /// Gets or sets a value indicating whether the profile filter is open.
    /// </summary>
    public bool ProfileFilterIsOpen
    {
        get => _profileFilterIsOpen;
        set
        {
            if (value == _profileFilterIsOpen)
                return;

            _profileFilterIsOpen = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view for profile filter tags.
    /// </summary>
    public ICollectionView ProfileFilterTagsView { get; }

    /// <summary>
    /// Gets the collection of profile filter tags.
    /// </summary>
    private ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; } = [];

    /// <summary>
    /// Backing field for <see cref="ProfileFilterTagsMatchAny"/>.
    /// </summary>
    private bool _profileFilterTagsMatchAny = GlobalStaticConfiguration.Profile_TagsMatchAny;

    /// <summary>
    /// Gets or sets a value indicating whether any tag match is sufficient for filtering.
    /// </summary>
    public bool ProfileFilterTagsMatchAny
    {
        get => _profileFilterTagsMatchAny;
        set
        {
            if (value == _profileFilterTagsMatchAny)
                return;

            _profileFilterTagsMatchAny = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ProfileFilterTagsMatchAll"/>.
    /// </summary>
    private bool _profileFilterTagsMatchAll;

    /// <summary>
    /// Gets or sets a value indicating whether all tags must match for filtering.
    /// </summary>
    public bool ProfileFilterTagsMatchAll
    {
        get => _profileFilterTagsMatchAll;
        set
        {
            if (value == _profileFilterTagsMatchAll)
                return;

            _profileFilterTagsMatchAll = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="IsProfileFilterSet"/>.
    /// </summary>
    private bool _isProfileFilterSet;

    /// <summary>
    /// Gets or sets a value indicating whether a profile filter is set.
    /// </summary>
    public bool IsProfileFilterSet
    {
        get => _isProfileFilterSet;
        set
        {
            if (value == _isProfileFilterSet)
                return;

            _isProfileFilterSet = value;
            OnPropertyChanged();
        }
    }
    
    private readonly GroupExpanderStateStore _groupExpanderStateStore = new();
    public GroupExpanderStateStore GroupExpanderStateStore => _groupExpanderStateStore;

    private bool _canProfileWidthChange = true;
    private double _tempProfileWidth;

    /// <summary>
    /// Backing field for <see cref="ExpandProfileView"/>.
    /// </summary>
    private bool _expandProfileView;

    /// <summary>
    /// Gets or sets a value indicating whether the profile view is expanded.
    /// </summary>
    public bool ExpandProfileView
    {
        get => _expandProfileView;
        set
        {
            if (value == _expandProfileView)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ExpandProfileView = value;

            _expandProfileView = value;

            if (_canProfileWidthChange)
                ResizeProfile(false);

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ProfileWidth"/>.
    /// </summary>
    private GridLength _profileWidth;

    /// <summary>
    /// Gets or sets the width of the profile view.
    /// </summary>
    public GridLength ProfileWidth
    {
        get => _profileWidth;
        set
        {
            if (value == _profileWidth)
                return;

            if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                GlobalStaticConfiguration.Profile_FloatPointFix) // Do not save the size when collapsed
                SettingsManager.Current.IPScanner_ProfileWidth = value.Value;

            _profileWidth = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScannerHostViewModel"/> class.
    /// </summary>
    public IPScannerHostViewModel()
    {
        _isLoading = true;

        InterTabClient = new DragablzInterTabClient(ApplicationName.IPScanner);
        InterTabPartition = nameof(ApplicationName.IPScanner);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new IPScannerView(tabId), tabId)
        ];

        // Profiles
        CreateTags();

        ProfileFilterTagsView = CollectionViewSource.GetDefaultView(ProfileFilterTags);
        ProfileFilterTagsView.SortDescriptions.Add(new SortDescription(nameof(ProfileFilterTagsInfo.Name),
            ListSortDirection.Ascending));

        SetProfilesView(new ProfileFilterInfo());

        ProfileManager.OnProfilesUpdated += ProfileManager_OnProfilesUpdated;

        _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
        _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        ExpandProfileView = SettingsManager.Current.IPScanner_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.IPScanner_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.IPScanner_ProfileWidth;
    }

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to add a new tab.
    /// </summary>
    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    /// <summary>
    /// Action to add a new tab.
    /// </summary>
    private void AddTabAction()
    {
        AddTab();
    }

    /// <summary>
    /// Gets the command to scan the selected profile.
    /// </summary>
    public ICommand ScanProfileCommand => new RelayCommand(_ => ScanProfileAction(), ScanProfile_CanExecute);

    /// <summary>
    /// Checks if the scan profile command can be executed.
    /// </summary>
    private bool ScanProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    /// <summary>
    /// Action to scan the selected profile.
    /// </summary>
    private void ScanProfileAction()
    {
        AddTab(SelectedProfile);
    }

    /// <summary>
    /// Gets the command to add a new profile.
    /// </summary>
    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    /// <summary>
    /// Action to add a new profile.
    /// </summary>
    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.IPScanner)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if the profile modification commands can be executed.
    /// </summary>
    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    /// <summary>
    /// Gets the command to edit the selected profile.
    /// </summary>
    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    /// <summary>
    /// Action to edit the selected profile.
    /// </summary>
    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to copy the selected profile as a new profile.
    /// </summary>
    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    /// <summary>
    /// Action to copy the selected profile as a new profile.
    /// </summary>
    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to delete the selected profile.
    /// </summary>
    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    /// <summary>
    /// Action to delete the selected profile.
    /// </summary>
    private void DeleteProfileAction()
    {
        ProfileDialogManager
            .ShowDeleteProfileDialog(Application.Current.MainWindow, this, new List<ProfileInfo> { SelectedProfile })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to edit a profile group.
    /// </summary>
    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    /// <summary>
    /// Action to edit a profile group.
    /// </summary>
    private void EditGroupAction(object group)
    {
        ProfileDialogManager
            .ShowEditGroupDialog(Application.Current.MainWindow, this, ProfileManager.GetGroupByName($"{group}"))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to open the profile filter.
    /// </summary>
    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    /// <summary>
    /// Action to open the profile filter.
    /// </summary>
    private void OpenProfileFilterAction()
    {
        ProfileFilterIsOpen = true;
    }

    /// <summary>
    /// Gets the command to apply the profile filter.
    /// </summary>
    public ICommand ApplyProfileFilterCommand => new RelayCommand(_ => ApplyProfileFilterAction());

    /// <summary>
    /// Action to apply the profile filter.
    /// </summary>
    private void ApplyProfileFilterAction()
    {
        RefreshProfiles();

        ProfileFilterIsOpen = false;
    }

    /// <summary>
    /// Gets the command to clear the profile filter.
    /// </summary>
    public ICommand ClearProfileFilterCommand => new RelayCommand(_ => ClearProfileFilterAction());

    /// <summary>
    /// Action to clear the profile filter.
    /// </summary>
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
    /// Gets the command to expand all profile groups.
    /// </summary>
    public ICommand ExpandAllProfileGroupsCommand => new RelayCommand(_ => ExpandAllProfileGroupsAction());

    /// <summary>
    /// Action to expand all profile groups.
    /// </summary>
    private void ExpandAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(true);
    }

    /// <summary>
    /// Gets the command to collapse all profile groups.
    /// </summary>
    public ICommand CollapseAllProfileGroupsCommand => new RelayCommand(_ => CollapseAllProfileGroupsAction());

    /// <summary>
    /// Action to collapse all profile groups.
    /// </summary>
    private void CollapseAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(false);
    }

    /// <summary>
    /// Gets the callback for closing a tab item.
    /// </summary>
    public ItemActionCallback CloseItemCommand => CloseItemAction;

    /// <summary>
    /// Action to close a tab item.
    /// </summary>
    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IPScannerView)?.CloseTab();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets the IsExpanded property for all profile groups.
    /// </summary>
    /// <param name="isExpanded">The value to set.</param>
    private void SetIsExpandedForAllProfileGroups(bool isExpanded)
    {
        foreach (var group in Profiles.Groups.Cast<CollectionViewGroup>())
            GroupExpanderStateStore[group.Name.ToString()] = isExpanded;
    }
    
    /// <summary>
    /// Resizes the profile view.
    /// </summary>
    /// <param name="dueToChangedSize">Indicates whether the resize is due to a size change.</param>
    private void ResizeProfile(bool dueToChangedSize)
    {
        _canProfileWidthChange = false;

        if (dueToChangedSize)
        {
            ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                                GlobalStaticConfiguration.Profile_FloatPointFix;
        }
        else
        {
            if (ExpandProfileView)
            {
                ProfileWidth =
                    Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) <
                    GlobalStaticConfiguration.Profile_FloatPointFix
                        ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded)
                        : new GridLength(_tempProfileWidth);
            }
            else
            {
                _tempProfileWidth = ProfileWidth.Value;
                ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
            }
        }

        _canProfileWidthChange = true;
    }

    /// <summary>
    /// Adds a new tab for the specified host or IP range.
    /// </summary>
    /// <param name="hostOrIPRange">The host or IP range to scan.</param>
    public void AddTab(string hostOrIPRange = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(Strings.NewTab, new IPScannerView(tabId, hostOrIPRange),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    /// <summary>
    /// Adds a new tab for the specified profile.
    /// </summary>
    /// <param name="profile">The profile to scan.</param>
    private void AddTab(ProfileInfo profile)
    {
        AddTab(profile.IPScanner_HostOrIPRange);
    }

    /// <summary>
    /// Called when the view becomes visible.
    /// </summary>
    public void OnViewVisible()
    {
        _isViewActive = true;

        RefreshProfiles();
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
        _isViewActive = false;
    }

    /// <summary>
    /// Creates the profile filter tags.
    /// </summary>
    private void CreateTags()
    {
        var tags = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.IPScanner_Enabled)
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

    /// <summary>
    /// Sets the profiles view with the specified filter.
    /// </summary>
    /// <param name="filter">The profile filter.</param>
    /// <param name="profile">The profile to select.</param>
    private void SetProfilesView(ProfileFilterInfo filter, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.Groups.SelectMany(x => x.Profiles).Where(x => x.IPScanner_Enabled && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1 ||
                    x.IPScanner_HostOrIPRange.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
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

    /// <summary>
    /// Refreshes the profiles.
    /// </summary>
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

    /// <summary>
    /// Handles the OnProfilesUpdated event of the ProfileManager.
    /// </summary>
    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        CreateTags();

        RefreshProfiles();
    }

    /// <summary>
    /// Handles the Tick event of the search dispatcher timer.
    /// </summary>
    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    #endregion
}