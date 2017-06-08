namespace NETworkManager.Models.Network
{
    public class MaximumHopsReachedArgs : System.EventArgs
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
