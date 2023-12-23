using NETworkManager.Models;

namespace NETworkManager.Settings;

/// <summary>
///     Class to store the command line parameters
/// </summary>
public class CommandLineInfo
{
    /// <summary>
    ///     Indicates if the application should show the help dialog
    /// </summary>
    public bool Help { get; set; }

    /// <summary>
    ///     Indicates if the application was started automatically
    /// </summary>
    public bool Autostart { get; set; }

    /// <summary>
    ///     Indicates if the application should reset the settings
    /// </summary>
    public bool ResetSettings { get; set; }

    /// <summary>
    ///     Process ID of the previous instance of the application to wait for it to close
    /// </summary>
    public int RestartPid { get; set; } = -1;

    /// <summary>
    ///     Name of the application to start
    /// </summary>
    public ApplicationName Application { get; set; } = ApplicationName.None;

    /// <summary>
    ///     Wrong parameter(s) detected in the command line to display in the help dialog
    /// </summary>
    public string WrongParameter { get; set; }
}