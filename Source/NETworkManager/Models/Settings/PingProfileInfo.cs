namespace NETworkManager.Models.Settings
{
    public class PingProfileInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public PingProfileInfo()
        {

        }

        public PingProfileInfo(string name, string host, string group, string tags)
        {
            Name = name;
            Host = host;
            Group = group;
            Tags = tags;
        }
    }
}
