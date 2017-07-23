using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public class ApplicationViewInfo
    {
        public ApplicationViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }
        public bool IsDev { get; set; }

        public ApplicationViewInfo()
        {

        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Icon = icon;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, Canvas icon, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Icon = icon;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconModern packIconModern)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconModern packIconModern, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconMaterial packIconMaterial)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconMaterial packIconMaterial, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconFontAwesome packIconFontAwesome)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconFontAwesome);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconFontAwesome packIconFontAwesome, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconFontAwesome);
            Icon = canvas;
            IsDev = isDev;
        }
    }
}