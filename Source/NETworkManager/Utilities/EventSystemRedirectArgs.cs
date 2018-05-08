using System;

namespace NETworkManager.Utilities
{
    public class EventSystemRedirectArgs : EventArgs
    {
        public ApplicationViewManager.Name Application { get; set; }
        public string Data { get; set; }

        public EventSystemRedirectArgs(ApplicationViewManager.Name application, string data)
        {
            Application = application;
            Data = data;
        }
    }
}
