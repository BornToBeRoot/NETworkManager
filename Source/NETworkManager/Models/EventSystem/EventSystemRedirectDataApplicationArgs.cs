using NETworkManager.Models;
using System;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectDataApplicationArgs : EventArgs
    {
        public ApplicationName Application { get; set; }
        public string Args { get; set; }


        public EventSystemRedirectDataApplicationArgs(ApplicationName application, string args)
        {
            Application = application;
            Args = args;
        }
    }
}
