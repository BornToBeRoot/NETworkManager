namespace NETworkManager.Models.Network
{
    public class DNSServerClassicInfo
    {
        
        public string Server { get; set; }

        public int Port { get; set; }

        public DNSServerClassicInfo()
        {
           
        }

        public DNSServerClassicInfo(string server, int port)
        {
            Server = server;
            Port = port;
        }
    }
}
