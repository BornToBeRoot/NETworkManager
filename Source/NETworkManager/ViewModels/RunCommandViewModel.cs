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
    ///     Maximum number of recent commands shown in the suggestions.
    /// </summary>
    private const int MaxHistorySuggestions = 5;

    private readonly IReadOnlyList<RunCommandDefinition> _commands;
    private readonly Action<RunCommandViewModel> _runHandler;

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

        ErrorMessage = RunCommandManager.TryCreateRequest(result, suggestion?.Profile, out var request);

        if (ErrorMessage != null)
            return;

        Request = request;
        IsSettings = result.Command.Type == RunCommandType.Setting;

        ListHelper.Modify(SettingsManager.Current.RunCommand_History, text.Trim(),
            SettingsManager.Current.General_HistoryListEntries);

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
            RunCommandPosition.Target when _result.ProfileName != null => GetProfileSuggestions(),
            RunCommandPosition.Target => [GetRunSuggestion(), .. GetProfileSuggestions(), .. GetHistorySuggestions()],
            _ => [GetRunSuggestion(), .. GetTypeSuggestions(), .. GetHistorySuggestions()]
        };

        if (_result.Position != RunCommandPosition.Command)
            suggestions = suggestions.Take(MaxSuggestions);

        Suggestions.Clear();

        foreach (var suggestion in suggestions)
            Suggestions.Add(suggestion);

        // The input as typed is the first suggestion, so Enter runs what was typed. Profiles, history entries and
        // types are selected with Up/Down.
        SelectedSuggestion = Suggestions.FirstOrDefault();
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

        foreach (var command in _commands)
        {
            var names = new[] { command.Command }.Concat(command.Aliases).ToList();
            var translatedName = RunCommandManager.GetTranslatedName(command);

            // Complete with the command or alias that was typed (e.g. "ping" instead of "pingmonitor").
            var name = names.FirstOrDefault(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase)) ??
                       names.FirstOrDefault(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase));

            if (name == null && !translatedName.Contains(text, StringComparison.OrdinalIgnoreCase))
                continue;

            suggestions.Add(new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Command,
                Application = command.Application,
                Title = translatedName,
                Description = string.Join(", ", names),
                Info = command.Syntax,
                CompletionText = (name ?? command.Command) + " "
            });
        }

        return suggestions;
    }

    /// <summary>
    ///     Profiles of the command that match the typed profile name (or target).
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetProfileSuggestions()
    {
        var text = _result.ProfileName ?? _result.CurrentWord;

        return RunCommandManager.GetProfiles(_result.Command)
            .Where(x => string.IsNullOrEmpty(text) ||
                        x.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                        (x.Host?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Profile,
                Application = _result.Command.Application,
                Title = x.Name,
                Description = string.IsNullOrEmpty(x.Host) ? x.Group : $"{x.Group} · {x.Host}",
                Info = "@" + x.Name,
                CompletionText = _result.InputBeforeCurrentWord + "@" + x.Name,
                Profile = x
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
    ///     Recent commands. With a command, only its entries that contain the input.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetHistorySuggestions()
    {
        var input = Input.Trim();

        return SettingsManager.Current.RunCommand_History
            .Where(x => _result.Command == null && string.IsNullOrEmpty(input) ||
                        _result.Command != null &&
                        RunCommandParser.Parse(x, _commands).Command == _result.Command &&
                        x.Contains(_result.CurrentWord, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(x, input, StringComparison.OrdinalIgnoreCase))
            .Select(x =>
            {
                var command = RunCommandParser.Parse(x, _commands).Command;

                return new RunCommandSuggestion
                {
                    Kind = RunCommandSuggestionKind.History,
                    Application = command?.Application ?? Models.ApplicationName.None,
                    Title = x,
                    Description = command == null ? null : RunCommandManager.GetTranslatedName(command),
                    Info = Strings.Recent,
                    CompletionText = x
                };
            });
    }

    #endregion
}
