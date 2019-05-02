using System;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectDataApplicationArgs : EventArgs
    {
        public ApplicationViewManager.Name Application { get; set; }
        public string Args { get; set; }


        public EventSystemRedirectDataApplicationArgs(ApplicationViewManager.Name application, string args)
        {
            Application = application;
            Args = args;
        }
    }
}
