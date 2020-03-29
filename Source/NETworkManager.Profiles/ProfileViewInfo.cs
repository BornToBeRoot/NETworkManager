using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Profiles
{
    public class ProfileViewInfo
    {
        public ProfileViewName Name { get; set; }

        public Canvas Icon { get; set; }

        public ProfileViewInfo()
        {
        }

        public ProfileViewInfo(ProfileViewName name, Canvas icon)
        {
            Name = name;
            Icon = icon;
        }

        public ProfileViewInfo(ProfileViewName name, UIElement uiElement)
        {
            Name = name;
            var canvas = new Canvas();
            canvas.Children.Add(uiElement);
            Icon = canvas;
        }
    }
}