using System.IO;
using System.Security.Principal;
using NETworkManager.Models;

namespace NETworkManager.Settings;

/// <summary>
///     Class includes static and dynamic configuration used in the application
///     across multiple windows, views, dialogs, etc.
/// </summary>
public static class ConfigurationManager
{
    /// <summary>
    ///     Name of the file that indicates that the application is portable.
    /// </summary>
    private const string IsPortableFileName = "IsPortable";

    /// <summary>
    ///     Extension of the file that indicates that the application is portable.
    /// </summary>
    private const string IsPortableExtension = "settings";

    /// <summary>
    ///     Create a new instance of the <see cref="ConfigurationManager" /> class and load static configuration.
    /// </summary>
    static ConfigurationManager()
    {
        Current = new ConfigurationInfo(
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator),
            AssemblyManager.Current.Location,
            Path.Combine(AssemblyManager.Current.Location, AssemblyManager.Current.Name + ".exe"),
            AssemblyManager.Current.Name,
            File.Exists(Path.Combine(AssemblyManager.Current.Location, $"{IsPortableFileName}.{IsPortableExtension}")));
    }

    /// <summary>
    ///     Current <see cref="ConfigurationInfo" /> that is used in the application.
    /// </summary>
    public static ConfigurationInfo Current { get; }

    /// <summary>
    ///     Method can be called before opening a dialog to fix airspace issues.
    ///     Call the <see cref="OnDialogClose" /> method after the dialog has been closed.
    /// </summary>
    public static void OnDialogOpen()
    {
        switch (Current.CurrentApplication)
        {
            case ApplicationName.RemoteDesktop when Current.RemoteDesktopTabCount > 0:
            case ApplicationName.PowerShell when Current.PowerShellTabCount > 0:
            case ApplicationName.PuTTY when Current.PuTTYTabCount > 0:
            case ApplicationName.AWSSessionManager when Current.AWSSessionManagerTabCount > 0:
            case ApplicationName.TigerVNC when Current.TigerVNCTabCount > 0:
            case ApplicationName.WebConsole when Current.WebConsoleTabCount > 0:
                Current.FixAirspace = true;
                break;
        }
    }

    /// <summary>
    ///     Method must be called after closing a dialog if <see cref="OnDialogOpen" /> was called before.
    /// </summary>
    public static void OnDialogClose()
    {
        Current.FixAirspace = false;
    }
}