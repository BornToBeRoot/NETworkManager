namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolPackageInfo
    {
        public string Device { get; private set; }
        public string Port { get; private set; }
        public string Description { get; private set; }
        public string Model { get; private set; }
        public string VLAN { get; private set; }
        public string IPAddress { get; private set; }
        public string Protocol { get; private set; }
        public string Time { get; private set; }

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