using NETworkManager.Localization.Resources;
using NETworkManager.Models.RunCommand;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
///     View model of the run command dialog: type a command (e.g. "ping 10.0.0.1"), select a suggestion and run it.
/// </summary>
public class RunCommandViewModel : ViewModelBase
{
    /// <summary>
    ///     Maximum number of suggestions shown for a target or argument (profiles, history, types). The commands are
    ///     always shown completely.
    /// </summary>
    private const int MaxSuggestions = 10;

    /// <summary>
    ///     Maximum number of recent commands shown with an empty input (before the commands).
    /// </summary>
    private const int MaxHistorySuggestions = 5;

    /// <summary>
    ///     Maximum number of recent commands of a command shown before its profiles.
    /// </summary>
    private const int MaxCommandHistorySuggestions = 3;

    private readonly IReadOnlyList<RunCommandDefinition> _commands;
    private readonly Action<RunCommandViewModel> _runHandler;

    /// <summary>
    ///     Recent commands with their command (parsed once, the history doesn't change while the dialog is open).
    /// </summary>
    private readonly List<(string Text, RunCommandDefinition Command)> _history;

    /// <summary>
    ///     Names (command and aliases) and translated name per command.
    /// </summary>
    private readonly List<(RunCommandDefinition Command, List<string> Names, string TranslatedName)> _commandNames;

    /// <summary>
    ///     Profiles per command (loaded once per command while the dialog is open).
    /// </summary>
    private readonly Dictionary<RunCommandDefinition, List<ProfileInfo>> _profiles = [];

    private RunCommandParseResult _result;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RunCommandViewModel" /> class.
    /// </summary>
    /// <param name="runHandler">Called with the request to run (<see cref="Request" />, <see cref="IsSettings" />).</param>
    /// <param name="cancelHandler">Called when the dialog is canceled.</param>
    public RunCommandViewModel(Action<RunCommandViewModel> runHandler, Action<RunCommandViewModel> cancelHandler)
    {
        _commands = RunCommandManager.GetCommands();
        _runHandler = runHandler;

        _commandNames =
        [
            .. _commands.Select(x => (x, new List<string> { x.Command }.Concat(x.Aliases).ToList(),
                RunCommandManager.GetTranslatedName(x)))
        ];

        // Entries of commands that are not available anymore (e.g. a hidden application) are not shown.
        _history =
        [
            .. SettingsManager.Current.RunCommand_History.Select(x => (x, RunCommandParser.Parse(x, _commands).Command))
                .Where(x => x.Command != null)
        ];

        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        UpdateSuggestions();
    }

    #region Properties

    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Input of the user (e.g. "dns borntoberoot.net aaaa").
    /// </summary>
    public string Input
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();

