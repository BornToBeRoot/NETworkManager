using System.Security;

namespace NETworkManager.Models.Settings
{
    public class CredentialInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }

        public CredentialInfo()
        {

        }

        public CredentialInfo(int index, string name, string username, SecureString password)
        {
            Index = index;
            Name = name;
            Username = username;
            Password = password;
        }
    }
}
