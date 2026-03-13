using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETworkManager.ViewModels;

using NETworkManager.Interfaces.ViewModels;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using System.Linq;
using System.Windows.Input;
using Controls;
using Profiles;
using Models;
using Settings;
using Models.Firewall;
using Utilities;
using Localization.Resources;

/// <summary>
/// ViewModel for the Firewall application.
/// </summary>
public class FirewallViewModel : ViewModelBase, IProfileManager, ICloneable, IFirewallViewModel
{
    #region Variables

    public static IFirewallViewModel Instance
    {
        get
        {
            if (field is not null)
                return field;
            field = new FirewallViewModel();
            IFirewallViewModel.SetInstance(field);
            return field;
        }
    }

    /// <summary>
    /// Dispatcher timer for profile searches.
    /// </summary>
    private readonly DispatcherTimer _searchDispatcherTimer = new();

    /// <summary>
    /// Search unavailable.
    /// </summary>
    private bool _searchDisabled;

    /// <summary>
    /// Base Constructor is running. 
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Settings are loaded.
    /// </summary>
    private bool _loadingSettings;

    /// <summary>
    /// View is active.
    /// </summary>
    public bool IsViewActive { get; private set; } = true;

    /// <summary>
    /// The last value of the UseWindowsPortSyntax setting.
    /// </summary>
    private bool _lastUseWindowsPortSyntax;

    /// <summary>
    /// This model has been cloned.
    /// </summary>
    public bool IsClone { get; }

    /// <summary>
    /// Firewall rule currently being selected.
    /// </summary>
    public FirewallRuleViewModel SelectedRule
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
    /// Firewall rule config being saved on application exit.
    /// </summary>
    private List<FirewallRule> FirewallRulesConfig
    {
        set
        {
            if (value == field)
                return;
            field = value;
            if ((!_isLoading || _loadingSettings) && (Profiles?.IsEmpty ?? true) && !IsClone)
                SettingsManager.Current.Firewall_FirewallRules = value;
        }
    }

    public ObservableCollection<IFirewallRuleViewModel> FirewallRulesInterface => new(FirewallRules);

