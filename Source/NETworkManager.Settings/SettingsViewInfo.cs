using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Settings
{
    public class SettingsViewInfo
    {
        public SettingsViewName Name { get; set; }
        public Canvas Icon { get; set; }
        public SettingsViewGroup Group { get; set; }

        public SettingsViewInfo()
        {
        }

        public SettingsViewInfo(SettingsViewName name, Canvas icon, SettingsViewGroup group)
        {
            Name = name;
            Icon = icon;
            Group = group;
        }

        public SettingsViewInfo(SettingsViewName name, UIElement uiElement, SettingsViewGroup group)
        {
            Name = name;
            var canvas = new Canvas();
            canvas.Children.Add(uiElement);
            Icon = canvas;
            Group = group;
        }
    }
}