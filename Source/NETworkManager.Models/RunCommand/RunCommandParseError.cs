namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Errors of a run command input.
/// </summary>
public enum RunCommandParseError
{
    None,
    Empty,
    UnknownCommand,
    NoArguments,
    NoProfiles,
    MissingProfileName,
    UnknownArgument,

    /// <summary>
    ///     A command without a target that accepts a profile (e.g. "networkinterface") got an argument without "@".
    /// </summary>
    OnlyProfile
}
