namespace NETworkManager.Settings.Templates
{
    public class NetworkInterfaceTemplate
    {
        public string Name { get; set; }
        public bool UseStaticIPv4Address { get; set; }
        public string IPv4Address { get; set; }
        public string Subnetmask { get; set; }
        public string IPv4Gateway { get; set; }
        public bool UseStaticIPv4DNSServer { get; set; }
        public string IPv4PrimaryDNSServer { get; set; }
        public string IPv4SecondaryDNSServer { get; set; }
        
        public NetworkInterfaceTemplate()
        {
                
        }

        public NetworkInterfaceTemplate(string name, bool useStaticIPAddress, string ipv4Address, string subnetmask, string ipv4Gateway, bool useDynamicIPv4DNSServer, bool useStaticIPv4DNSServer, string ipv4PrimaryDNSServer, string ipv4SecondaryIPv4Server)
        {
            Name = name;
            UseStaticIPv4Address = useStaticIPAddress;
            IPv4Address = ipv4Address;
            Subnetmask = subnetmask;
            IPv4Gateway = ipv4Gateway;            
            UseStaticIPv4DNSServer = useStaticIPv4DNSServer;
            IPv4PrimaryDNSServer = ipv4PrimaryDNSServer;
            IPv4SecondaryDNSServer = ipv4SecondaryIPv4Server;
        }
    }
}
