using System;

namespace NETworkManager.Models.EventSystem;

public class EventSystemRedirectArgs : EventArgs
{
    public EventSystemRedirectArgs(ApplicationName application, string args)
    {
        Application = application;
        Args = args;
    }

    public ApplicationName Application { get; set; }
    public string Args { get; set; }
}