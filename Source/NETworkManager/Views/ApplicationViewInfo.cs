using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public class ApplicationViewInfo
    {
        public ApplicationView.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }
        public bool IsDev { get; set; }

        public ApplicationViewInfo()
        {

        }

        public ApplicationViewInfo(ApplicationView.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = ApplicationView.TranslateName(name);
            Icon = icon;
        }

        public ApplicationViewInfo(ApplicationView.Name name, Canvas icon, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationView.TranslateName(name);
            Icon = icon;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationView.Name name, PackIconModern packIconModern)
        {
            Name = name;
            TranslatedName = ApplicationView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationView.Name name, PackIconModern packIconModern, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
            IsDev = isDev;
        }

        public ApplicationViewInfo(ApplicationView.Name name, PackIconMaterial packIconMaterial)
        {
            Name = name;
            TranslatedName = ApplicationView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationView.Name name, PackIconMaterial packIconMaterial, bool isDev)
        {
            Name = name;
            TranslatedName = ApplicationView.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
            IsDev = isDev;
        }
    }
}