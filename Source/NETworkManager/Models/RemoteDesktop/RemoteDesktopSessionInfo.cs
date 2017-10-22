namespace NETworkManager.Models.RemoteDesktop
{
    public class RemoteDesktopSessionInfo
    {
        public string Hostname { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public RemoteDesktopSessionInfo()
        {

        }

        public RemoteDesktopSessionInfo(string hostname, string domain, string username, string password)
        {
            Hostname = hostname;
            Domain = domain;
            Username = username;
            Password = password;
        }
    }
}
