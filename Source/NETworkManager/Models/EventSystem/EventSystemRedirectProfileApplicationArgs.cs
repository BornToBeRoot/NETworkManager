using System;
using NETworkManager.Models.Application;
using NETworkManager.Models.Profile;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectProfileApplicationArgs : EventArgs
    {
        public ApplicationName Application { get; set; }
        public ProfileInfo Profile { get; set; }
        
        public EventSystemRedirectProfileApplicationArgs(ApplicationName application, ProfileInfo profile)
        {
            Application = application;
            Profile = profile;
        }
    }
}
