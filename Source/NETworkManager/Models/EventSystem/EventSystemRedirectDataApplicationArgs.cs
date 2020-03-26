using NETworkManager.Models.Application;
using System;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectDataApplicationArgs : EventArgs
    {
        public Name Application { get; set; }
        public string Args { get; set; }


        public EventSystemRedirectDataApplicationArgs(Name application, string args)
        {
            Application = application;
            Args = args;
        }
    }
}
