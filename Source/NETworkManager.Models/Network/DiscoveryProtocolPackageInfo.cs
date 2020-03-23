namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolPackageInfo
    {
        public string Device { get; set; }
        public string Port { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string VLAN { get; set; }
        public string IPAddress { get; set; }
        public string Protocol { get; set; }
        public string Time { get; set; }

        public DiscoveryProtocolPackageInfo()
        {

        }

        public DiscoveryProtocolPackageInfo(string device, string port, string description, string model, string vlan, string ipAdress, string protocol, string time)
        {
            Device = device;
            Port = port;
            Description = description;
            Model = model;
            VLAN = vlan;
            IPAddress = ipAdress;
            Protocol = protocol;
            Time = time;
        }

        public static DiscoveryProtocolPackageInfo Parse(DiscoveryProtocolPackageArgs e)
        {
            return new DiscoveryProtocolPackageInfo(e.Device, e.Protocol, e.Device, e.Model, e.VLAN, e.IPAddress, e.Protocol, e.Time);
        }

    }
}