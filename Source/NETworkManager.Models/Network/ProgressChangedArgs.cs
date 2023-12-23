using System;

namespace NETworkManager.Models.Network;

public class ProgressChangedArgs : EventArgs
{
    public ProgressChangedArgs()
    {
    }

    public ProgressChangedArgs(int value)
    {
        Value = value;
    }

    public int Value { get; set; }
}