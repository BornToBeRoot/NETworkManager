namespace NETworkManager.Models.PowerShell;

/// <summary>
///     Class to store information's about a PowerShell session
/// </summary>
public class PowerShellSessionInfo
{
    /// <summary>
    ///     Path to the PowerShell executable.
    /// </summary>
    public string ApplicationFilePath { get; set; }

    /// <summary>
    ///     Enable the remote console.
    /// </summary>
    public bool EnableRemoteConsole { get; set; }

    /// <summary>
    ///     Host to connect to.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     Command to execute.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    ///     Additional command line arguments.
    /// </summary>
    public string AdditionalCommandLine { get; set; }

    /// <summary>
    ///     Execution policy to use.
    /// </summary>
    public ExecutionPolicy ExecutionPolicy { get; set; }
}