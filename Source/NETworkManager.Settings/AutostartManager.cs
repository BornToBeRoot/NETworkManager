using Microsoft.Win32;
using System.Threading.Tasks;

namespace NETworkManager.Settings;

/// <summary>
/// Class to manage the autostart of the application
/// </summary>
public class AutostartManager
{
    // Key where the autorun entries for the current user are stored
    private const string RunKeyCurrentUser = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Indicates if the application autostart is enabled.
    /// </summary>
    public static bool IsEnabled
    {
        get
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser);

            return registryKey?.GetValue(ConfigurationManager.Current.ApplicationName) != null;
        }
    }

    /// <summary>
    /// Enable the autostart of the application async.
    /// </summary>
    /// <returns><see cref="Task"/> to wait for.</returns>
    public static Task EnableAsync()
    {
        return Task.Run(() => Enable());
    }

    /// <summary>
    /// Enable the autostart of the application.
    /// </summary>
    public static void Enable()
    {
        var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

        var command = $"{ConfigurationManager.Current.ApplicationFullName} {CommandLineManager.ParameterAutostart}";

        if (registryKey == null)
            return; // LOG

        registryKey.SetValue(ConfigurationManager.Current.ApplicationName, command);
        registryKey.Close();
    }

    /// <summary>
    /// Disable the autostart of the application async.
    /// </summary>
    /// <returns><see cref="Task"/> to wait for.</returns>
    public static Task DisableAsync()
    {
        return Task.Run(() => Disable());
    }

    /// <summary>
    /// Disable the autostart of the application.
    /// </summary>
    public static void Disable()
    {
        var registryKey = Registry.CurrentUser.OpenSubKey(RunKeyCurrentUser, true);

        if (registryKey == null)
            return; // LOG

        registryKey.DeleteValue(ConfigurationManager.Current.ApplicationName);
        registryKey.Close();
    }
}
