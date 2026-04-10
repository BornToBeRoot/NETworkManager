using NETworkManager.Utilities;

namespace NETworkManager.Profiles;

/// <summary>
/// Class used to represent a tag filter in the UI (Popup).
/// </summary>
public class ProfileFilterTagsInfo : PropertyChangedBase
{
    /// <summary>
    /// Indicates whether the tag is selected for filtering.
    /// </summary>
    public bool IsSelected
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Tag name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="ProfileFilterTagsInfo"/> with parameters.
    /// </summary>
    /// <param name="isSelected">Indicates whether the tag is selected for filtering.</param>
    /// <param name="name">Tag name.</param>
    public ProfileFilterTagsInfo(bool isSelected, string name)
    {
        IsSelected = isSelected;
        Name = name;
    }
}
