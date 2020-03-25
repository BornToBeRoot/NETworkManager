namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolPackageArgs : System.EventArgs
    {
        public string Device { get; private set; }
        public string Port { get; private set; }
        public string Description { get; private set; }
        public string Model { get; private set; }
        public string VLAN { get; private set; }
        public string IPAddress { get; private set; }
        public string Protocol { get; private set; }
        public string Time { get; private set; }

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
