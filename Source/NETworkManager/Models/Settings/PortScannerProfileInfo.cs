namespace NETworkManager.Models.Settings
{
    public class PortScannerProfileInfo
    {
        public string Name { get; set; }
        public string Ports { get; set; }
        
        public PortScannerProfileInfo()
        {

        }

        public PortScannerProfileInfo(string name, string ports)
        {
            Name = name;
            Ports = ports;
        }
    }
}
