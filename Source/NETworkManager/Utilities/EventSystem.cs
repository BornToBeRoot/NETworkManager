using System;

namespace NETworkManager.Utilities
{
    public class EventSystem
    {
        public static event EventHandler RedirectToApplicationEvent;

        public static void RedirectToApplication(ApplicationViewManager.Name application, string data)
        {
            RedirectToApplicationEvent?.Invoke(typeof(string), new EventSystemRedirectArgs(application, data));
        }
    }
}