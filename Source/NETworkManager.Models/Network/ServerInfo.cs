namespace NETworkManager.Models.Network
{
    public class ServerInfo
    {
        public string Server { get; set; }
        public int Port { get; set; }

        public ServerInfo()
        {
            
        }

        public ServerInfo(string server, int port)
        {
            Server = server;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Server}:{Port}";
        }
    }
}
