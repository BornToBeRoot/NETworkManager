using Microsoft.Win32;
using System.Reflection;

namespace NETworkManager.Settings
{
    public class Autostart
    {
        // Name of the Application
        private static string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;

        // Key where the autorun entries for the current user are stored
        private const string RunKeyCurrentUser = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        /// <summary>
        /// Indicates if the autostart is enabled or disabled for the current user
        /// </summary>
        public static bool IsEnabled
        {
            get
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser);

                if (registryKey.GetValue(ApplicationName) != null)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Enables the autostart
        /// </summary>
        public static void Enable()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

            string command = string.Format("{0} --{1}", Assembly.GetExecutingAssembly().Location, Properties.Resources.StartParameter_Autostart);

            registryKey.SetValue(ApplicationName, command);
            registryKey.Close();
        }

        /// <summary>
        /// Disables the autostart
        /// </summary>
        public static void Disable()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

            registryKey.DeleteValue(ApplicationName);
            registryKey.Close();
        }
    }
}
