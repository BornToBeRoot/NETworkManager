namespace NETworkManager.Models.Settings
{
    public class ConfigurationInfo
    {
        public bool IsAdmin { get; set; }
        public string StartupPath { get; set; }         
        public string ApplicationPath { get; set; }

        public ConfigurationInfo()
        {

        }
    }
}
