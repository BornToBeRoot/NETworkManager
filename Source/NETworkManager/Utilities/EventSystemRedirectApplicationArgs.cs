using System;

namespace NETworkManager.Utilities
{
    public class EventSystemRedirectApplicationArgs : EventArgs
    {
        public ApplicationViewManager.Name Application { get; set; }
        public string Data { get; set; }

        public EventSystemRedirectApplicationArgs(ApplicationViewManager.Name application, string data)
        {
            Application = application;
            Data = data;
        }
    }
}
