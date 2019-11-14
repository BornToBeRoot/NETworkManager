using System;
using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystem
    {
        // This will notify the mail window, to change the view to another application and redirect a profile
        public static event EventHandler RedirectProfileToApplicationEvent;

        public static void RedirectProfileToApplication(ApplicationViewManager.Name application, ProfileInfo profile)
        {
            RedirectProfileToApplicationEvent?.Invoke(typeof(string), new EventSystemRedirectProfileApplicationArgs(application, profile));
        }

        // This will notify the mail window, to change the view to another application and redirect some data (hostname, ip)
        public static event EventHandler RedirectDataToApplicationEvent;

        public static void RedirectDataToApplication(ApplicationViewManager.Name application, string data)
        {
            RedirectDataToApplicationEvent?.Invoke(typeof(string), new EventSystemRedirectDataApplicationArgs(application, data));
        }

        // This will notify the main window, to change the view to the settings...
        public static event EventHandler RedirectToSettingsEvent;

        public static void RedirectToSettings()
        {
            RedirectToSettingsEvent?.Invoke(typeof(string), EventArgs.Empty);
        }
    }
}