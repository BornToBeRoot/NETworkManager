namespace NETworkManager.Models.Settings
{
    public class CredentialInfoSerializable
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public CredentialInfoSerializable()
        {

        }

        public CredentialInfoSerializable(int index, string name, string username, string password)
        {
            Index = index;
            Name = name;
            Username = username;
            Password = password;
        }
    }
}
