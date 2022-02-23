using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Profiles
{
    public class GroupViewInfo
    {
        public GroupViewName Name { get; set; }

        public Canvas Icon { get; set; }

        public GroupViewInfo()
        {
        }

        public GroupViewInfo(GroupViewName name, Canvas icon)
        {
            Name = name;
            Icon = icon;
        }

        public GroupViewInfo(GroupViewName name, UIElement uiElement)
        {
            Name = name;
            var canvas = new Canvas();
            canvas.Children.Add(uiElement);
            Icon = canvas;
        }
    }
}
