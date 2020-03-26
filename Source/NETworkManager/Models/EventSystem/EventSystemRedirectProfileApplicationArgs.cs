using System;
using NETworkManager.Models.Application;
using NETworkManager.Models.Profile;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectProfileApplicationArgs : EventArgs
    {
        public Name Application { get; set; }
        public ProfileInfo Profile { get; set; }
        
        public EventSystemRedirectProfileApplicationArgs(Name application, ProfileInfo profile)
        {
            Application = application;
            Profile = profile;
        }
    }
}
