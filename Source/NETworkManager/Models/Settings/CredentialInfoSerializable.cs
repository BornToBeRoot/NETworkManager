using System;

namespace NETworkManager.Models.Settings
{
    public class CredentialInfoSerializable
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public CredentialInfoSerializable()
        {

        }

        public CredentialInfoSerializable(Guid id, string name, string username, string password)
        {
            ID = id;
            Name = name;
            Username = username;
            Password = password;
        }
    }
}
