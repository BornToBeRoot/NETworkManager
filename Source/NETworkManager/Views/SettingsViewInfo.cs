using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.Views

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
            TranslatedName = SettingsViewManager.TranslateName(name, group);
            Icon = icon;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconModern packIconModern, SettingsViewManager.Group group)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name, group);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconMaterial packIconMaterial, SettingsViewManager.Group group)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name, group);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconEntypo packIconEntypo, SettingsViewManager.Group group)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name, group);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconEntypo);
            Icon = canvas;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconOcticons packIconOcticons, SettingsViewManager.Group group)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name, group);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconOcticons);
            Icon = canvas;
            Group = group;
            TranslatedGroup = SettingsViewManager.TranslateGroup(group);
        }
    }
}