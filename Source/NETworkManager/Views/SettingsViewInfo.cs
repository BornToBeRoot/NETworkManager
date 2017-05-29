using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.Views

{
    public class SettingsViewInfo
    {
        public SettingsViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }

        public SettingsViewInfo()
        {

        }

        public SettingsViewInfo(SettingsViewManager.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            Icon = icon;
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconModern packIconModern)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconMaterial packIconMaterial)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconEntypo packIconEntypo)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconEntypo);
            Icon = canvas;
        }

        public SettingsViewInfo(SettingsViewManager.Name name, PackIconOcticons packIconOcticons)
        {
            Name = name;
            TranslatedName = SettingsViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconOcticons);
            Icon = canvas;
        }
    }
}