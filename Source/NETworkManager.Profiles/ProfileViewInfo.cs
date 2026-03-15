using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Profiles;

/// <summary>
/// Represents the information required to display a profile, including the profile name, icon, and associated
/// group.
/// </summary>
/// <remarks>The ProfileViewInfo class provides flexibility for representing a profile's icon by allowing either a
/// pre-existing Canvas or a UIElement to be used. This enables customization of the profile's visual appearance. The
/// class is typically used to encapsulate profile display data for UI scenarios where grouping and icon representation
/// are important.</remarks>
public class ProfileViewInfo
{
    /// <summary>
    /// Initializes a new instance of the ProfileViewInfo class with the specified profile name, icon, and group.
    /// </summary>
    /// <param name="name">The name of the profile to associate with this view. Cannot be null.</param>
    /// <param name="icon">The icon representing the profile. Cannot be null.</param>
    /// <param name="group">The group to which the profile belongs. Cannot be null.</param>
    public ProfileViewInfo(ProfileName name, Canvas icon, ProfileGroup group)
    {
        Name = name;
        Icon = icon;
        Group = group;
    }

    /// <summary>
    /// Initializes a new instance of the ProfileViewInfo class with the specified profile name, UI element, and profile
    /// group.
    /// </summary>
    /// <remarks>The provided UI element is added to a Canvas, allowing for flexible layout and rendering
    /// within the profile view.</remarks>
    /// <param name="name">The name of the profile to associate with this view. Must be a valid ProfileName instance.</param>
    /// <param name="uiElement">The UI element to display within the profile view. Cannot be null.</param>
    /// <param name="group">The group to which this profile belongs. Must be a valid ProfileGroup instance.</param>
    public ProfileViewInfo(ProfileName name, UIElement uiElement, ProfileGroup group)
    {
        Name = name;
        var canvas = new Canvas();
        canvas.Children.Add(uiElement);
        Icon = canvas;
        Group = group;
    }

    /// <summary>
    /// Gets or sets the name of the profile.
    /// </summary>
    /// <remarks>The profile name is used to identify the profile in various operations. It should be unique
    /// within the context of the application.</remarks>
    public ProfileName Name { get; set; }

    /// <summary>
    /// Gets or sets the icon displayed on the canvas.
    /// </summary>
    /// <remarks>This property allows customization of the visual representation of the canvas. The icon can be
    /// used to convey additional information or branding associated with the canvas content.</remarks>
    public Canvas Icon { get; set; }

    /// <summary>
    /// Gets or sets the profile group associated with the user profile.
    /// </summary>
    /// <remarks>This property allows for categorizing user profiles into different groups, which can be used
    /// for managing permissions and settings based on group membership.</remarks>
    public ProfileGroup Group { get; set; }
}
