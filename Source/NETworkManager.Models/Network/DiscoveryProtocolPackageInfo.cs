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
        public string TimeToLive { get; private set; }
        public string Management { get; private set; }
        public string ChassisId { get; private set; }

        public DiscoveryProtocolPackageInfo()
        {

        }

        public DiscoveryProtocolPackageInfo(string device, string port, string description, string model, string vlan, string ipAdress, string protocol, string timeToLive, string management, string chassisId)
        {
            Device = device;
            Port = port;
            Description = description;
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
            return new DiscoveryProtocolPackageInfo(e.Device, e.Port, e.Description, e.Model, e.VLAN, e.IPAddress, e.Protocol, e.TimeToLive, e.Management, e.ChassisId);
        }
    }
}