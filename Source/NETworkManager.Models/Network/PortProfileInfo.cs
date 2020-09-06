namespace NETworkManager.Models.Network
{
    public class PortProfileInfo
    {
        public string Name { get; set; }
        public string Ports { get; set; }

        public PortProfileInfo()
        {

        }

        public PortProfileInfo(string name, string ports)
        {
            Name = name;
            Ports = ports;
        }
    }
}