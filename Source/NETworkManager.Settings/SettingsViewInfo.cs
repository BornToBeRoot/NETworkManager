using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Settings;

/// <summary>
/// Represents the view information for a settings item, including its name, icon, and associated group.
/// </summary>
/// <remarks>This class provides constructors to initialize the settings view with either a Canvas icon or a
/// UIElement. The icon is displayed in the settings interface, and the group categorizes the settings item.</remarks>
public class SettingsViewInfo
{
    /// <summary>
    /// Initializes a new instance of the SettingsViewInfo class with the specified settings view name, icon, and group.
    /// </summary>
    /// <param name="name">The name of the settings view to associate with this instance. Must be a valid value from the SettingsName
    /// enumeration.</param>
    /// <param name="icon">The icon to display for the settings view. Represents a graphical element of type Canvas.</param>
    /// <param name="group">The group to which the settings view belongs. Must be a valid value from the SettingsGroup enumeration.</param>
    public SettingsViewInfo(SettingsName name, Canvas icon, SettingsGroup group)
    {
        Name = name;
        Icon = icon;
        Group = group;
    }

    /// <summary>
    /// Initializes a new instance of the SettingsViewInfo class with the specified settings name, UI element, and
    /// settings group.
    /// </summary>
    /// <remarks>The UI element is added to a Canvas, which serves as the container for the settings view's
    /// visual representation.</remarks>
    /// <param name="name">The name of the settings, represented by the SettingsName enumeration.</param>
    /// <param name="uiElement">The UI element to be displayed within the settings view.</param>
    /// <param name="group">The group to which this settings view belongs, represented by the SettingsGroup enumeration.</param>
    public SettingsViewInfo(SettingsName name, UIElement uiElement, SettingsGroup group)
    {
        Name = name;
        var canvas = new Canvas();
        canvas.Children.Add(uiElement);
        Icon = canvas;
        Group = group;
    }

    /// <summary>
    /// Gets or sets the name of the settings.
    /// </summary>
    public SettingsName Name { get; set; }

    /// <summary>
    /// Gets or sets the icon displayed on the canvas.
    /// </summary>
    /// <remarks>This property allows customization of the visual representation of the canvas. The icon can
    /// be used to convey additional information or branding associated with the canvas content.</remarks>
    public Canvas Icon { get; set; }

    /// <summary>
    /// Gets or sets the settings group associated with the current configuration.
    /// </summary>
    /// <remarks>This property allows for categorizing settings into distinct groups, facilitating better
    /// organization and management of configuration options.</remarks>
    public SettingsGroup Group { get; set; }
}
