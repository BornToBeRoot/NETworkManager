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
    ///     A list separator with whitespace around it ("10.0.0.1; 10.0.0.2").
    /// </summary>
    [GeneratedRegex(@"\s*;\s*")]
    private static partial Regex ListSeparatorRegex();

    /// <summary>
    ///     Parses the input.
    ///     - Lists use ";" as everywhere in the application. Whitespace around ";" is removed, so whitespace only
    ///     separates the arguments.
    ///     - The first word is the command (name or alias), the second word the target. "@name" (the rest of the input,
    ///     since profile names can contain spaces) is a profile instead of a target.
    ///     - Further words are a type keyword of the command or its second argument (e.g. ports, DNS server), in any
    ///     order.
    /// </summary>
    /// <param name="input">Input of the dialog.</param>
    /// <param name="commands">Available commands.</param>
    /// <returns>Parse result, see <see cref="RunCommandParseResult.Error" />.</returns>
    public static RunCommandParseResult Parse(string input, IEnumerable<RunCommandDefinition> commands)
    {
        input = (input ?? string.Empty).TrimStart();

        var endsWithWhitespace = input.Length > 0 && char.IsWhiteSpace(input[^1]);
        var words = ListSeparatorRegex().Replace(input, ";").Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

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
            var profileName = input[(input.IndexOf('@') + 1)..].Trim();

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
            InputBeforeCurrentWord = string.Join(" ", words[..^(currentWord.Length == 0 ? 0 : 1)]) + " ",
            Error = unknownArgument == null ? RunCommandParseError.None : RunCommandParseError.UnknownArgument,
            ErrorValue = unknownArgument
        };
    }
}
