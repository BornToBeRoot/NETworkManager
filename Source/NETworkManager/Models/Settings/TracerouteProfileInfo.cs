namespace NETworkManager.Models.Settings
{
    public class TracerouteProfileInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public TracerouteProfileInfo()
        {

        }

        public TracerouteProfileInfo(string name, string host, string group, string tags)
        {
            Name = name;
            Host = host;
            Group = group;
            Tags = tags;
        }
    }
}
