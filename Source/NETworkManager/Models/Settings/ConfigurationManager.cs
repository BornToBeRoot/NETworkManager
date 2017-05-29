using System.Security.Principal;
namespace NETworkManager.Models.Settings
{
    public static class ConfigurationManager
    {
        public static ConfigurationInfo Current { get; set; }

        public static void Detect()
        {
            Current = new ConfigurationInfo()
            {
                IsAdmin = (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator)
            };
        }
    }
}
