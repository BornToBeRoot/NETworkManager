using System.Security.Principal;
namespace NETworkManager.Settings
{
    public static class Configuration
    {
        public static ConfigurationInfo Current { get; set; }

        public static void Detect()
        {
            Current = new ConfigurationInfo();

            Current.IsAdmin = (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
