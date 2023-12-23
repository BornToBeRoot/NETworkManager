using System;

namespace NETworkManager.Models.Network;

public class MaximumHopsReachedArgs : EventArgs
{
    public MaximumHopsReachedArgs()
    {
    }

    public MaximumHopsReachedArgs(int hops)
    {
        Hops = hops;
    }

    public int Hops { get; set; }
}