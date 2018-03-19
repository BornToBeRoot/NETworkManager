namespace NETworkManager.Models.Settings
{
    public class PuTTYSessionInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public PuTTYSessionInfo()
        {

        }

        public PuTTYSessionInfo(string name, string host, string group, string tags)
        {
            Name = name;
            Host = host;
            Group = group;
            Tags = tags;
        }
    }
}
