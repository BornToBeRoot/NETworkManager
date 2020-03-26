namespace NETworkManager.Models.Network
{
    public class WhoisServerInfo
    {
        public string Server { get; set; }
        public string Tld { get; set; }

        public WhoisServerInfo(string server, string tld)
        {
            Server = server;
            Tld = tld;
        }
    }
}
