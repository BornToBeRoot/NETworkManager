namespace NETworkManager.Models.Profile
{
    public class ProfileFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsEncrypted { get; set; }
        
        public ProfileFileInfo(string name, string path, bool isEncrypted = false)
        {
            Name = name;
            Path = path;
            IsEncrypted = isEncrypted;
        }
    }
}
