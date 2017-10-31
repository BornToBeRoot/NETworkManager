namespace NETworkManager.Models.Settings
{
    public class PortScannerProfileInfo
    {
        public string Name { get; set; }
        public string HostnameOrIPAddress { get; set; }
        public string Ports { get; set; }
        public string Group { get; set; }

        public PortScannerProfileInfo()
        {

        }

        public PortScannerProfileInfo(string name, string hostnameOrIPAddress, string ports, string group)
        {
            Name = name;
            HostnameOrIPAddress = hostnameOrIPAddress;
            Ports = ports;
            Group = group;
        }
    }
}
