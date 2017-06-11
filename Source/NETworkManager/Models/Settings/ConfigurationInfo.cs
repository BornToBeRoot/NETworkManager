namespace NETworkManager.Models.Settings
{
    public class ConfigurationInfo
    {
        public bool IsAdmin { get; set; }
        public string ExecutionPath { get; set; }
        public string ApplicationFullName { get; set; }         
        public string ApplicationName { get; set; }

        public ConfigurationInfo()
        {

        }
    }
}
