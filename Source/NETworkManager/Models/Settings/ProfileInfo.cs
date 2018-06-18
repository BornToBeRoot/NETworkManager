namespace NETworkManager.Models.Settings
{
    public class ProfileInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public bool NetworkInterface_Enabled { get; set; }

        public bool IPScanner_Enabled { get; set; }
        public bool IPScanner_InheritHost { get; set; }
        public string IPScanner_IPRange { get; set; }

        public bool PortScanner_Enabled { get; set; }
        public bool PortScanner_InheritHost { get; set; }
        public string PortScanner_Host { get; set; }
        public string PortScanner_Ports { get; set; }

        public ProfileInfo()
        {

        }
    }
}
