using System.Reflection;
using System.Security.Principal;
using System.IO;

namespace NETworkManager.Models.Settings
{
    public static class ConfigurationManager
    {
        public static ConfigurationInfo Current { get; set; }

        public static void Detect()
        {
            string applicationLocation = Assembly.GetExecutingAssembly().Location;

            Current = new ConfigurationInfo()
            {
                IsAdmin = (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator),
                ExecutionPath = Path.GetDirectoryName(applicationLocation),
                ApplicationFullName = applicationLocation,
                ApplicationName = Path.GetFileNameWithoutExtension(Path.GetFileName(applicationLocation))
            };
        }
    }
}
