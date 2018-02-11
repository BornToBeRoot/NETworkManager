namespace NETworkManager.Models.Settings
{
    public class RemoteDesktopSessionInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int? CredentialID { get; set; }
        public string Group { get; set; }
        public string Tags { get; set; }

        public RemoteDesktopSessionInfo()
        {

        }

        public RemoteDesktopSessionInfo(string name, string host, string group, string tags)
        {
            Name = name;
            Host = host;
            CredentialID = null;
            Group = group;
            Tags = tags;
        }

        public RemoteDesktopSessionInfo(string name, string host, int credentialID, string group, string tags)
        {
            Name = name;
            Host = host;
            CredentialID = credentialID; 
            Group = group;
            Tags = tags;
        }
    }
}
