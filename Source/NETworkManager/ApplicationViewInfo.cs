using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace NETworkManager
{
    public class ApplicationViewInfo
    {
        public ApplicationViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }

        public ApplicationViewInfo()
        {

        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Icon = icon;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconModern packIconModern)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconModern);
            Icon = canvas;
        }
                
        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconMaterial packIconMaterial)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterial);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconMaterialLight packIconMaterialLight)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconMaterialLight);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconFontAwesome packIconFontAwesome)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconFontAwesome);
            Icon = canvas;
        }

        public ApplicationViewInfo(ApplicationViewManager.Name name, PackIconOcticons packIconOcticons)
        {
            Name = name;
            TranslatedName = ApplicationViewManager.TranslateName(name);
            Canvas canvas = new Canvas();
            canvas.Children.Add(packIconOcticons);
            Icon = canvas;
        }
    }
}