using System.Security.Principal;
using System.IO;

namespace NETworkManager.Settings;

/// <summary>
/// Class includes static and dynamic configuration used in the application 
/// across multiple windows, views, dialogs, etc.
/// </summary>
public static class ConfigurationManager
{
    /// <summary>
    /// Name of the file that indicates that the application is portable.
    /// </summary>
    private const string IsPortableFileName = "IsPortable";

    /// <summary>
    /// Extension of the file that indicates that the application is portable.
    /// </summary>
    private const string IsPortableExtension = "settings";

    /// <summary>
    /// Current <see cref="ConfigurationInfo"/> that is used in the application.
    /// </summary>
    public static ConfigurationInfo Current { get; set; }

    /// <summary>
    /// Create a new instance of the <see cref="ConfigurationManager"/> class and load static configuration.
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
    /// Method can be called before opening a dialog to fix airspace issues. 
    /// Call the <see cref="OnDialogClose"/> method after the dialog has been closed.
    /// </summary>
    public static void OnDialogOpen()
    {
        if (Current.CurrentApplication == Models.ApplicationName.RemoteDesktop && Current.RemoteDesktopHasTabs)
            Current.FixAirspace = true;

        if (Current.CurrentApplication == Models.ApplicationName.PowerShell && Current.PowerShellHasTabs)
            Current.FixAirspace = true;

        if (Current.CurrentApplication == Models.ApplicationName.PuTTY && Current.PuTTYHasTabs)
            Current.FixAirspace = true;

        if (Current.CurrentApplication == Models.ApplicationName.AWSSessionManager && Current.AWSSessionManagerHasTabs)
            Current.FixAirspace = true;

        if (Current.CurrentApplication == Models.ApplicationName.TigerVNC && Current.TigerVNCHasTabs)
            Current.FixAirspace = true;

        if (Current.CurrentApplication == Models.ApplicationName.WebConsole && Current.WebConsoleHasTabs)
            Current.FixAirspace = true;
    }

    /// <summary>
    /// Method must be called after closing a dialog if <see cref="OnDialogOpen"/> was called before.
    /// </summary>
    public static void OnDialogClose()
    {
        Current.FixAirspace = false;
    }
}
