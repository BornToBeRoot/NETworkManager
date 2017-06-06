namespace NETworkManager.Models.Network
{
    public class PortScannerOptions
    {
        public int Threads { get; set; }
        public int Timeout { get; set; }

        public PortScannerOptions()
        {

        }

        public PortScannerOptions(int threads, int timeout)
        {
            Threads = threads;
            Timeout = timeout;
        }
    }
}
