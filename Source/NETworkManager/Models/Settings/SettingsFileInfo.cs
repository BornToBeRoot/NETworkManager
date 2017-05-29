namespace NETworkManager.Models.Settings
{
    public class SettingsFileInfo
    {
        public bool Export { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        
        public SettingsFileInfo()
        {

        }

        public SettingsFileInfo(bool export, string fileName, string filePath)
        {
            Export = export;
            FileName = fileName;
            FilePath = filePath;
        }
    }
}
