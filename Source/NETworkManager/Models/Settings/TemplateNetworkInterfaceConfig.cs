namespace NETworkManager.Models.Settings
{
    public class TemplateNetworkInterfaceConfig
    {
        public string Name { get; set; }
        public bool EnableStaticIPAddress { get; set; }
        public string IPAddress { get; set; }
        public string Subnetmask { get; set; }
        public string Gateway { get; set; }
        public bool EnableStaticDns { get; set; }
        public string PrimaryDnsServer { get; set; }
        public string SecondaryDnsServer { get; set; }

        public TemplateNetworkInterfaceConfig()
        {

        }        
    }
}
