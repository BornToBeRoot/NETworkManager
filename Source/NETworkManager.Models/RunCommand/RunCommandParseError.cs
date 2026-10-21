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
    UnknownArgument
}
