namespace NETworkManager.Models.Network;

public class PortProfileInfo
{
    public PortProfileInfo()
    {
    }

    public PortProfileInfo(string name, string ports)
    {
        Name = name;
        Ports = ports;
    }

    public string Name { get; set; }
    public string Ports { get; set; }
}