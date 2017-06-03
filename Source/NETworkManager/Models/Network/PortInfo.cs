namespace NETworkManager.Models.Network
{
    public class PortInfo
    {
        public int Number { get; set; }
        public PortLookup.Protocol Protocol { get; set; }
        public string Service { get; set; }
        public string Description { get; set; }

        public PortInfo()
        {

        }

        public PortInfo(int number, PortLookup.Protocol protocol)
        {
            Number = number;
            Protocol = protocol;
        }

        public PortInfo(int number, PortLookup.Protocol protocol, string service, string description)
        {
            Number = number;
            Protocol = protocol;
            Service = service;
            Description = description;
        }      
    }
}
