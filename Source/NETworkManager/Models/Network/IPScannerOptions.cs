namespace NETworkManager.Models.Network
{
    public class IPScannerOptions
    {
        public int Threads { get; set; }
        public int ICMPTimeout { get; set; }
        public byte[] ICMPBuffer { get; set; }
        public int ICMPAttempts { get; set; }
        public bool ResolveHostname { get; set; }
        public bool UseCustomDNSServer { get; set; }
        public string CustomDNSServer { get; set; }
        public int DNSPort { get; set; }
        public int DNSAttempts { get; set; }
        public int DNSTimeout { get; set; }
        public bool ResolveMACAddress { get; set; }
        public bool ShowScanResultForAllIPAddresses { get; set; }

        public IPScannerOptions()
        {

        }
    }
}
