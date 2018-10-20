using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    public class DNSServerInfo
    {
        public string Name { get; set; }
        public List<string> Server { get; set; }
        public int Port { get; set; } = 53;
        public bool UseWindowsDNSServer { get; set; }

        public DNSServerInfo()
        {

        }

        public DNSServerInfo(string name, List<string> server, int port = 53)
        {
            Name = name;
            Server = server;
            Port = port;
        }
    }
}