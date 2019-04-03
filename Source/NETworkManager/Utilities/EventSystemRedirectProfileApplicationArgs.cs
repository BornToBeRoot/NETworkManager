using System;
using NETworkManager.Models.Settings;

namespace NETworkManager.Utilities
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
