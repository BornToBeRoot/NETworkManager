namespace NETworkManager.Models.Profile
{
    public class ProfileFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsEncryptionEnabled { get; set; }
        public string Password { get; set; }
        public bool IsEncrypted { get; set; }

        public ProfileFileInfo(string name, string path, bool isEncryptionEnabled = false)
        {
            Name = name;
            Path = path;
            IsEncryptionEnabled = isEncryptionEnabled;
        }
    }
}
