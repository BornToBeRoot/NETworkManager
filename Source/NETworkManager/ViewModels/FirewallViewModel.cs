using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Firewall;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

using Controls;
using Profiles;
using Models;

/// <summary>
/// ViewModel for the Firewall application.
/// </summary>
public class FirewallViewModel : ViewModelBase, IProfileManager
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(FirewallViewModel));

    private readonly DispatcherTimer _searchDispatcherTimer = new();
    private bool _searchDisabled;
    private readonly bool _isLoading;
    private bool _isViewActive = true;

    #region Rules

    /// <summary>
    /// Gets the loaded firewall rules.
    /// </summary>
    public ObservableCollection<FirewallRule> Results { get; } = [];

    /// <summary>
    /// Gets the filtered/sorted view over <see cref="Results"/>.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Gets or sets the currently selected firewall rule.
    /// </summary>
    public FirewallRule SelectedResult
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
    /// Gets or sets the list of selected firewall rules (multi-select).
    /// </summary>
    public IList SelectedResults
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

    /// <summary>
    /// Gets or sets the search text for filtering rules.
    /// </summary>
    public string RulesSearch
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            ResultsView.Refresh();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether a refresh is currently running.
    /// </summary>
    public bool IsRefreshing
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
    /// Gets or sets whether the status message bar is shown.
    /// </summary>
    public bool IsStatusMessageDisplayed
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
    /// Gets or sets the status message text.
    /// </summary>
    public string StatusMessage
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

    #endregion

    #region Profiles

    /// <summary>
    /// Gets the collection view of profiles.
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
    /// Gets or sets the selected profile.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the search text.
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
    /// Gets or sets a value indicating whether a search is in progress.
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
    /// Gets or sets a value indicating whether the profile filter is open.
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
    /// Gets the collection view for profile filter tags.
    /// </summary>
    public ICollectionView ProfileFilterTagsView { get; }

    /// <summary>
    /// Gets the collection of profile filter tags.
    /// </summary>
    private ObservableCollection<ProfileFilterTagsInfo> ProfileFilterTags { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether any tag match is sufficient for filtering.
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
    /// Gets or sets a value indicating whether all tags must match for filtering.
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
    /// Gets or sets a value indicating whether a profile filter is set.
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

    public GroupExpanderStateStore GroupExpanderStateStore { get; } = new();

    private bool _canProfileWidthChange = true;
    private double _tempProfileWidth;

    /// <summary>
    /// Gets or sets a value indicating whether the profile view is expanded.
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
    /// Gets or sets the width of the profile view.
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

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="FirewallViewModel"/> class.
    /// </summary>
    public FirewallViewModel()
    {
        _isLoading = true;

        // Rules
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(RulesSearch))
                return true;

            if (o is not FirewallRule rule)
                return false;

            return rule.Name.IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   rule.Protocol.ToString().IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   rule.Action.ToString().IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   rule.Direction.ToString().IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Load firewall rules
        Refresh(true).ConfigureAwait(false);

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
        ExpandProfileView = SettingsManager.Current.Firewall_ExpandProfileView;

        ProfileWidth = ExpandProfileView
            ? new GridLength(SettingsManager.Current.Firewall_ProfileWidth)
            : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

        _tempProfileWidth = SettingsManager.Current.Firewall_ProfileWidth;
    }

    #endregion

    #region ICommand & Actions

    /// <summary>Gets the command to refresh the firewall rules.</summary>
    public ICommand RefreshCommand => new RelayCommand(_ => RefreshAction().ConfigureAwait(false), Refresh_CanExecute);

    private bool Refresh_CanExecute(object _) => !IsRefreshing;

    private async Task RefreshAction() => await Refresh();

    /// <summary>Gets the command to add a new firewall entry.</summary>
    public ICommand AddEntryCommand => new RelayCommand(_ => AddEntryAction());

    private void AddEntryAction()
    {
        // TODO: open AddFirewallRuleDialog
    }

    /// <summary>Gets the command to enable the selected firewall entry.</summary>
    public ICommand EnableEntryCommand => new RelayCommand(_ => EnableEntryAction(), _ => ModifyEntry_CanExecute() && SelectedResult is { IsEnabled: false });

    private void EnableEntryAction()
    {
        // TODO: call Firewall.SetRuleEnabledAsync(SelectedResult, true)
    }

    /// <summary>Gets the command to disable the selected firewall entry.</summary>
    public ICommand DisableEntryCommand => new RelayCommand(_ => DisableEntryAction(), _ => ModifyEntry_CanExecute() && SelectedResult is { IsEnabled: true });

    private void DisableEntryAction()
    {
        // TODO: call Firewall.SetRuleEnabledAsync(SelectedResult, false)
    }

    /// <summary>Gets the command to edit the selected firewall entry.</summary>
    public ICommand EditEntryCommand => new RelayCommand(_ => EditEntryAction(), _ => ModifyEntry_CanExecute() && SelectedResult != null);

    private void EditEntryAction()
    {
        // TODO: open EditFirewallRuleDialog
    }

    /// <summary>Gets the command to delete the selected firewall entry.</summary>
    public ICommand DeleteEntryCommand => new RelayCommand(_ => DeleteEntryAction(), _ => ModifyEntry_CanExecute() && SelectedResult != null);

    private void DeleteEntryAction()
    {
        // TODO: confirm and call Firewall.DeleteRuleAsync
    }

    /// <summary>Checks if entry modification commands can be executed.</summary>
    private static bool ModifyEntry_CanExecute()
    {
        return ConfigurationManager.Current.IsAdmin &&
               Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    /// <summary>Gets the command to restart the application as administrator.</summary>
    public ICommand RestartAsAdminCommand => new RelayCommand(_ => RestartAsAdminAction().ConfigureAwait(false));

    private async Task RestartAsAdminAction()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.RestartApplication(true);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    /// <summary>Gets the command to export the firewall rules.</summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private Task ExportAction()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<FirewallRule>(SelectedResults.Cast<FirewallRule>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Firewall_ExportFileType = instance.FileType;
            SettingsManager.Current.Firewall_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.Firewall_ExportFileType,
        SettingsManager.Current.Firewall_ExportFilePath);

        childWindow.Title = Strings.Export;
        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to apply the selected profile configuration.
    /// </summary>
    public ICommand ApplyProfileCommand => new RelayCommand(_ => ApplyProfileAction());

    /// <summary>
    /// Action to apply the selected profile configuration.
    /// </summary>
    private void ApplyProfileAction()
    {
        MessageBox.Show("Not implemented");
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
            .ShowAddProfileDialog(Application.Current.MainWindow, this, null, null, ApplicationName.Firewall)
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
    /// Action to copy the selected profile as a new profile.
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
    
    #region Additional commands
    
    /// <summary>
    /// Gets the command to open the Windows Firewall management console (WF.msc).
    /// </summary>
    public ICommand OpenWindowsFirewallCommand => new RelayCommand(_ => OpenWindowsFirewallAction().ConfigureAwait(false));

    /// <summary>
    /// Action to open the Windows Firewall management console (WF.msc).
    /// Shows an error dialog if the process cannot be started.
    /// </summary>
    private async Task OpenWindowsFirewallAction()
    {
        try
        {
            ExternalProcessStarter.RunProcess("WF.msc");
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    #endregion
    
    #endregion

    #region Methods

    /// <summary>
    /// Loads firewall rules from Windows via PowerShell and populates <see cref="Results"/>.
    /// </summary>
    private async Task Refresh(bool init = false)
    {
        if (IsRefreshing)
            return;

        IsRefreshing = true;
        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        if (!init)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        try
        {
            var rules = await Firewall.GetRulesAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Results.Clear();

                foreach (var rule in rules)
                    Results.Add(rule);
            });

            StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
            IsStatusMessageDisplayed = true;
        }
        catch (Exception ex)
        {
            Log.Error("Error while loading firewall rules", ex);

            StatusMessage = string.Format(Strings.FailedToLoadFirewallRulesMessage, ex.Message);
            IsStatusMessageDisplayed = true;
        }

        IsRefreshing = false;
    }

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
        var tags = ProfileManager.LoadedProfileFileData.Groups.SelectMany(x => x.Profiles).Where(x => x.Firewall_Enabled)
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
            Source = ProfileManager.LoadedProfileFileData.Groups.SelectMany(x => x.Profiles).Where(x => x.Firewall_Enabled && (
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
    
    #region Events
    
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
