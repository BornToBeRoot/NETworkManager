namespace NETworkManager.Models.Network
{
    public class IPScannerOptions
    {
        public int Threads { get; set; }
        public int Timeout { get; set; }
        public byte[] Buffer { get; set; }
        public int Attempts { get; set; }
        public bool ResolveHostname { get; set; }
        public bool ResolveMACAddress { get; set; }

        public IPScannerOptions()
        {

        }
    }
}
