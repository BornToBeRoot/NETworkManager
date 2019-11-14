using System;
using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectProfileApplicationArgs : EventArgs
    {
        public ApplicationViewManager.Name Application { get; set; }
        public ProfileInfo Profile { get; set; }
        
        public EventSystemRedirectProfileApplicationArgs(ApplicationViewManager.Name application, ProfileInfo profile)
        {
            Application = application;
            Profile = profile;
        }
    }
}
