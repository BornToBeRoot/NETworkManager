using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.Views

{
    public class SettingsGeneralViewInfo
    {
        public SettingsGeneralViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }

        public SettingsGeneralViewInfo()
        {

        }

        public SettingsGeneralViewInfo(SettingsGeneralViewManager.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = SettingsGeneralViewManager.TranslateName(name);
            Icon = icon;
        }

        public SettingsGeneralViewInfo(SettingsGeneralViewManager.Name name, PackIconModern packIconModern)
        {
            Name = name;
            TranslatedName = SettingsGeneralViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
        }

        public SettingsGeneralViewInfo(SettingsGeneralViewManager.Name name, PackIconMaterial packIconMaterial)
        {
            Name = name;
            TranslatedName = SettingsGeneralViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
        }

        public SettingsGeneralViewInfo(SettingsGeneralViewManager.Name name, PackIconEntypo packIconEntypo)
        {
            Name = name;
            TranslatedName = SettingsGeneralViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconEntypo);
            Icon = canvas;
        }

        public SettingsGeneralViewInfo(SettingsGeneralViewManager.Name name, PackIconOcticons packIconOcticons)
        {
            Name = name;
            TranslatedName = SettingsGeneralViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconOcticons);
            Icon = canvas;
        }
    }
}