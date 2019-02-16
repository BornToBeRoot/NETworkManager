using System.Windows;
using System.Windows.Controls;

namespace NETworkManager
{
    public class ProfileViewInfo
    {
        public ProfileViewManager.Name Name { get; set; }
        public string TranslatedName { get; set; }
        public Canvas Icon { get; set; }

        public ProfileViewInfo()
        {
        }

        public ProfileViewInfo(ProfileViewManager.Name name, Canvas icon)
        {
            Name = name;
            TranslatedName = ProfileViewManager.TranslateName(name);
            Icon = icon;
        }

        public ProfileViewInfo(ProfileViewManager.Name name, UIElement uiElement)
        {
            Name = name;
            TranslatedName = ProfileViewManager.TranslateName(name);
            var canvas = new Canvas();
            canvas.Children.Add(uiElement);
            Icon = canvas;
        }
    }
}