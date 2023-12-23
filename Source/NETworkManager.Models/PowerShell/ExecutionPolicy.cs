namespace NETworkManager.Models.PowerShell;

/// <summary>
///     Represents the execution policy for the current PowerShell session.
/// </summary>
public enum ExecutionPolicy
{
    /// <summary>
    ///     Restricted execution policy. No scripts can be run. Windows PowerShell can be used only in
    ///     interactive mode.
    /// </summary>
    Restricted,

    /// <summary>
    ///     AllSigned execution policy. Scripts can be run only if they are signed by a trusted publisher.
    /// </summary>
    AllSigned,

    /// <summary>
    ///     RemoteSigned execution policy. Scripts can be run only if they are signed by a trusted publisher
    ///     or if they were written on the local computer.
    /// </summary>
    RemoteSigned,

    /// <summary>
    ///     Unrestricted execution policy. No restrictions; all Windows PowerShell scripts can be run.
    /// </summary>
    Unrestricted,

    /// <summary>
    ///     Bypass execution policy. Nothing is blocked and there are no warnings or prompts.
    /// </summary>
    Bypass
}