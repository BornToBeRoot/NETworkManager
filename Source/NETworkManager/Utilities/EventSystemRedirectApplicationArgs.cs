using System;

namespace NETworkManager.Utilities
{
    public class EventSystemRedirectApplicationArgs : EventArgs
    {
        public ApplicationViewManager.Name Application { get; set; }
        public string Args { get; set; }

        public EventSystemRedirectApplicationArgs(ApplicationViewManager.Name application, string args)
        {
            Application = application;
            Args = args;
        }
    }
}
