using System.Security.Principal;
using System.IO;

namespace NETworkManager.Settings
{
    public static class ConfigurationManager
    {
        private const string IsPortableFileName = "IsPortable";
        private const string IsPortableExtension = "settings";

        public static ConfigurationInfo Current { get; set; }

        static ConfigurationManager()
        {
            Current = new ConfigurationInfo
            {
                IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator),
                ExecutionPath = Path.GetDirectoryName(AssemblyManager.Current.Location),
                ApplicationFullName = AssemblyManager.Current.Location,
                ApplicationName = AssemblyManager.Current.Name,
                OSVersion = System.Environment.OSVersion.Version,
                IsPortable = File.Exists(Path.Combine(Path.GetDirectoryName(AssemblyManager.Current.Location), $"{IsPortableFileName}.{IsPortableExtension}"))
            };
        }
    }
}
