using NETworkManager.Models;
using NETworkManager.Profiles;

namespace NETworkManager;

/// <summary>
///     Kind of a suggestion in the run command dialog.
/// </summary>
public enum RunCommandSuggestionKind
{
    /// <summary>
    ///     Run the input as typed.
    /// </summary>
    Run,

    /// <summary>
    ///     Command of an application (or the settings).
    /// </summary>
    Command,

    /// <summary>
    ///     Profile of the application.
    /// </summary>
    Profile,

    /// <summary>
    ///     Recently run command.
    /// </summary>
    History,

    /// <summary>
    ///     Type keyword of the command (e.g. DNS record type).
    /// </summary>
    Type
}

/// <summary>
///     Suggestion in the run command dialog.
/// </summary>
public sealed class RunCommandSuggestion
{
    public RunCommandSuggestionKind Kind { get; init; }

    /// <summary>
    ///     Application of the suggestion (for the icon), <see cref="ApplicationName.None" /> for the settings.
    /// </summary>
    public ApplicationName Application { get; init; }

    public string Title { get; init; }

    /// <summary>
    ///     Second line (e.g. group and host of a profile).
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    ///     Text on the right (e.g. "@name" of a profile).
    /// </summary>
    public string Info { get; init; }

    /// <summary>
    ///     Input after completing the suggestion (Tab) and that is run (Enter).
    /// </summary>
    public string CompletionText { get; init; }

    /// <summary>
    ///     Profile to run (<see cref="RunCommandSuggestionKind.Profile" />).
    /// </summary>
    public ProfileInfo Profile { get; init; }
}
