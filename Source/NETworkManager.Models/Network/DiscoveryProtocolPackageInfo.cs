namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class contains discovery protocol package informations.
    /// </summary>
    public class DiscoveryProtocolPackageInfo
    {
        /// <summary>
        /// Device name.
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        /// Device description.
        /// </summary>
        public string DeviceDescription { get; set; }

        /// <summary>
        /// Port name or number.
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Port description.
        /// </summary>
        public string PortDescription { get; set; }
        
        /// <summary>
        /// Device model.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Management VLAN.
        /// </summary>
        public string VLAN { get; set; }

        /// <summary>
        /// IP address(es) of the device.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Protocol type.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Time to live of the LLDP/CDP package.
        /// </summary>
        public string TimeToLive { get; set; }

        /// <summary>
        /// Device Management.
        /// </summary>
        public string Management { get; set; }

        /// <summary>
        /// Device Chassis ID.
        /// </summary>
        public string ChassisId { get; set; }

        /// <summary>
        /// Local connection used to capture the LLDP/CDP package.
        /// </summary>
        public string LocalConnection { get; set; }

        /// <summary>
        /// Local interface used to capture the LLDP/CDP package.
        /// </summary>
        public string LocalInterface { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryProtocolPackageInfo"/> class.
        /// </summary>
        public DiscoveryProtocolPackageInfo()
        {

        }
    }
}