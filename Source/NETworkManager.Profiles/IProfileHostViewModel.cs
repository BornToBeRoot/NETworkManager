using NETworkManager.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace NETworkManager.Profiles;

/// <summary>
/// Interface for a view model that hosts the shared profile panel (search, tag filter, group list) next to a
/// tool-specific view, extending <see cref="IProfileManager" /> with the additional members the reusable
/// profile panel control binds to.
/// </summary>
public interface IProfileHostViewModel : IProfileManager
{
    /// <summary>
    /// Gets or sets the selected profile.
    /// </summary>
    ProfileInfo SelectedProfile { get; set; }

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    string Search { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a search is in progress.
    /// </summary>
    bool IsSearching { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the profile filter popup is open.
    /// </summary>
    bool ProfileFilterIsOpen { get; set; }

    /// <summary>
    /// Gets the view for the profile filter tags.
    /// </summary>
    ICollectionView ProfileFilterTagsView { get; }

    /// <summary>
    /// Gets or sets a value indicating whether to match any profile filter tag.
    /// </summary>
    bool ProfileFilterTagsMatchAny { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match all profile filter tags.
    /// </summary>
    bool ProfileFilterTagsMatchAll { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a profile filter is currently applied.
    /// </summary>
    bool IsProfileFilterSet { get; set; }

    /// <summary>
    /// Gets the group expander state store for the profile list.
    /// </summary>
    GroupExpanderStateStore GroupExpanderStateStore { get; }

    /// <summary>
    /// Gets the command to open the profile filter popup.
    /// </summary>
    ICommand OpenProfileFilterCommand { get; }

    /// <summary>
    /// Gets the command to apply the profile filter.
    /// </summary>
    ICommand ApplyProfileFilterCommand { get; }

    /// <summary>
    /// Gets the command to clear the profile filter.
    /// </summary>
    ICommand ClearProfileFilterCommand { get; }

    /// <summary>
    /// Gets the command to expand all profile groups.
    /// </summary>
    ICommand ExpandAllProfileGroupsCommand { get; }

    /// <summary>
    /// Gets the command to collapse all profile groups.
    /// </summary>
    ICommand CollapseAllProfileGroupsCommand { get; }
}
