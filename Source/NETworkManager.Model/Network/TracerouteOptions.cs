namespace NETworkManager.Model.Network
{
    public class TracerouteOptions
    {
        public int Timeout;
        public int Buffer;
        public int MaximumHops;
        public bool DontFragement;

        public TracerouteOptions()
        {

        }

        public TracerouteOptions(int timeout, int buffer, int maximumHops, bool dontFragement)
        {
            Timeout = timeout;
            Buffer = buffer;
            MaximumHops = maximumHops;
            DontFragement = dontFragement;
        }
    }
}
