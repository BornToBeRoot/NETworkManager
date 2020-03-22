using System;
using NETworkManager.Models.Profile;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectProfileApplicationArgs : EventArgs
    {
        public Models.Application.Name Application { get; set; }
        public ProfileInfo Profile { get; set; }
        
        public EventSystemRedirectProfileApplicationArgs(Models.Application.Name application, ProfileInfo profile)
        {
            Application = application;
            Profile = profile;
        }
    }
}
