namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Contains informations about a received discovery protocol package.
    /// </summary>
    public class DiscoveryProtocolPackageArgs : System.EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string Device { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Port { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Model { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string VLAN { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string IPAddress { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Protocol { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Time { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DiscoveryProtocolPackageArgs()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="port"></param>
        /// <param name="description"></param>
        /// <param name="model"></param>
        /// <param name="vlan"></param>
        /// <param name="ipAddress"></param>
        /// <param name="protocol"></param>
        /// <param name="time"></param>
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
