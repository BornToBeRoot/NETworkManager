namespace NETworkManager.Models.Settings
{
    public class RemoteDesktopSessionInfo
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public int? CredentialID { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public RemoteDesktopSessionInfo()
        {

        }

        public RemoteDesktopSessionInfo(string name, string hostname, string group, string tags)
        {
            Name = name;
            Hostname = hostname;
            CredentialID = null;
            Group = group;
            Tags = tags;
        }

        public RemoteDesktopSessionInfo(string name, string hostname, int credentialID, string group, string tags)
        {
            Name = name;
            Hostname = hostname;
            CredentialID = credentialID; 
            Group = group;
            Tags = tags;
        }
    }
}
