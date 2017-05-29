namespace NETworkManager.Models.Network
{
    public class TracerouteMaximumHopsReachedArgs : System.EventArgs
    {
        public int Hops { get; set; }
        
        public TracerouteMaximumHopsReachedArgs()
        {

        }

        public TracerouteMaximumHopsReachedArgs(int hops)
        {
            Hops = hops;
        }
    }
}
