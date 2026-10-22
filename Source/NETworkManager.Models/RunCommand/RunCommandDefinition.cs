using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Command of the run command dialog (e.g. "dnslookup" with the alias "dns"), which opens an application or the
///     settings.
/// </summary>
public sealed class RunCommandDefinition
{
    /// <summary>
    ///     Whether the command opens an application or the settings.
    /// </summary>
    public RunCommandType Type { get; init; }

    /// <summary>
    ///     Application to open (<see cref="ApplicationName.None" /> for the settings).
    /// </summary>
    public ApplicationName Application { get; init; }

    /// <summary>
    ///     Command (lowercase, e.g. "dnslookup").
    /// </summary>
    public string Command { get; init; }

    /// <summary>
    ///     Short forms of the command (lowercase, e.g. "dns", "nslookup").
    /// </summary>
    public IReadOnlyList<string> Aliases { get; init; } = [];

    /// <summary>
    ///     The command accepts a target (e.g. host, IP range, query).
    /// </summary>
    public bool SupportsTarget { get; init; }

    /// <summary>
    ///     The command accepts a profile ("@name") instead of a target.
    /// </summary>
    public bool SupportsProfile { get; init; }

    /// <summary>
    ///     Second argument after the target (e.g. ports or DNS server).
    /// </summary>
    public RunCommandArgument Argument { get; init; }

    /// <summary>
    ///     Type keywords that can follow the target (lowercase, e.g. "a", "aaaa" for the DNS lookup).
    /// </summary>
    public IReadOnlyList<string> Types { get; init; } = [];

    /// <summary>
    ///     The type keyword is required and is the first word after the command (e.g. "lookup port 22"). The rest of the
    ///     input is the target, so it can contain spaces (e.g. "lookup oui Cisco Systems").
    /// </summary>
    public bool TypeBeforeTarget { get; init; }

    /// <summary>
    ///     Parts of the syntax of the arguments for the hint (e.g. "&lt;Hosts&gt;", "[DNS server]", "[Type]",
    ///     "@&lt;Profile&gt;"), empty if there are none.
    /// </summary>
    public IReadOnlyList<RunCommandSyntaxPart> SyntaxParts { get; init; } = [];

    /// <summary>
    ///     Syntax of the arguments (e.g. "&lt;Hosts&gt; [DNS server] [Type] | @&lt;Profile&gt;"), empty if there are
    ///     none.
    /// </summary>
    public string Syntax => string.Join(" ",
        SyntaxParts.Select(x => x.Kind == RunCommandSyntaxPartKind.Profile ? $"| {x.Text}" : x.Text));

    /// <summary>
    ///     Checks if the text is the command or one of its aliases (case-insensitive).
    /// </summary>
    /// <param name="text">Command text.</param>
    /// <returns>True if the text matches.</returns>
    public bool Matches(string text)
    {
        return string.Equals(Command, text, StringComparison.OrdinalIgnoreCase) ||
               Aliases.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase));
    }
}
