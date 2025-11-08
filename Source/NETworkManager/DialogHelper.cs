using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;
using NETworkManager.Views;
using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager
{
    public static class DialogHelper
    {
        public static Task ShowOKMessageAsync(Window parentWindow, string title, string message, ChildWindowIcon icon = ChildWindowIcon.Info, string buttonOK = null)
        {
            if (string.IsNullOrEmpty(buttonOK))
                buttonOK = Strings.OK;

            var childWindow = new OKMessageChildWindow();

            var childWindowViewModel = new OKMessageViewModel(_ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            }, message, buttonOK, icon);

            childWindow.Title = title;

            childWindow.DataContext = childWindowViewModel;

            ConfigurationManager.Current.IsChildWindowOpen = true;

            return parentWindow.ShowChildWindowAsync(childWindow);
        }
    }
}
