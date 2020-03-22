using System;

namespace NETworkManager.Models.EventSystem
{
    public class EventSystemRedirectDataApplicationArgs : EventArgs
    {
        public Models.Application.Name Application { get; set; }
        public string Args { get; set; }


        public EventSystemRedirectDataApplicationArgs(Models.Application.Name application, string args)
        {
            Application = application;
            Args = args;
        }
    }
}
