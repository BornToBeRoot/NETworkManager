using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Settings;

public class SettingsViewInfo
{
    public SettingsViewInfo(SettingsName name, Canvas icon, SettingsGroup group)
    {
        Name = name;
        Icon = icon;
        Group = group;
    }

    public SettingsViewInfo(SettingsName name, UIElement uiElement, SettingsGroup group)
    {
        Name = name;
        var canvas = new Canvas();
        canvas.Children.Add(uiElement);
        Icon = canvas;
        Group = group;
    }

    public SettingsName Name { get; set; }
    public Canvas Icon { get; set; }
    public SettingsGroup Group { get; set; }
}