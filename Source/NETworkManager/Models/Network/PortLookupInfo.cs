namespace NETworkManager.Models.Network
{
    public class PortLookupInfo
    {
        public int Number { get; set; }
        public PortLookup.Protocol Protocol { get; set; }
        public string Service { get; set; }
        public string Description { get; set; }

        public PortLookupInfo()
        {

        }

        public PortLookupInfo(int number, PortLookup.Protocol protocol)
        {
            Number = number;
            Protocol = protocol;
        }

        public PortLookupInfo(int number, PortLookup.Protocol protocol, string service, string description)
        {
            Number = number;
            Protocol = protocol;
            Service = service;
            Description = description;
        }      
    }
}
