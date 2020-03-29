using System;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectArgs : EventArgs
    {
        public ApplicationName Application { get; set; }
        public string Args { get; set; }


        public EventSystemRedirectArgs(ApplicationName application, string args)
        {
            Application = application;
            Args = args;
        }
    }
}
