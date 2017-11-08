namespace NETworkManager.Models.Settings
{
    public class RemoteDesktopSessionInfo
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public RemoteDesktopSessionInfo()
        {

        }

        public RemoteDesktopSessionInfo(string name, string hostname, string group, string tags)
        {
            Name = name;
            Hostname = hostname;
            Group = group;
            Tags = tags;
        }
    }
}
