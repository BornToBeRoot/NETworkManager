using System;

namespace NETworkManager.Model.Network
{
    public class MaximumHopsReachedArgs : EventArgs
    {
        public int Hops { get; set; }
        
        public MaximumHopsReachedArgs()
        {

        }

        public MaximumHopsReachedArgs(int hops)
        {
            Hops = hops;
        }
    }
}
