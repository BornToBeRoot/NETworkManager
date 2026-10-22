using NETworkManager.Models;
using NETworkManager.Profiles;

namespace NETworkManager;

/// <summary>
///     Kind of a suggestion in the run command dialog.
/// </summary>
public enum RunCommandSuggestionKind
{
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
    Type,

    /// <summary>
    ///     DNS server configured in the DNS lookup.
    /// </summary>
    DNSServer,

    /// <summary>
    ///     Port profile of the Port Scanner (e.g. "Webserver").
    /// </summary>
    PortProfile,

    /// <summary>
    ///     Command for a target typed without a command (e.g. "ping 10.0.0.1" for "10.0.0.1").
    /// </summary>
    Target,

    /// <summary>
    ///     Command for the target in the clipboard (e.g. "ping 10.0.0.1").
    /// </summary>
    Clipboard,

    /// <summary>
    ///     Example command (empty input without history).
    /// </summary>
    Example
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
    ///     Typed text that is highlighted in the title and description.
    /// </summary>
    public string Highlight { get; init; }

    /// <summary>
    ///     Header of the group in the list (e.g. "Recent"), null for no header.
    /// </summary>
    public string Group { get; init; }

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
