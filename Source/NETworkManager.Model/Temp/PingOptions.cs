namespace NETworkManager.Model.Network
{
    public class PingOptions
    {
        public int Attempts { get; set; }
        public int WaitTime { get; set; }
        public int Timeout { get; set; }
        public byte[] Buffer { get; set; }
        public int TTL { get; set; }
        public bool DontFragement { get; set; }

        public PingOptions()
        {

        }

        public PingOptions(int attempts, int waitTime, int timeout, byte[] buffer, int ttl, bool dontFragement)
        {
            Attempts = attempts;
            WaitTime = waitTime;
            Timeout = timeout;
            Buffer = buffer;
            TTL = ttl;
            DontFragement = dontFragement;
        }
    }
}