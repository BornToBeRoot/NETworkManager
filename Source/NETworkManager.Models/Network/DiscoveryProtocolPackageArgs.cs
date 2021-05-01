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
        public string Device { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VLAN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TimeToLive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Management { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ChassisId { get; set; }

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
        /// <param name="timeToLive"></param>
        /// <param name="management"></param>
        /// <param name="chassisId"></param>
        public DiscoveryProtocolPackageArgs(string device, string port, string description, string model, string vlan, string ipAddress, string protocol, string timeToLive, string management, string chassisId)
        {
            Device = device;
            Port = port;
            Description = description;
            Model = model;
            VLAN = vlan;
            IPAddress = ipAddress;
            Protocol = protocol;
            TimeToLive = timeToLive;
            Management = management;
            ChassisId = chassisId;
        }
    }
}
