using System.Security;

namespace NETworkManager.Models.Settings
{
    public class CredentialInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }

        public CredentialInfo()
        {

        }

        public CredentialInfo(int id, string name, string username, SecureString password)
        {
            ID = id;
            Name = name;
            Username = username;
            Password = password;
        }
    }
}