            ErrorMessage = null;
            UpdateSuggestions();
        }
    } = string.Empty;

    public ObservableCollection<RunCommandSuggestion> Suggestions { get; } = [];

    public RunCommandSuggestion SelectedSuggestion
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Syntax of the recognized command (e.g. "dnslookup &lt;hosts&gt; [server] [type] | @&lt;profile&gt;").
    /// </summary>
    public string Hint
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Error of the last run (e.g. invalid host), null if there is none.
    /// </summary>
    public string ErrorMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Request to run (set before the run handler is called), null for the settings.
    /// </summary>
    public ApplicationRequest Request { get; private set; }

    /// <summary>
    ///     The settings should be opened (set before the run handler is called).
    /// </summary>
    public bool IsSettings { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Selects the next suggestion (Down).
    /// </summary>
    public void SelectNext()
    {
        Select(1);
    }

    /// <summary>
    ///     Selects the previous suggestion (Up).
    /// </summary>
    public void SelectPrevious()
    {
        Select(-1);
    }

    private void Select(int offset)
    {
        if (Suggestions.Count == 0)
            return;

        var index = SelectedSuggestion == null ? -1 : Suggestions.IndexOf(SelectedSuggestion);

        SelectedSuggestion = Suggestions[Math.Clamp(index + offset, 0, Suggestions.Count - 1)];
    }

    /// <summary>
    ///     Inserts the selected suggestion into the input (Tab).
    /// </summary>
    public void Complete()
    {
        if (SelectedSuggestion == null || SelectedSuggestion.Kind == RunCommandSuggestionKind.Run)
            return;

        Input = SelectedSuggestion.CompletionText;
    }

    /// <summary>
    ///     Runs the selected suggestion, or the input if none is selected (Enter).
    /// </summary>
    public void Run()
    {
        var suggestion = SelectedSuggestion;

        var text = suggestion?.CompletionText ?? Input;
        var result = RunCommandParser.Parse(text, _commands);

        var error = RunCommandManager.TryCreateRequest(result, suggestion?.Profile, out var request);

        if (error != null)
        {
            // Show the text of a failed suggestion (e.g. a recent command with an ambiguous profile name), so it can be
            // corrected or completed with the suggestions.
            if (suggestion != null && suggestion.Kind != RunCommandSuggestionKind.Run)
                Input = text;

            ErrorMessage = error;

            return;
        }

        Request = request;
        IsSettings = result.Command.Type == RunCommandType.Setting;

        // Commands are case-insensitive, so "Ping 10.0.0.1" and "ping 10.0.0.1" are the same entry.
        ListHelper.Modify(SettingsManager.Current.RunCommand_History, text.Trim(),
            SettingsManager.Current.General_HistoryListEntries, StringComparer.OrdinalIgnoreCase);

        _runHandler(this);
    }

    /// <summary>
    ///     Updates the suggestions and the hint for the input.
    /// </summary>
    private void UpdateSuggestions()
    {
        _result = RunCommandParser.Parse(Input, _commands);

        Hint = _result.Command == null
            ? null
            : $"{_result.Command.Command} {_result.Command.Syntax}".TrimEnd();

        IEnumerable<RunCommandSuggestion> suggestions = _result.Position switch
        {
            RunCommandPosition.Command => GetCommandSuggestions(),
            RunCommandPosition.Type =>
                [GetRunSuggestion(), .. GetTypeSuggestions(), .. GetHistorySuggestions()],
            RunCommandPosition.Target when _result.ProfileName != null => GetProfileSuggestions(),
            RunCommandPosition.Target =>
            [
                GetRunSuggestion(), .. GetHistorySuggestions().Take(MaxCommandHistorySuggestions),
                .. GetProfileSuggestions()
            ],
            _ =>
            [
                GetRunSuggestion(), .. GetTypeSuggestions(), .. GetDNSServerSuggestions(),
                .. GetHistorySuggestions()
            ]
        };

        if (_result.Position != RunCommandPosition.Command)
            suggestions = suggestions.Take(MaxSuggestions);

        Suggestions.Clear();

        foreach (var suggestion in suggestions)
            Suggestions.Add(suggestion);

        SelectedSuggestion = GetPreselectedSuggestion();
    }

    /// <summary>
    ///     Gets the suggestion that Enter runs without selecting one, so Enter runs what was typed:
    ///     - After the command: the input as typed (first suggestion).
    ///     - While typing the command: the best matching command, but not if the input is empty or an unknown command has
    ///     arguments (e.g. a typo like "pin 10.0.0.1"), which would drop the input.
    ///     - While typing a profile ("@"): the profile, only if exactly one matches. Otherwise, the typed name is used,
    ///     which must be unique (e.g. two profiles with the same name in different groups).
    ///     - While typing a type before the target (e.g. "lookup po"): the matching type, like the command.
    /// </summary>
    private RunCommandSuggestion GetPreselectedSuggestion()
    {
        return _result.Position switch
        {
            RunCommandPosition.Command when string.IsNullOrWhiteSpace(Input) ||
                                            _result.Command == null && GetInputAfterCommand().Length > 0 => null,
            RunCommandPosition.Target when _result.ProfileName != null => Suggestions.Count == 1 ? Suggestions[0] : null,
            RunCommandPosition.Type when _result.CurrentWord.Length > 0 =>
                Suggestions.FirstOrDefault(x => x.Kind == RunCommandSuggestionKind.Type) ?? Suggestions.FirstOrDefault(),
            _ => Suggestions.FirstOrDefault()
        };
    }

    /// <summary>
    ///     Gets the input after the first word (the command), e.g. "10.0.0.1" for "pin 10.0.0.1".
    /// </summary>
    private string GetInputAfterCommand()
    {
        var input = Input.TrimStart();
        var index = input.IndexOfAny([' ', '\t']);

        return index < 0 ? string.Empty : input[index..].Trim();
    }

    /// <summary>
    ///     Suggestion to run the input as typed.
    /// </summary>
    private RunCommandSuggestion GetRunSuggestion()
    {
        return new RunCommandSuggestion
        {
            Kind = RunCommandSuggestionKind.Run,
            Application = _result.Command.Application,
            Title = Input.Trim(),
            Description = RunCommandManager.GetTranslatedName(_result.Command),
            CompletionText = Input
        };
    }

    /// <summary>
    ///     Recent commands (empty input) and the commands that match the typed command.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetCommandSuggestions()
    {
        var text = _result.CurrentWord;

        var suggestions = new List<RunCommandSuggestion>();

        if (string.IsNullOrEmpty(text))
            suggestions.AddRange(GetHistorySuggestions().Take(MaxHistorySuggestions));

        // Completing the command keeps the arguments (e.g. "pin 10.0.0.1" → "ping 10.0.0.1").
        var arguments = GetInputAfterCommand();

        var commands = new List<(int Rank, RunCommandSuggestion Suggestion)>();

        foreach (var (command, names, translatedName) in _commandNames)
        {
            // Complete with the command or alias that was typed, or the shortest one that starts with the text (e.g.
            // "pin" → "ping" instead of "pingmonitor").
            var exactName = names.FirstOrDefault(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase));
            var name = exactName ?? names.Where(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Length).FirstOrDefault();

            if (name == null && !translatedName.Contains(text, StringComparison.OrdinalIgnoreCase))
                continue;

            // Exact command or alias first, then commands that start with the text, then translated names that contain
            // it (e.g. "lookup" before "DNS Lookup").
            var rank = exactName != null ? 0 : name != null ? 1 : 2;

            commands.Add((rank, new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Command,
                Application = command.Application,
                Title = translatedName,
                Description = string.Join(", ", names),
                Info = command.Syntax,
                CompletionText = (name ?? command.Command) + " " + arguments
            }));
        }

        suggestions.AddRange(commands.OrderBy(x => x.Rank).Select(x => x.Suggestion));

        return suggestions;
    }

    /// <summary>
    ///     Profiles of the command that match the typed profile name (or target).
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetProfileSuggestions()
    {
        var text = _result.ProfileName ?? _result.CurrentWord;
        var application = _result.Command.Application;

        // Exact name first, then names that start with the text, then names and targets (e.g. host) that contain it.
        static int? GetRank(ProfileInfo profile, string target, string text)
        {
            if (string.IsNullOrEmpty(text) || string.Equals(profile.Name, text, StringComparison.OrdinalIgnoreCase))
                return 0;

            if (profile.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                return 1;

            if (profile.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (target?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
                return 2;

            return null;
        }

        if (!_profiles.TryGetValue(_result.Command, out var profiles))
        {
            profiles = [.. RunCommandManager.GetProfiles(_result.Command)];
            _profiles[_result.Command] = profiles;
        }

        return profiles
            .Select(x => (Profile: x, Target: x.GetTarget(application)))
            .Select(x => (x.Profile, x.Target, Rank: GetRank(x.Profile, x.Target, text)))
            .Where(x => x.Rank != null)
            .OrderBy(x => x.Rank)
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Profile,
                Application = application,
                Title = x.Profile.Name,
                // The target of the application (e.g. the RDP host), which can differ from the general host.
                Description = string.IsNullOrEmpty(x.Target) ? x.Profile.Group : $"{x.Profile.Group} · {x.Target}",
                Info = "@" + x.Profile.Name,
                CompletionText = _result.InputBeforeCurrentWord + "@" + x.Profile.Name,
                Profile = x.Profile
            });
    }

    /// <summary>
    ///     Type keywords of the command that start with the typed word.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetTypeSuggestions()
    {
        // A complete type was already entered (and is not being typed).
        if (_result.Type != null && !string.Equals(_result.Type, _result.CurrentWord, StringComparison.OrdinalIgnoreCase))
            return [];

        return _result.Command.Types
            .Where(x => x.StartsWith(_result.CurrentWord, StringComparison.OrdinalIgnoreCase))
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Type,
                Application = _result.Command.Application,
                Title = x.ToUpperInvariant(),
                Description = Strings.Type,
                CompletionText = _result.InputBeforeCurrentWord + x + " "
            });
    }

    /// <summary>
    ///     DNS servers configured in the DNS lookup (e.g. "Cloudflare") that contain the typed word, if no DNS server was
    ///     entered yet. Names with spaces are inserted in quotes.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetDNSServerSuggestions()
    {
        if (_result.Command.Argument != RunCommandArgument.DNSServer ||
            _result.Argument != null && !string.Equals(_result.Argument, _result.CurrentWord))
            return [];

        return RunCommandManager.GetDNSServers()
            .Where(x => x.Contains(_result.CurrentWord, StringComparison.OrdinalIgnoreCase))
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.DNSServer,
                Application = _result.Command.Application,
                Title = x,
                Description = Strings.DNSServer,
                CompletionText = _result.InputBeforeCurrentWord + (x.Contains(' ') ? $"\"{x}\"" : x) + " "
            });
    }

    /// <summary>
    ///     Recent commands. With a command, only its entries that contain the input.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetHistorySuggestions()
    {
        var input = Input.Trim();

        return _history
            .Where(x => _result.Command == null && string.IsNullOrEmpty(input) ||
                        _result.Command != null &&
                        x.Command == _result.Command &&
                        x.Text.Contains(_result.CurrentWord, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(x.Text, input, StringComparison.OrdinalIgnoreCase))
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.History,
                Application = x.Command?.Application ?? Models.ApplicationName.None,
                Title = x.Text,
                Description = x.Command == null ? null : RunCommandManager.GetTranslatedName(x.Command),
                Info = Strings.Recent,
                CompletionText = x.Text
            });
    }

    #endregion
}
