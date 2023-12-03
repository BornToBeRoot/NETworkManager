using System.Net;

namespace NETworkManager.Models.Network;

public class PingMonitorOptions(string host, IPAddress ipAddress)
{
    public string Host { get; } = host;
    public IPAddress IPAddress { get; } = ipAddress;
}
