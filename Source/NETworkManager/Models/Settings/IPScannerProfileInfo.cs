namespace NETworkManager.Models.Settings
{
    public class IPScannerProfileInfo
    {
        public string Name { get; set; }
        public string IPRange { get; set; }
        
        public IPScannerProfileInfo()
        {

        }

        public IPScannerProfileInfo(string name, string ipRange)
        {
            Name = name;
            IPRange = ipRange;
        }
    }
}
