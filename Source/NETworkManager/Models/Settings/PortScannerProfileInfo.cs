namespace NETworkManager.Models.Settings
{
    public class PortScannerProfileInfo
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public string Ports { get; set; }
        public string Group { get; set; }

        public PortScannerProfileInfo()
        {

        }

        public PortScannerProfileInfo(string name, string hostname, string ports, string group)
        {
            Name = name;
            Hostname = hostname;
            Ports = ports;
            Group = group;
        }
    }
}
