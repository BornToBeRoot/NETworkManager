using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NETworkManager.Models.RunCommand;

/// <summary>
///     Parses the input of the run command dialog: "command target [argument] [type]" or "command @profile".
/// </summary>
public static partial class RunCommandParser
{
    /// <summary>
    ///     A list separator with whitespace around it ("10.0.0.1; 10.0.0.2"), or a value in quotes, which is kept as it is.
    /// </summary>
    [GeneratedRegex("\"[^\"]*(?:\"|$)|\\s*;\\s*")]
    private static partial Regex ListSeparatorOrQuoteRegex();

    /// <summary>
    ///     A word: a value in quotes (can contain spaces, the closing quote can be missing while typing) or a value
    ///     without spaces.
    /// </summary>
    [GeneratedRegex("\"[^\"]*(?:\"|$)|[^\\s\"]+")]
    private static partial Regex WordRegex();

    /// <summary>
    ///     Parses the input.
    ///     - Lists use ";" as everywhere in the application. Whitespace around ";" is removed, so whitespace only
    ///     separates the arguments.
    ///     - The first word is the command (name or alias), the second word the target. "@name" (the rest of the input,
    ///     since profile names can contain spaces) is a profile instead of a target.
    ///     - A value with spaces is written in quotes (e.g. a DNS server "Google Public DNS").
    ///     - Further words are a type keyword of the command or its second argument (e.g. ports, DNS server), in any
    ///     order.
    /// </summary>
    /// <param name="input">Input of the dialog.</param>
    /// <param name="commands">Available commands.</param>
    /// <returns>Parse result, see <see cref="RunCommandParseResult.Error" />.</returns>
    public static RunCommandParseResult Parse(string input, IEnumerable<RunCommandDefinition> commands)
    {
        input = (input ?? string.Empty).TrimStart();

        // Whitespace around ";" is removed (outside of quotes), so a list that is being typed ("10.0.0.1; ") is still
        // the same word.
        var normalized = ListSeparatorOrQuoteRegex().Replace(input, x => x.Value.StartsWith('"') ? x.Value : ";");

        // Words as typed (with quotes, to rebuild the input) and their values (without quotes)
        var tokens = WordRegex().Matches(normalized).Select(x => x.Value).ToArray();
        var words = tokens.Select(Unquote).ToArray();

        // Whitespace in an open quote (e.g. "Google ) is part of the word that is being typed.
        var endsWithWhitespace = normalized.Length > 0 && char.IsWhiteSpace(normalized[^1]) &&
                                 !(tokens.Length > 0 && IsOpenQuote(tokens[^1]));

        if (words.Length == 0)
            return new RunCommandParseResult { Position = RunCommandPosition.Command, Error = RunCommandParseError.Empty };

        var commandText = words[0];
        var command = commands.FirstOrDefault(x => x.Matches(commandText));

        // The command is being typed.
        if (command == null || words.Length == 1 && !endsWithWhitespace)
            return new RunCommandParseResult
            {
                Command = command,
                CommandText = commandText,
                Position = RunCommandPosition.Command,
                CurrentWord = commandText,
                Error = command == null ? RunCommandParseError.UnknownCommand : RunCommandParseError.None,
                ErrorValue = command == null ? commandText : null
            };

        var commandPrefix = commandText + " ";

        // Only the command (e.g. "ping ").
        if (words.Length == 1)
            return new RunCommandParseResult
            {
                Command = command,
                CommandText = commandText,
                Position = RunCommandPosition.Target,
                InputBeforeCurrentWord = commandPrefix
            };

        // Profile: the rest of the input after "@" is the name.
        if (words[1].StartsWith('@'))
        {
            // Quotes around the name or the whole target (e.g. @"My Server" or "@My Server") are removed.
            var profileName = input[(input.IndexOf('@') + 1)..].Trim().Trim('"').Trim();

            var error = !command.SupportsProfile
                ? RunCommandParseError.NoProfiles
                : profileName.Length == 0
                    ? RunCommandParseError.MissingProfileName
                    : RunCommandParseError.None;

            return new RunCommandParseResult
            {
                Command = command,
                CommandText = commandText,
                ProfileName = profileName,
                Position = RunCommandPosition.Target,
                CurrentWord = "@" + profileName,
                InputBeforeCurrentWord = commandPrefix,
                Error = error,
                ErrorValue = error == RunCommandParseError.NoProfiles ? command.Command : null
            };
        }

        if (!command.SupportsTarget)
            return new RunCommandParseResult
            {
                Command = command,
                CommandText = commandText,
                Position = RunCommandPosition.Target,
                CurrentWord = words[1],
                InputBeforeCurrentWord = commandPrefix,
                Error = RunCommandParseError.NoArguments,
                ErrorValue = command.Command
            };

        // Further words: type keyword or second argument, in any order. A type keyword is never the second argument.
        string type = null;
        string argument = null;
        string unknownArgument = null;

        foreach (var word in words[2..])
        {
            var keyword = word.ToLowerInvariant();

            if (command.Types.Contains(keyword))
            {
                if (type == null)
                    type = keyword;
                else
                    unknownArgument ??= word;
            }
            else if (argument == null && command.Argument != RunCommandArgument.None)
            {
                argument = word;
            }
            else
            {
                unknownArgument ??= word;
            }
        }

        var isTypingTarget = words.Length == 2 && !endsWithWhitespace;
        var currentWord = endsWithWhitespace ? string.Empty : words[^1];

        return new RunCommandParseResult
        {
            Command = command,
            CommandText = commandText,
            Target = words[1],
            Argument = argument,
            Type = type,
            Position = isTypingTarget ? RunCommandPosition.Target : RunCommandPosition.Argument,
            CurrentWord = currentWord,
            InputBeforeCurrentWord = string.Join(" ", tokens[..^(endsWithWhitespace ? 0 : 1)]) + " ",
            Error = unknownArgument == null ? RunCommandParseError.None : RunCommandParseError.UnknownArgument,
            ErrorValue = unknownArgument
        };
    }

    /// <summary>
    ///     Removes the quotes of a value (e.g. "Google Public DNS").
    /// </summary>
    private static string Unquote(string word)
    {
        return word.StartsWith('"') ? word.Trim('"') : word;
    }

    private static bool IsOpenQuote(string token)
    {
        return token.StartsWith('"') && (token.Length == 1 || !token.EndsWith('"'));
    }
}
