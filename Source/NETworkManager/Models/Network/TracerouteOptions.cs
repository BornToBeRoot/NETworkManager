namespace NETworkManager.Models.Network
{
    public class TracerouteOptions
    {
        public int Timeout;
        public int Buffer;
        public int MaximumHops;
        public bool DontFragement;
        public bool ResolveHostname;

        public TracerouteOptions()
        {

        }

        public TracerouteOptions(int timeout, int buffer, int maximumHops, bool dontFragement, bool resolveHostname)
        {
            Timeout = timeout;
            Buffer = buffer;
            MaximumHops = maximumHops;
            DontFragement = dontFragement;
            ResolveHostname = resolveHostname;
        }
    }
}
