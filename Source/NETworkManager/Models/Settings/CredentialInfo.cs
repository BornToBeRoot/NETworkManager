using System;
using System.Security;

namespace NETworkManager.Models.Settings
{
    public class CredentialInfo
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }

        public CredentialInfo()
        {
            ID = Guid.NewGuid();
        }

        public CredentialInfo(Guid id)
        {
            ID = id;
        }

        public CredentialInfo(Guid id, string name)
        {
            ID = id;
            Name = name;
        }

        public CredentialInfo(Guid id, string name, string username, SecureString password)
        {
            ID = id;
            Name = name;
            Username = username;
            Password = password;
        }
    }
}
