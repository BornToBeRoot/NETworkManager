namespace NETworkManager.Models.Network
{
    public class PortScannerOptions
    {
        public int Threads { get; set; }
        public bool ShowClosed { get; set; }
        public int Timeout { get; set; }

        public PortScannerOptions()
        {

        }

        public PortScannerOptions(int threads, bool showClosed, int timeout)
        {
            Threads = threads;
            ShowClosed = showClosed;
            Timeout = timeout;
        }
    }
}
