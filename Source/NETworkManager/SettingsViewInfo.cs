using System.Windows;
using System.Windows.Controls;

namespace NETworkManager
{
    public class SettingsViewInfo
    {
        public SettingsViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }
        public SettingsViewManager.Group Group { get; set; }
        public string TranslatedGroup { get; set; }

        public SettingsViewInfo()
        {
        }

        public SettingsViewInfo(SettingsViewManager.Name name, Canvas icon, SettingsViewManager.Group group)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            Icon = icon;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }

        public SettingsViewInfo(SettingsViewManager.Name name, UIElement uiElement, SettingsViewManager.Group group)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            var canvas = new Canvas();
            canvas.Children.Add(uiElement);
            Icon = canvas;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }
    }
}