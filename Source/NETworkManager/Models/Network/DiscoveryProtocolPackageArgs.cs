namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolPackageArgs : System.EventArgs
    {
        public string Device { get; set; }
        public string Port { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string VLAN { get; set; }
        public string IPAddress { get; set; }
        public string Protocol { get; set; }
        public string Time { get; set; }

        public DiscoveryProtocolPackageArgs()
        {

        }

        public DiscoveryProtocolPackageArgs(string device, string port, string description, string model, string vlan, string ipAddress, string protocol, string time)
        {
            Device = device;
            Port = port;
            Description = description;
            Model = model;
            VLAN = vlan;
            IPAddress = ipAddress;
            Protocol = protocol;
            Time = time;
        }
    }
}
