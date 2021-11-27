namespace NETworkManager.Models.Network
{
    public class NetworkInterfaceConfig
    {
        public string Name { get; set; }
        public bool EnableStaticIPAddress { get; set; }
        public string IPAddress { get; set; }
        public string Subnetmask { get; set; }
        public string Gateway { get; set; }
        public bool EnableStaticDNS { get; set; }
        public string PrimaryDNSServer { get; set; }
        public string SecondaryDNSServer { get; set; }
        public IpVersionConfig IpVersion { get; set; } = IpVersionConfig.ipv4;
        public NetworkInterfaceConfig()
        {

        }        
    }
    public enum IpVersionConfig
    {
        ipv4,
        ipv6
    }
}
