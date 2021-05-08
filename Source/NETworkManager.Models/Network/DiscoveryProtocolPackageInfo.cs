namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolPackageInfo
    {
        public string Device { get; private set; }
        public string DeviceDescription { get; private set; }
        public string Port { get; private set; }
        public string PortDescription { get; private set; }
        public string Model { get; private set; }
        public string VLAN { get; private set; }
        public string IPAddress { get; private set; }
        public string Protocol { get; private set; }
        public string TimeToLive { get; private set; }
        public string Management { get; private set; }
        public string ChassisId { get; private set; }

        public DiscoveryProtocolPackageInfo()
        {

        }

        public DiscoveryProtocolPackageInfo(string device, string deviceDescription, string port, string portDescription, string model, string vlan, string ipAdress, string protocol, string timeToLive, string management, string chassisId)
        {
            Device = device;
            DeviceDescription = deviceDescription;
            Port = port;
            PortDescription = portDescription;
            Model = model;
            VLAN = vlan;
            IPAddress = ipAdress;
            Protocol = protocol;
            TimeToLive = timeToLive;
            Management = management;
            ChassisId = chassisId;
        }

        public static DiscoveryProtocolPackageInfo Parse(DiscoveryProtocolPackageArgs e)
        {
            return new DiscoveryProtocolPackageInfo(e.Device, e.DeviceDescription, e.Port, e.PortDescription, e.Model, e.VLAN, e.IPAddress, e.Protocol, e.TimeToLive, e.Management, e.ChassisId);
        }
    }
}