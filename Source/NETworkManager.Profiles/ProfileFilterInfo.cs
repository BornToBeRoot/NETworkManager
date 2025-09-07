using System.Collections.Generic;

namespace NETworkManager.Profiles;

/// <summary>
/// Class used to filter profiles in the UI.
/// </summary>
public class ProfileFilterInfo
{
    /// <summary>
    /// Search string for filtering profiles.
    /// </summary>
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// Tags for filtering profiles.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = [];

    /// <summary>
    /// Indicates whether to match any or all tags in the filter.
    /// </summary>
    public ProfileFilterTagsMatch TagsFilterMatch { get; set; } = ProfileFilterTagsMatch.Any;
}
