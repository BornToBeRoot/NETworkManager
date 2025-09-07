using NETworkManager.Utilities;

namespace NETworkManager.Profiles;

/// <summary>
/// Class used to represent a tag filter in the UI (Popup).
/// </summary>
public class ProfileTagsFilterInfo : PropertyChangedBase
{
    /// <summary>
    /// Private field for <see cref="IsSelected"/>.
    /// </summary>
    private bool _isSelected;

    /// <summary>
    /// Indicates whether the tag is selected for filtering.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Tag name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="ProfileTagsFilterInfo"/> with parameters.
    /// </summary>
    /// <param name="isSelected">Indicates whether the tag is selected for filtering.</param>
    /// <param name="name">Tag name.</param>
    public ProfileTagsFilterInfo(bool isSelected, string name)
    {
        IsSelected = isSelected;
        Name = name;
    }
}
