namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Result of <see cref="RunCommandParser.Parse" />.
/// </summary>
public sealed class RunCommandParseResult
{
    /// <summary>
    ///     Command that matches the first word, or null if there is none.
    /// </summary>
    public RunCommandDefinition Command { get; init; }

    /// <summary>
    ///     First word as typed.
    /// </summary>
    public string CommandText { get; init; } = string.Empty;

    /// <summary>
    ///     Target (e.g. "10.0.0.1;10.0.0.2"), or null if there is none.
    /// </summary>
    public string Target { get; init; }

    /// <summary>
    ///     Name of the profile ("@name"), or null if no profile is used. Empty if only "@" was typed.
    /// </summary>
    public string ProfileName { get; init; }

    /// <summary>
    ///     Second argument (e.g. ports or DNS server), or null if there is none.
    /// </summary>
    public string Argument { get; init; }

    /// <summary>
    ///     Type keyword in lowercase (e.g. "aaaa"), or null if there is none.
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    ///     Part of the input that is being typed.
    /// </summary>
    public RunCommandPosition Position { get; init; }

    /// <summary>
    ///     Word that is being typed (empty after a space), used to filter the suggestions.
    /// </summary>
    public string CurrentWord { get; init; } = string.Empty;

    /// <summary>
    ///     Input without the word that is being typed (e.g. "dns borntoberoot.net " for "dns borntoberoot.net aa"),
    ///     used to complete a suggestion.
    /// </summary>
    public string InputBeforeCurrentWord { get; init; } = string.Empty;

    /// <summary>
    ///     Error, if the input can't be run.
    /// </summary>
    public RunCommandParseError Error { get; init; }

    /// <summary>
    ///     Value the error refers to (e.g. the unknown command or argument).
    /// </summary>
    public string ErrorValue { get; init; }
}
