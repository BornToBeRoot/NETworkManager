using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public class ApplicationViewInfo
    {
        public ApplicationViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }
        public bool HasSettings { get; set; }
        public bool IsDev { get; set; }

        public ApplicationViewInfo()
        {

        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, Canvas icon, bool hasSettings)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Icon = icon;
            HasSettings = hasSettings;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, Canvas icon, bool hasSettings, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Icon = icon;
            HasSettings = hasSettings;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconModern packIconModern, bool hasSettings)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
            HasSettings = hasSettings;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconModern packIconModern, bool hasSettings, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
            HasSettings = hasSettings;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconMaterial packIconMaterial, bool hasSettings)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
            HasSettings = hasSettings;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconMaterial packIconMaterial, bool hasSettings, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
            HasSettings = hasSettings;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconFontAwesome packIconFontAwesome, bool hasSettings)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconFontAwesome);
            Icon = canvas;
            HasSettings = hasSettings;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconFontAwesome packIconFontAwesome, bool hasSettings, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconFontAwesome);
            Icon = canvas;
            HasSettings = hasSettings;
            IsDev = isDev;
        }
    }
}