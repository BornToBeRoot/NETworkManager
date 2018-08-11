using Microsoft.Win32;
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
                var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser);

                return registryKey?.GetValue(ConfigurationManager.Current.ApplicationName) != null;
            }
        }

        public static Task EnableAsync()
        {
            return Task.Run(() => Enable());
        }

        public static void Enable()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

            var command = $"{ConfigurationManager.Current.ApplicationFullName} {CommandLineManager.ParameterAutostart}";

            if (registryKey == null)
                return; // LOG

            registryKey.SetValue(ConfigurationManager.Current.ApplicationName, command);
            registryKey.Close();
        }

        public static Task DisableAsync()
        {
            return Task.Run(() => Disable());
        }

        public static void Disable()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

            if (registryKey == null)
                return; // LOG

            registryKey.DeleteValue(ConfigurationManager.Current.ApplicationName);
            registryKey.Close();
        }
    }
}
