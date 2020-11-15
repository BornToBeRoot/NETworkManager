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
                ExecutionPath = AssemblyManager.Current.Location,
                ApplicationFullName = Path.Combine(AssemblyManager.Current.Location, AssemblyManager.Current.Name + ".exe"),
                ApplicationName = AssemblyManager.Current.Name,
                OSVersion = System.Environment.OSVersion.Version,
                IsPortable = File.Exists(Path.Combine(AssemblyManager.Current.Location, $"{IsPortableFileName}.{IsPortableExtension}")),
                IsPreview = File.Exists(Path.Combine(AssemblyManager.Current.Location, "IsPreview.settings"))
            };
        }
    }
}
