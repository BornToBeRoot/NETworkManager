using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NETworkManager.Models.Settings
{
    public class AutostartManager
    {
        // Key where the autorun entries for the current user are stored
        private const string RunKeyCurrentUser = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static bool IsEnabled
        {
            get
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser);

                if (registryKey.GetValue(ConfigurationManager.Current.ApplicationName) != null)
                    return true;

                return false;
            }
        }

        public static Task EnableAsync()
        {
            return Task.Run(() => Enable());
        }

        public static void Enable()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

            string command = string.Format("{0} {1}", ConfigurationManager.Current.ApplicationFullName, CommandLineManager.ParameterAutostart);

            registryKey.SetValue(ConfigurationManager.Current.ApplicationName, command);
            registryKey.Close();
        }

        public static Task DisableAsync()
        {
            return Task.Run(() => Disable());
        }

        public static void Disable()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

            registryKey.DeleteValue(ConfigurationManager.Current.ApplicationName);
            registryKey.Close();
        }
    }
}
