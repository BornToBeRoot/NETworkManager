using System.Diagnostics.CodeAnalysis;

namespace NETworkManager.Models.Profile
{
    public class ProfileLocationInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Encrypted { get; set; }
        
        public ProfileLocationInfo(string name, string path, bool encrypted = false)
        {
            Name = name;
            Path = path;
            Encrypted = encrypted;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
