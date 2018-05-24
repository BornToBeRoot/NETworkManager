using System;

namespace NETworkManager.Utilities
{
    public class EventSystem
    {
        // This will notify the mail window, to change the view to another application and redirect some data (hostname, ip)
        public static event EventHandler RedirectToApplicationEvent;

        public static void RedirectToApplication(ApplicationViewManager.Name application, string data)
        {
            RedirectToApplicationEvent?.Invoke(typeof(string), new EventSystemRedirectApplicationArgs(application, data));
        }

        // This will notify the main window, to change the view to the settings...
        public static event EventHandler RedirectToSettingsEvent;

        public static void RedirectToSettings()
        {
            RedirectToSettingsEvent?.Invoke(typeof(string), EventArgs.Empty);
        }
    }
}