    /// <summary>
    /// Firewall rules being displayed in the UI.
    /// </summary>
    public ObservableCollection<FirewallRuleViewModel> FirewallRules
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FirewallRulesInterface));
        }
    }

    /// <summary>
    /// Currently selected rules, if more than one is selected.
    /// </summary>
    public IList SelectedRules
    {
        get;
        set
        {
            if (value.Equals(field))
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicate whether settings are applied.
    /// </summary>
    private bool IsConfigurationRunning
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

    public int MaxLengthHistory
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

    public string ToolTipAdd => $"{Strings.Firewall_ToolTip_Add}: {Strings.Ctrl}+N";

    public string ToolTipDelete => $"{Strings.Firewall_ToolTip_Delete}: {Strings.Ctrl}+D; {Strings.Del}";

    public string ToolTipClear => $"{Strings.Firewall_ToolTip_Clear}: {Strings.Ctrl}+{Strings.Shift}+C";

    public string ToolTipApply => $"{Strings.Firewall_ToolTip_Apply}: {Strings.Ctrl}+A";

    public string ToolTipOpenWindowsFirewall => $"{Strings.Firewall_ToolTip_OpenWindowsFirewall}: {Strings.Ctrl}+W";

    public string ToolTipClearWindows => $"{Strings.Firewall_ToolTip_ClearWindows}: {Strings.Ctrl}+{Strings.Shift}+{Strings.Alt}+C";
    #region Profiles

    /// <summary>
    /// Collection of user or system profiles associated with the current instance,
    /// providing access to profile-specific data and configurations.
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

    public string ProfileName
    {
        get;
        set
        {
            if (value == field)
                return;
            if (value != null)
            {
                if (SelectedProfile is null)
                {
                    foreach (var rule in FirewallRules)
                    {
                        rule.ProfileName = value;
                        rule.OnRuleChanged += OnRulesChanged;
                        rule.OnAddingPortsToHistory += OnAddingPortsToHistory;
                        rule.OnAddedPortsToHistory += OnAddedPortsToHistory;
                    }

                }
                else
                {
                    FirewallRules = [];
                    foreach (var rule in SelectedProfile.Firewall_Rules)
                    {
                        FirewallRules.Add(new FirewallRuleViewModel(rule, value));
                        var addedRule = FirewallRules.Last();
                        addedRule.OnRuleChanged += OnRulesChanged;
                        addedRule.OnAddingPortsToHistory += OnAddingPortsToHistory;
                        addedRule.OnAddedPortsToHistory += OnAddedPortsToHistory;
                    }
                }
                OnPropertyChanged(nameof(FirewallRules));
            }
            field = value;
        }
    }

    /// <summary>
    /// Represents the currently selected user profile in the system or application,
    /// allowing access to profile-specific settings, preferences, or data.
    /// </summary>
    public ProfileInfo SelectedProfile
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            if (value != null)
                ProfileName = value.Name;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the search query or functionality used to filter or locate specific data
    /// within the context of the containing class.
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
    /// Indicates whether a search operation is currently in progress within the associated context
    /// or functionality of the class.
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
    /// Indicates whether the profile filter panel is currently open or closed.
    /// This property is used to control the visibility state of the profile filter UI element
    /// in the associated view or component.
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
    /// Represents a view model that handles the display and management of profile filter tags
    /// within the application's user interface, enabling users to customize and refine data filtering
    /// based on specific tag selections.
    /// </summary>
    public ICollectionView ProfileFilterTagsView { get; set; }

    /// <summary>
    /// A collection of tags used to filter and categorize user profiles based on specific metadata.
    /// </summary>
    public ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; set; } = [];

    /// <summary>
    /// Specifies whether the profile filter matches any of the tags provided.
    /// This property evaluates to true if any tag within the filter criteria aligns
    /// with the tags associated with the profile being evaluated.
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
    /// Represents the collection of filter tags that must all match to include a profile.
    /// This property enforces strict matching criteria by requiring every tag in the collection
    /// to be satisfied for the profile to be considered valid according to the applied filter.
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
    /// Indicates whether the profile filter has been configured or applied
    /// in the current context.
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
    /// Represents a storage mechanism for maintaining the expanded or collapsed state
    /// of groups within a user interface that supports hierarchical or grouped views.
    /// </summary>
    public GroupExpanderStateStore GroupExpanderStateStore { get; } = new();

    /// <summary>
    /// Indicates whether the profile width can be modified dynamically during runtime.
    /// This variable typically reflects configuration settings or operational conditions
    /// that determine if adjustments to the profile width are allowed.
    /// </summary>
    private bool _canProfileWidthChange = true;

    /// <summary>
    /// Represents the temporary width value of a profile, used for intermediate calculations
    /// or adjustments before being finalized or applied.
    /// </summary>
    private double _tempProfileWidth;

    /// <summary>
    /// Indicates whether the profile view should be expanded, providing a toggle mechanism
    /// for showing or hiding additional profile details in the UI.
    /// </summary>
    public bool ExpandProfileView
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Firewall_ExpandProfileView = value;

            field = value;

            if (_canProfileWidthChange)
                ResizeProfile(false);

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the width of the profile, typically used to define or configure
    /// dimensions in a specific context where profile measurements are required.
    /// </summary>
    public GridLength ProfileWidth
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) >
                GlobalStaticConfiguration.Profile_FloatPointFix) // Do not save the size when collapsed
                SettingsManager.Current.Firewall_ProfileWidth = value.Value;

            field = value;

            if (_canProfileWidthChange)
                ResizeProfile(true);

            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Constructor

    /// <summary>
    /// Encapsulates the logic and data binding required to handle
    /// firewall configuration and visualization in the user interface.
    /// </summary>
    private FirewallViewModel()
    {
        _isLoading = true;

        FirewallRules = [];

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
    /// Get an instance avoiding the base initialization. Solely used on cloning.
    /// </summary>
    /// <param name="isClone">True for cloning.</param>
    private FirewallViewModel(bool isClone)
    {
        if (!isClone)
            return;
        _isLoading = true;
        IsClone = true;
        _isLoading = false;
    }

    #endregion


    #region Methods

    #region Events

    public event CommandExecutedEventHandler CommandExecuted;

    public delegate void CommandExecutedEventHandler(object sender, RoutedEventArgs args);

    /// <summary>
    /// Update config if a rule has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnRulesChanged(object sender, EventArgs e)
    {
        UpdateRulesConfig();
    }

    /// <summary>
    /// Store values if ports are being added to the history.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnAddingPortsToHistory(object sender, EventArgs e)
    {
        if (sender is not FirewallRuleViewModel)
            return;
        foreach (var rule in FirewallRules)
            rule.StorePortValues();
    }

    /// <summary>
    /// Restore port values after the history has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnAddedPortsToHistory(object sender, EventArgs e)
    {
        if (sender is not FirewallRuleViewModel)
            return;
        foreach (var rule in FirewallRules)
            rule.RestorePortValues();
    }

    /// <summary>
    /// Toggle the view.
    /// </summary>
    public void OnViewHide()
    {
        IsViewActive = !IsViewActive;
        if (IsViewActive)
            UpdatePortHistorySeparator();
    }

    /// <summary>
    /// Method triggered when the associated view becomes visible
    /// because the settings dialog has been closed.
    /// </summary>
    public void OnViewVisible()
    {
        IsViewActive = true;
        bool combined = SettingsManager.Current.Firewall_CombinePortHistory;
        if (SettingsManager.Current.Firewall_LocalPortsHistoryConfig.Count
            < FirewallRuleViewModel.LocalPortsHistory.Count)
        {
            FirewallRuleViewModel.LocalPortsHistory.Clear();
            if (combined)
                FirewallRuleViewModel.CombinedPortsHistory.Clear();
        }
        if (SettingsManager.Current.Firewall_RemotePortsHistoryConfig.Count
            < FirewallRuleViewModel.RemotePortsHistory.Count)
        {
            FirewallRuleViewModel.RemotePortsHistory.Clear();
            if (combined)
                FirewallRuleViewModel.CombinedPortsHistory.Clear();
        }
        MaxLengthHistory = SettingsManager.Current.Firewall_MaxLengthHistory;
        foreach (var rule in FirewallRules)
        {
            if (combined)
            {
                rule.StorePortValues();
                rule.LocalPortsHistoryView = CollectionViewSource
                    .GetDefaultView(FirewallRuleViewModel.CombinedPortsHistory);
                rule.RemotePortsHistoryView = CollectionViewSource
                    .GetDefaultView(FirewallRuleViewModel.CombinedPortsHistory);
            }
            else
            {
                rule.LocalPortsHistoryView = CollectionViewSource
                    .GetDefaultView(FirewallRuleViewModel.LocalPortsHistory);
                rule.RemotePortsHistoryView = CollectionViewSource
                    .GetDefaultView(FirewallRuleViewModel.RemotePortsHistory);
            }
            rule.MaxLengthHistory = SettingsManager.Current.Firewall_MaxLengthHistory;
        }
        // Also calls FirewallRuleViewModel.UpdateCombinedPortHistory()
        UpdatePortHistorySeparator();
        if (!combined) return;
        foreach (var rule in FirewallRules)
            rule.RestorePortValues();
    }

    /// Handles the ProfilesUpdated event from the ProfileManager.
    /// This method is triggered when the profiles are updated and ensures the profile tags are refreshed and the profiles view is updated.
    /// <param name="sender">The source of the event, typically the ProfileManager instance.</param>
    /// <param name="e">An EventArgs object that contains no event data.</param>
    private void ProfileManager_OnProfilesUpdated(object sender, EventArgs e)
    {
        CreateTags();

        RefreshProfiles();
    }

    /// <summary>
    /// Handles the Tick event of the SearchDispatcherTimer.
    /// Responsible for executing periodic operations related to the search functionality,
    /// such as updating search results or triggering dependent actions at the timer's interval.
    /// </summary>
    /// <param name="sender">The source of the event, typically the timer object.</param>
    /// <param name="e">An instance of EventArgs containing event data.</param>
    private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
    {
        _searchDispatcherTimer.Stop();

        RefreshProfiles();

        IsSearching = false;
    }

    public void OnProfilesLoaded()
    {
        var firstGroupProfiles = (Profiles?.Groups?.FirstOrDefault() as CollectionViewGroup)?.Items;
        if (firstGroupProfiles?.Count is 0 or null)
            LoadRulesFromSettings();
        else
            SelectedProfile = firstGroupProfiles.FirstOrDefault() as ProfileInfo;
    }

    #endregion Events

    #region ProfileMethods

    /// <summary>
    /// Provides functionality to manage and adjust the size of a user profile
    /// or related visual elements based on specified parameters or constraints.
    /// </summary>
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
    /// Populates and updates the list of profile filter tags based on the tags present
    /// in the profiles where the firewall is enabled.
    /// </summary>
    /// <remarks>
    /// This method retrieves all tags associated with profiles in all groups
    /// that have the firewall enabled. It ensures the `ProfileFilterTags` collection
    /// is synchronized with the current set of tags, removing any outdated tags and
    /// adding any new ones. Duplicate tags are not allowed, and the tags are maintained
    /// in a consistent state.
    /// </remarks>
    private void CreateTags()
    {
        var tags = ProfileManager.LoadedProfileFileData?.Groups.SelectMany(x => x.Profiles).Where(x => x.Firewall_Enabled)
            .SelectMany(x => x.TagsCollection).Distinct().ToList();
        if (tags is null)
            return;
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
    /// Represents the View responsible for configuring and displaying
    /// profile settings within the application.
    /// </summary>
    private void SetProfilesView(ProfileFilterInfo filter, ProfileInfo profile = null)
    {
        Profiles = new CollectionViewSource
        {
            Source = ProfileManager.LoadedProfileFileData?
                .Groups.SelectMany(x => x.Profiles).Where(x => x.Firewall_Enabled && (
                    string.IsNullOrEmpty(filter.Search) ||
                    x.Name.IndexOf(filter.Search, StringComparison.OrdinalIgnoreCase) > -1) && (
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

        // Set the specific profile or first if null
        SelectedProfile = null;

        if (profile != null)
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault(x => x.Equals(profile)) ??
                              Profiles.Cast<ProfileInfo>().FirstOrDefault();
        else
            SelectedProfile = Profiles.Cast<ProfileInfo>().FirstOrDefault();
    }

    /// <summary>
    /// Refreshes the list of user profiles by retrieving the latest data
    /// and updating the application state accordingly.
    /// </summary>
    private void RefreshProfiles()
    {
        if (!IsViewActive)
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

    #region Action and commands
    #region ProfileCommands
    /// <summary>
    /// Command responsible for applying the profile configuration settings.
    /// This command encapsulates the logic required to execute changes to
    /// user-defined or system-defined profile configurations.
    /// </summary>
    public ICommand ApplyProfileConfigCommand => new RelayCommand(_ => ApplyProfileProfileAction());

    /// <summary>
    /// Executes the action that applies the configuration settings stored in the selected profile.
    /// </summary>
    /// <remarks>
    /// This method is invoked as part of the ApplyProfileConfigCommand, which is bound to a user action
    /// in the view layer. It asynchronously applies configuration settings from the currently
    /// selected profile to the appropriate system components.
    /// </remarks>
    private void ApplyProfileProfileAction()
    {
        ApplyConfigurationFromProfile().ConfigureAwait(false);
    }

    /// <summary>
    /// Command used to initiate the process of adding a new profile,
    /// facilitating the binding between the user interface and the
    /// corresponding business logic.
    /// </summary>
    public ICommand AddProfileCommand => new RelayCommand(_ => AddProfileAction());

    /// <summary>
    /// Represents an action that handles the addition of a new profile,
    /// including the associated logic and state management necessary
    /// to persist and update profile-related information.
    /// </summary>
    private void AddProfileAction()
    {
        ProfileDialogManager
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.Firewall)
            .ConfigureAwait(false);
    }

    private bool ModifyProfile_CanExecute(object obj)
    {
        return SelectedProfile is { IsDynamic: false };
    }

    /// <summary>
    /// Command responsible for handling the execution of the profile editing functionality
    /// within the associated view model.
    /// </summary>
    public ICommand EditProfileCommand => new RelayCommand(_ => EditProfileAction(), ModifyProfile_CanExecute);

    /// <summary>
    /// Handles the action to edit the currently selected profile in the application.
    /// This method invokes the profile editing dialog through the <see cref="ProfileDialogManager.ShowEditProfileDialog"/>
    /// and passes the current application window, the view model, and the selected profile to it.
    /// The dialog allows users to modify the details of the selected profile.
    /// </summary>
    private void EditProfileAction()
    {
        ProfileDialogManager.ShowEditProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Command encapsulating the logic to copy the current configuration or selected
    /// settings as a new profile within the application.
    /// </summary>
    public ICommand CopyAsProfileCommand => new RelayCommand(_ => CopyAsProfileAction(), ModifyProfile_CanExecute);

    /// <summary>
    /// Defines an action responsible for creating a copy of an existing profile
    /// in a system or application, preserving all configurations and settings
    /// associated with the original profile.
    /// </summary>
    private void CopyAsProfileAction()
    {
        ProfileDialogManager.ShowCopyAsProfileDialog(Application.Current.MainWindow, this, SelectedProfile)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Command that facilitates the deletion of a user profile
    /// by encapsulating the associated logic and providing
    /// the necessary bindings for UI interaction.
    /// </summary>
    public ICommand DeleteProfileCommand => new RelayCommand(_ => DeleteProfileAction(), ModifyProfile_CanExecute);

    /// <summary>
    /// Represents an action responsible for handling the deletion of a user or application profile,
    /// including any associated resources or configurations tied to that profile.
    /// </summary>
    private async void DeleteProfileAction()
    {
        try
        {
            await ProfileDialogManager
                .ShowDeleteProfileDialog(Application.Current.MainWindow, this,
                    new List<ProfileInfo> { SelectedProfile })
                .ConfigureAwait(false);
            // Reload rules from configuration on last profile deletion
            if (Profiles?.IsEmpty ?? true)
            {
                SelectedProfile = null;
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(LoadRulesFromSettings));
                ProfileName = null;
            }

            UpdateRulesConfig();
        }
        catch
        {
            // Prevent a process crash on errors.
        }
    }

    /// <summary>
    /// Command responsible for handling the editing of a group, typically invoked to open
    /// or manage the group editing interface and apply updated changes.
    /// </summary>
    public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

    private void EditGroupAction(object group)
    {
        ProfileDialogManager
            .ShowEditGroupDialog(Application.Current.MainWindow, this, ProfileManager.GetGroupByName($"{group}"))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Command that encapsulates the logic for opening and applying a profile filter
    /// within the associated view model, allowing dynamic adjustments to the profile display.
    /// </summary>
    public ICommand OpenProfileFilterCommand => new RelayCommand(_ => OpenProfileFilterAction());

    /// <summary>
    /// Defines an action responsible for filtering profiles based on specific criteria
    /// within a system or application, enabling targeted operations on the resulting set of profiles.
    /// </summary>
    private void OpenProfileFilterAction()
    {
        ProfileFilterIsOpen = true;
    }

    /// <summary>
    /// Command responsible for applying a profile-based filter to the data
    /// or view model, enabling targeted operations based on the selected profile criteria.
    /// </summary>
    public ICommand ApplyProfileFilterCommand => new RelayCommand(_ => ApplyProfileFilterAction());

    /// <summary>
    /// Defines an action responsible for applying a specific profile-based filter
    /// to a data set or collection, modifying its state based on the selected profile criteria.
    /// </summary>
    private void ApplyProfileFilterAction()
    {
        RefreshProfiles();

        ProfileFilterIsOpen = false;
    }

    /// <summary>
    /// Command used to clear the applied profile filters, resetting the view or data
    /// to its unfiltered state within the context of the associated ViewModel.
    /// </summary>
    public ICommand ClearProfileFilterCommand => new RelayCommand(_ => ClearProfileFilterAction());

    /// <summary>
    /// Represents an action responsible for clearing the applied profile filters,
    /// resetting the state to display unfiltered results.
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
    /// Command used to trigger the expansion of all profile groups
    /// in the associated view model, typically for improving visibility
    /// and accessibility of group details.
    /// </summary>
    public ICommand ExpandAllProfileGroupsCommand => new RelayCommand(_ => ExpandAllProfileGroupsAction());

    /// <summary>
    /// Represents the action responsible for expanding all profile groups
    /// within a given context, typically used to enhance visibility or access
    /// to grouped profile information in the UI.
    /// </summary>
    private void ExpandAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(true);
    }

    /// <summary>
    /// Command responsible for collapsing all profile groups in the associated view model,
    /// typically used to improve user navigation and focus by reducing the visible complexity.
    /// </summary>
    public ICommand CollapseAllProfileGroupsCommand => new RelayCommand(_ => CollapseAllProfileGroupsAction());

    /// Executes the action to collapse all profile groups in the firewall view.
    /// This method sets the expansion state of all profile groups to false, effectively collapsing them.
    /// Remarks:
    /// - It uses the `SetIsExpandedForAllProfileGroups` method to modify the expansion state.
    /// - This action is typically invoked by the related command bound to the UI.
    /// Dependencies:
    /// - Requires access to the profile group expansion state via `SetIsExpandedForAllProfileGroups
    private void CollapseAllProfileGroupsAction()
    {
        SetIsExpandedForAllProfileGroups(false);
    }

    /// <summary>
    /// Updates the expanded state for all profile groups in the data set.
    /// </summary>
    /// <param name="isExpanded">A boolean value indicating whether all profile groups should be expanded (true) or collapsed (false).</param>
    private void SetIsExpandedForAllProfileGroups(bool isExpanded)
    {
        foreach (var group in Profiles.Groups.Cast<CollectionViewGroup>())
            GroupExpanderStateStore[group.Name.ToString()] = isExpanded;
    }
    #endregion ProfileCommands

    /// <summary>
    /// Command responsible for applying the current configuration settings,
    /// ensuring the changes are validated and executed within the context of the application.
    /// </summary>
    public ICommand ApplyConfigurationCommand =>
        new RelayCommand(_ => ApplyConfigurationAction(), ApplyConfiguration_CanExecute);

    /// <summary>
    /// Determines whether the apply configuration command can execute
    /// based on the current state of the application.
    /// </summary>
    /// <param name="parameter">An optional parameter used for evaluating the command's execution status.</param>
    /// <returns>True if the command can execute; otherwise, false.</returns>
    private static bool ApplyConfiguration_CanExecute(object parameter)
    {
        return !(Application.Current.MainWindow as MetroWindow)?.IsAnyDialogOpen ?? false;
        //!ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>
    /// Represents an action responsible for applying configuration settings
    /// to a specified system or component. It encapsulates the logic required
    /// to validate, process, and enforce the provided configuration details.
    /// </summary>
    private void ApplyConfigurationAction()
    {
        ApplyConfiguration().ConfigureAwait(false);
        CommandExecuted?.Invoke(this, null);
    }

    public ICommand OpenWindowsFirewallCommand => new RelayCommand(_ => OpenWindowsFirewallAction());

    private void OpenWindowsFirewallAction()
    {
        if (IsClone)
        {
            CommandExecuted?.Invoke(this, null);
            return;
        }
        PowerShellHelper.ExecuteCommand("WF.msc");
        CommandExecuted?.Invoke(this, null);
    }

    public ICommand DeleteAllRulesCommand => new RelayCommand(_ => DeleteAllRulesAction());

    private void DeleteAllRulesAction()
    {
        FirewallRules.Clear();
        UpdateRulesConfig();
        CommandExecuted?.Invoke(this, null);
    }

    /// <summary>
    /// Command for <see cref="AddRuleAction" />.
    /// </summary>
    public ICommand AddRuleCommand => new RelayCommand(_ => AddRuleAction());

    /// <summary>
    /// Action for <see cref="AddRule" />.
    /// </summary>
    private void AddRuleAction()
    {
        AddRule();
    }

    /// <summary>
    /// Adds a new firewall rule.
    /// </summary>
    private void AddRule()
    {
        FirewallRules ??= [];

        FirewallRules.Add(new FirewallRuleViewModel { ProfileName = ProfileName });
        var addedRule = FirewallRules.Last();
        addedRule.OnRuleChanged += OnRulesChanged;
        addedRule.OnAddedPortsToHistory += OnAddedPortsToHistory;
        addedRule.OnAddingPortsToHistory += OnAddingPortsToHistory;
        UpdateRulesConfig();
        CommandExecuted?.Invoke(this, null);
    }

    /// <summary>
    /// Command responsible for handling the deletion of rules within the context
    /// of the associated ViewModel or business logic. Encapsulates the logic
    /// required to execute the rule removal process.
    /// </summary>
    public ICommand DeleteRulesCommand => new RelayCommand(_ => DeleteRulesAction());

    /// <summary>
    /// Represents an action responsible for handling the deletion of rules
    /// within a specific system or application context.
    /// </summary>
    private void DeleteRulesAction()
    {
        DeleteRules();
        CommandExecuted?.Invoke(this, null);
    }

    /// <summary>
    /// Deletes the selected firewall rules from the collection of firewall rules.
    /// </summary>
    private void DeleteRules()
    {
        if (SelectedRules?.Count > 0)
        {
            var rulesToDelete = SelectedRules.Cast<FirewallRuleViewModel>().ToList();
            foreach (FirewallRuleViewModel rule in rulesToDelete)
            {
                rule.OnRuleChanged -= OnRulesChanged;
                rule.OnAddingPortsToHistory -= OnAddingPortsToHistory;
                rule.OnAddedPortsToHistory -= OnAddedPortsToHistory;
                FirewallRules?.Remove(rule);
            }
            UpdateRulesConfig();
            return;
        }

        var ruleToDelete = SelectedRule ?? FirewallRules?.LastOrDefault();
        ruleToDelete?.OnRuleChanged -= OnRulesChanged;
        ruleToDelete?.OnAddingPortsToHistory -= OnAddingPortsToHistory;
        ruleToDelete?.OnAddedPortsToHistory -= OnAddedPortsToHistory;
        // May be null, but Remove() handles this returning false.
        FirewallRules?.Remove(ruleToDelete);
        UpdateRulesConfig();
    }

    /// <summary>
    /// Command for deleting the shown rules in the Windows firewall.
    /// </summary>
    public static ICommand DeleteWindowsRulesCommand => new RelayCommand(_ => DeleteWindowsRulesAction());

    /// <summary>
    /// Clear the Windows firewall rules starting with "NwM_".
    /// </summary>
    private static void DeleteWindowsRulesAction()
    {
        Firewall.ClearAllRules();
    }

    #endregion

    #region Configuration

    /// <summary>
    /// Load the settings from <see cref="SettingsManager.Current" />.
    /// </summary>
    private void LoadSettings()
    {
        _loadingSettings = true;
        // Load port history
        var localPortHistory = SettingsManager.Current.Firewall_LocalPortsHistoryConfig;
        if (localPortHistory?.Count > 0)
            FirewallRuleViewModel.LocalPortsHistory = new ObservableCollection<string>(localPortHistory);
        var remotePortsHistory = SettingsManager.Current.Firewall_RemotePortsHistoryConfig;
        if (remotePortsHistory?.Count > 0)
            FirewallRuleViewModel.RemotePortsHistory = new ObservableCollection<string>(remotePortsHistory);
        if (SettingsManager.Current.Firewall_CombinePortHistory)
            FirewallRuleViewModel.UpdateCombinedPortsHistory();
        MaxLengthHistory = SettingsManager.Current.Firewall_MaxLengthHistory;
        // Set a profile name
        if (SelectedProfile is not null)
            ProfileName = SelectedProfile.Name;

        // Load firewall rules from settings if no profile is selected
        if (SelectedProfile is null)
            LoadRulesFromSettings();

        _loadingSettings = false;

        _lastUseWindowsPortSyntax = SettingsManager.Current.Firewall_UseWindowsPortSyntax;

        // Load profile view settings
        ExpandProfileView = SettingsManager.Current.Firewall_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.Firewall_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.Firewall_ProfileWidth;
    }

    /// <summary>
    /// Load the firewall rules from settings if no profiles are available.
    /// </summary>
    private void LoadRulesFromSettings()
    {
        SelectedProfile = null;
        ProfileName = null;
        FirewallRules = [];
        if (SettingsManager.Current.Firewall_FirewallRules is null)
            return;
        foreach (var rule in SettingsManager.Current.Firewall_FirewallRules)
        {
            FirewallRules.Add(new FirewallRuleViewModel(rule, SelectedProfile?.Name));
            var addedRule = FirewallRules.Last();
            addedRule.OnRuleChanged += OnRulesChanged;
            addedRule.OnAddingPortsToHistory += OnAddingPortsToHistory;
            addedRule.OnAddedPortsToHistory += OnAddedPortsToHistory;
        }
        OnPropertyChanged(nameof(FirewallRules));
    }

    /// <summary>
    /// Update the configuration for either the current profile or the default config
    /// if no profile is selected.
    /// </summary>
    private void UpdateRulesConfig()
    {
        if ((Profiles?.IsEmpty ?? true) && !IsClone)
        {
            FirewallRulesConfig = FirewallRules?.Select(x => x?.ToRule(true)).ToList();
        }
        else if (!IsClone)
        {
            SelectedProfile?.Firewall_Rules = FirewallRules?.Select(x => x?.ToRule(true)).ToList();
            ProfileManager.LoadedProfileFileData?.ProfilesChanged = true;
        }
        OnPropertyChanged(nameof(FirewallRules));
        OnPropertyChanged(nameof(FirewallRulesInterface));
    }

    /// <summary>
    /// Replace the separator in the port histories if the settings have been changed.
    /// </summary>
    private void UpdatePortHistorySeparator()
    {
        // Check whether UseWindowsPortSyntax has been changed.
        var currentWindowsPortSyntax = SettingsManager.Current.Firewall_UseWindowsPortSyntax;
        if (_lastUseWindowsPortSyntax == currentWindowsPortSyntax)
            return;
        // Replace history separators
        var localPortsHistory = FirewallRuleViewModel.LocalPortsHistory;
        var remotePortsHistory = FirewallRuleViewModel.RemotePortsHistory;
        char fromSeparator = currentWindowsPortSyntax ? ';' : ',';
        char toSeparator = currentWindowsPortSyntax ? ',' : ';';
        if (localPortsHistory != null)
        {
            FirewallRuleViewModel.LocalPortsHistory =
                new ObservableCollection<string>(localPortsHistory
                    .Select(x => x?.Replace(fromSeparator, toSeparator)));
        }

        if (remotePortsHistory != null)
        {
            FirewallRuleViewModel.RemotePortsHistory =
                new ObservableCollection<string>(remotePortsHistory
                    .Select(x => x?.Replace(fromSeparator, toSeparator)));
        }

        // Update the combined ports history if enabled
        foreach (var rule in FirewallRules)
        {
            if (SettingsManager.Current.Firewall_CombinePortHistory)
                FirewallRuleViewModel.UpdateCombinedPortsHistory();
            rule.PortWatermark = rule.PortWatermark?.Replace(fromSeparator, toSeparator);
        }

        _lastUseWindowsPortSyntax = currentWindowsPortSyntax;
    }

    /// <summary>
    /// Apply the firewall rules to Windows firewall configuration.
    /// </summary>
    private async Task ApplyConfiguration()
    {
        if (IsConfigurationRunning || IsClone)
            return;
        IsConfigurationRunning = true;

        try
        {
            var firewall = new Firewall();

            var firewallRules = FirewallRules
                .Select(ruleVm => ruleVm.ToRule()).Where(r => r != null).ToList();

            await firewall.ApplyRulesAsync(firewallRules);
        }
        finally
        {
            IsConfigurationRunning = false;
        }
    }

    /// <summary>
    /// Apply the firewall rules from a profile to the Windows firewall configuration.
    /// </summary>
    private async Task ApplyConfigurationFromProfile()
    {
        if (SelectedProfile is null)
            return;
        if (IsConfigurationRunning)
            return;
        IsConfigurationRunning = true;

        foreach (var rule in SelectedProfile.Firewall_Rules)
        {
            FirewallRules =
            [
                new FirewallRuleViewModel(rule, ProfileName)
            ];

            await ApplyConfiguration();
        }
    }

    #endregion

    /// <summary>
    /// Clone this instance.
    /// </summary>
    /// <returns>Cloned instance.</returns>
    public object Clone()
    {
        var clone = new FirewallViewModel(true)
        {
            FirewallRules = new ObservableCollection<FirewallRuleViewModel>(
                FirewallRules.Select(rule => rule.Clone() as FirewallRuleViewModel)),
        };
        return clone;
    }
    #endregion Methods

}