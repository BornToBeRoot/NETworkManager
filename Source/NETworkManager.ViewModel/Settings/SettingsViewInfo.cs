using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.ViewModel.Settings
{
    public class SettingsViewInfo
    {
        public SettingsView.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }

        public SettingsViewInfo()
        {

        }

        public SettingsViewInfo(SettingsView.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = SettingsView.TranslateName(name);
            Icon = icon;
        }

        public SettingsViewInfo(SettingsView.Name name, PackIconModern packIconModern)
        {
            Name = name;
            TranslatedName = SettingsView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
        }

        public SettingsViewInfo(SettingsView.Name name, PackIconMaterial packIconMaterial)
        {
            Name = name;
            TranslatedName = SettingsView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
        }

        public SettingsViewInfo(SettingsView.Name name, PackIconEntypo packIconEntypo)
        {
            Name = name;
            TranslatedName = SettingsView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconEntypo);
            Icon = canvas;
        }

        public SettingsViewInfo(SettingsView.Name name, PackIconOcticons packIconOcticons)
        {
            Name = name;
            TranslatedName = SettingsView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconOcticons);
            Icon = canvas;
        }
    }
}