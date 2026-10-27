using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.RunCommand;
using NETworkManager.Controls;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

/// <summary>
///     View model of the run command dialog: type a command (e.g. "ping 10.0.0.1") or only a target (e.g. "10.0.0.1"),
///     select a suggestion and run it.
/// </summary>
public class RunCommandViewModel : ViewModelBase
{
    /// <summary>
    ///     Maximum number of suggestions per group shown for a target or argument (profiles, history, types, DNS
    ///     servers), so a large group (e.g. the DNS record types) doesn't hide the others. The commands are always shown
    ///     completely.
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

    /// <summary>
    ///     Maximum number of commands suggested for the clipboard content (empty input).
    /// </summary>
    private const int MaxClipboardSuggestions = 3;

    /// <summary>
    ///     Number of suggestions that PageUp and PageDown move the selection.
    /// </summary>
    private const int SuggestionsPerPage = 5;

    /// <summary>
    ///     Maximum length of the clipboard content that is suggested as target.
    /// </summary>
    private const int MaxClipboardLength = 255;

    /// <summary>
    ///     Time without typing after which the input is validated.
    /// </summary>
    private static readonly TimeSpan ValidationDelay = TimeSpan.FromMilliseconds(300);

    /// <summary>
    ///     Examples shown with an empty input if there are no recent commands yet (only of available commands).
    /// </summary>
    private static readonly string[] ExampleCommands =
    [
        "ping 10.0.0.1",
        "trace borntoberoot.net",
        "dns borntoberoot.net 1.1.1.1 aaaa",
        "subnet 10.0.0.0/16 /24",
        "lookup port 22; 443"
    ];

    private readonly IReadOnlyList<RunCommandDefinition> _commands;
    private readonly Action<RunCommandViewModel> _runHandler;

    /// <summary>
    ///     Recent commands (parsed once, the history only changes while the dialog is open if a command is run and the
    ///     dialog is kept open, or an entry is removed).
    /// </summary>
    private readonly List<HistoryEntry> _history;

    /// <summary>
    ///     Names (command and aliases) and translated name per command.
    /// </summary>
    private readonly List<(RunCommandDefinition Command, List<string> Names, string TranslatedName)> _commandNames;

    /// <summary>
    ///     Profiles per command (loaded once per command while the dialog is open).
    /// </summary>
    private readonly Dictionary<RunCommandDefinition, List<ProfileInfo>> _profiles = [];

    /// <summary>
    ///     Content of the clipboard if it is a target (e.g. an IP address) and the setting is enabled, otherwise null.
    /// </summary>
    private readonly string _clipboardTarget;

    /// <summary>
    ///     Commands for the clipboard content (validated once, the clipboard is only read when the dialog is opened).
    /// </summary>
    private readonly List<(RunCommandDefinition Command, string Text)> _clipboardCommands = [];

    private readonly RunCommandTargetKind _clipboardKind;

    private readonly DispatcherTimer _validationTimer;

    private RunCommandParseResult _result;

    /// <summary>
    ///     Command that Enter runs for a target typed without a command (e.g. "ping 10.0.0.1" for "10.0.0.1"): the first
    ///     command for the target, also if it is shown as a recent command. Null if there is none.
    /// </summary>
    private string _targetOnlyPreselection;

    /// <summary>
    ///     Recent command with the parsed values used for the suggestions.
    /// </summary>
    private sealed record HistoryEntry(
        string Text,
        RunCommandDefinition Command,
        string ProfileName,
        RunCommandTargetKind TargetKind);

    /// <summary>
    ///     Initializes a new instance of the <see cref="RunCommandViewModel" /> class.
    /// </summary>
    /// <param name="runHandler">
    ///     Called with the request to run (<see cref="Request" />, <see cref="IsSettings" />, <see cref="KeepOpen" />).
    /// </param>
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
            .. SettingsManager.Current.RunCommand_History
                // The setting may have been lowered (the history is trimmed with the next run).
                .Take(SettingsManager.Current.RunCommand_HistoryEntries)
                .Select(x => (Text: x, Result: RunCommandParser.Parse(x, _commands)))
                .Where(x => x.Result.Command != null)
                .Select(x => CreateHistoryEntry(x.Text, x.Result))
        ];

        if (SettingsManager.Current.RunCommand_SuggestClipboard)
            _clipboardTarget = GetClipboardTarget();

        if (_clipboardTarget != null)
        {
            _clipboardKind = RunCommandTarget.GetKind(_clipboardTarget);
            _clipboardCommands = [.. RunCommandManager.GetCommandsForTarget(_clipboardTarget, _commands, _clipboardKind)];
        }

        SuggestionsView = CollectionViewSource.GetDefaultView(Suggestions);
        SuggestionsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(RunCommandSuggestion.Group)));

        _validationTimer = new DispatcherTimer { Interval = ValidationDelay };
        _validationTimer.Tick += (_, _) =>
        {
            _validationTimer.Stop();
            Validate();
        };

        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        UpdateSuggestions();
    }

    #region Properties

    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Placeholder of the input, a random line each time the dialog is opened (e.g. "Ping it, trace it, scan
    ///     it...").
    /// </summary>
    public string Watermark { get; } = GetRandomWatermark();

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

            UpdateSuggestions();
        }
    } = string.Empty;

    /// <summary>
    ///     Suggestions, replaced completely on every input (one notification for the grouped view).
    /// </summary>
    public ObservableRangeCollection<RunCommandSuggestion> Suggestions { get; } = [];

    /// <summary>
    ///     Suggestions grouped by <see cref="RunCommandSuggestion.Group" /> (e.g. "Recent", "Profiles").
    /// </summary>
    public ICollectionView SuggestionsView { get; }

    public RunCommandSuggestion SelectedSuggestion
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRemoveSelectedSuggestion));

            // The validation shows what Enter runs (the selected suggestion or the input).
            RestartValidation();
        }
    }

    /// <summary>
    ///     The selected suggestion is a recent command, which can be removed from the history (Shift+Del).
    /// </summary>
    public bool CanRemoveSelectedSuggestion => SelectedSuggestion?.Kind == RunCommandSuggestionKind.History;

    /// <summary>
    ///     A command is recognized (its icon is shown in front of the input).
    /// </summary>
    public bool HasCommand
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
    ///     Application of the recognized command (<see cref="ApplicationName.None" /> for the settings).
    /// </summary>
    public ApplicationName CommandApplication
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
    ///     Syntax of the recognized command (e.g. "dnslookup &lt;hosts&gt; [server] [type] | @&lt;profile&gt;"),
    ///     null if no command is recognized.
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
    ///     Parts of the <see cref="Hint" />, the part that is being typed is highlighted.
    /// </summary>
    public IReadOnlyList<RunCommandHintPart> HintParts
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    /// <summary>
    ///     Values of the input as they were understood (e.g. hosts, DNS server, type).
    /// </summary>
    public IReadOnlyList<RunCommandChip> Chips
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasChips));
        }
    } = [];

    public bool HasChips => Chips.Count > 0;

    /// <summary>
    ///     Show the key hints at the bottom of the dialog (setting).
    /// </summary>
    public bool ShowKeyHints { get; } = SettingsManager.Current.RunCommand_ShowKeyHints;

    /// <summary>
    ///     What Enter runs is valid (checked while typing).
    /// </summary>
    public bool IsValid
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
    ///     What Enter runs is not valid (checked while typing), see <see cref="ValidationMessage" />.
    /// </summary>
    public bool IsInvalid
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
    ///     Error of the validation while typing (tooltip of the warning icon), null if there is none.
    /// </summary>
    public string ValidationMessage
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

    /// <summary>
    ///     The dialog stays open after the run (set before the run handler is called), see
    ///     <see cref="RunCommandRunMode.RunAndKeepOpen" />.
    /// </summary>
    public bool KeepOpen { get; private set; }

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

    /// <summary>
    ///     Selects the suggestion one page below (PageDown).
    /// </summary>
    public void SelectNextPage()
    {
        Select(SuggestionsPerPage);
    }

    /// <summary>
    ///     Selects the suggestion one page above (PageUp).
    /// </summary>
    public void SelectPreviousPage()
    {
        Select(-SuggestionsPerPage);
    }

    private void Select(int offset)
    {
        if (Suggestions.Count == 0)
            return;

        var index = SelectedSuggestion == null ? -1 : Suggestions.IndexOf(SelectedSuggestion);

        // Nothing selected: Down and PageDown select the first. Up from the first suggestion selects none, so Enter runs
        // the input as typed. A page up stops at the first.
        SelectedSuggestion = (index < 0 && offset > 0 ? 0 : index + offset) switch
        {
            < 0 when index > 0 && offset < -1 => Suggestions[0],
            < 0 => null,
            var newIndex => Suggestions[Math.Min(newIndex, Suggestions.Count - 1)]
        };
    }

    /// <summary>
    ///     Inserts the selected suggestion into the input (Tab).
    /// </summary>
    public void Complete()
    {
        // Nothing selected: the first value of the part that is being typed, in the order of the syntax, e.g. a profile
        // ("rdp @ser" → the first matching profile) or a DNS server, port profile or type ("dns host c" → the DNS
        // server "Cloudflare", then the types).
        var suggestion = SelectedSuggestion ?? (_result.Position switch
        {
            RunCommandPosition.Target when _result.ProfileName is { Length: > 0 } =>
                Suggestions.FirstOrDefault(x => x.Kind == RunCommandSuggestionKind.Profile),
            RunCommandPosition.Argument when _result.CurrentWord.Length > 0 =>
                Suggestions.FirstOrDefault(x => x.Kind is RunCommandSuggestionKind.DNSServer
                    or RunCommandSuggestionKind.PortProfile or RunCommandSuggestionKind.Type),
            _ => null
        });

        if (suggestion == null)
            return;

        Input = suggestion.CompletionText;

        // Keep the completed profile selected, so Enter runs it (also if several profiles have the same name).
        if (suggestion.Profile != null)
            SelectedSuggestion = Suggestions.FirstOrDefault(x => x.Profile == suggestion.Profile) ?? SelectedSuggestion;
    }

    /// <summary>
    ///     Runs the selected suggestion, or the input if none is selected.
    /// </summary>
    /// <param name="mode">Run (Enter), run and keep the dialog open (Ctrl+Enter) or only fill in (Shift+Enter).</param>
    /// <returns>False if the input is not valid (the error is shown with the warning icon).</returns>
    public bool Run(RunCommandRunMode mode = RunCommandRunMode.Run)
    {
        var suggestion = SelectedSuggestion;

        var text = suggestion?.CompletionText ?? Input;
        var result = RunCommandParser.Parse(text, _commands);

        StopValidation();
        KeepOpen = false;

        var error = RunCommandManager.TryCreateRequest(result, suggestion?.Profile, out var request);

        if (error != null)
        {
            // Show the text of a failed suggestion (e.g. a recent command with an ambiguous profile name), so it can be
            // corrected or completed with the suggestions.
            if (suggestion != null)
                Input = text;

            ShowRunError(error);

            return false;
        }

        // Only fill in the input, so the options can be adjusted before starting (e.g. the scan).
        if (mode == RunCommandRunMode.FillIn && request != null)
            request = request with { Start = false };

        Request = request;
        IsSettings = result.Command.Type == RunCommandType.Setting;

        // A dialog that is opened (e.g. the connect dialog of a remote tool) needs the dialog to be closed.
        KeepOpen = mode == RunCommandRunMode.RunAndKeepOpen &&
                   !RunCommandManager.OpensDialog(IsSettings ? null : request);

        // Commands are case-insensitive, so "Ping 10.0.0.1" and "ping 10.0.0.1" are the same entry. Not if the text can't
        // be run again (e.g. a profile selected in the suggestions, whose name starts with a quote).
        var historyText = text.Trim();

        if (result.Error == RunCommandParseError.None)
            ListHelper.Modify(SettingsManager.Current.RunCommand_History, historyText,
                SettingsManager.Current.RunCommand_HistoryEntries, StringComparer.OrdinalIgnoreCase);

        _runHandler(this);

        if (!KeepOpen)
            return true;

        // Show the command in the recent commands and keep it in the input (e.g. "ping "), so it can be run with the
        // next target.
        if (result.Error == RunCommandParseError.None && SettingsManager.Current.RunCommand_HistoryEntries > 0)
        {
            _history.RemoveAll(x => string.Equals(x.Text, historyText, StringComparison.OrdinalIgnoreCase));
            _history.Insert(0, CreateHistoryEntry(historyText, result));

            // Like the stored history
            if (_history.Count > SettingsManager.Current.RunCommand_HistoryEntries)
                _history.RemoveRange(SettingsManager.Current.RunCommand_HistoryEntries,
                    _history.Count - SettingsManager.Current.RunCommand_HistoryEntries);
        }

        var commandPrefix = result.Command.TypeBeforeTarget && result.Type != null
            ? $"{result.CommandText} {result.Type} "
            : $"{result.CommandText} ";

        if (Input == commandPrefix)
            UpdateSuggestions();
        else
            Input = commandPrefix;

        return true;
    }

    /// <summary>
    ///     Removes a recent command from the history (Shift+Del on the selected one, or its remove button).
    /// </summary>
    /// <param name="suggestion">Suggestion of the recent command.</param>
    /// <returns>True if a recent command was removed.</returns>
    public bool RemoveSuggestion(RunCommandSuggestion suggestion)
    {
        if (suggestion?.Kind != RunCommandSuggestionKind.History)
            return false;

        var text = suggestion.Title;
        var index = Suggestions.IndexOf(suggestion);
        var wasSelected = suggestion == SelectedSuggestion;

        var entry = SettingsManager.Current.RunCommand_History.FirstOrDefault(x =>
            string.Equals(x, text, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
            SettingsManager.Current.RunCommand_History.Remove(entry);

        _history.RemoveAll(x => string.Equals(x.Text, text, StringComparison.OrdinalIgnoreCase));

        UpdateSuggestions();

        // Keep the position in the list, so several entries can be removed one after the other.
        if (wasSelected && Suggestions.Count > 0)
            SelectedSuggestion = Suggestions[Math.Min(index, Suggestions.Count - 1)];

        return true;
    }

    private static string GetRandomWatermark()
    {
        // Created on each call, since the lines are translated.
        string[] watermarks =
        [
            Strings.RunCommandWatermark1, Strings.RunCommandWatermark2, Strings.RunCommandWatermark3,
            Strings.RunCommandWatermark4, Strings.RunCommandWatermark5, Strings.RunCommandWatermark6,
            Strings.RunCommandWatermark7, Strings.RunCommandWatermark8, Strings.RunCommandWatermark9,
            Strings.RunCommandWatermark10, Strings.RunCommandWatermark11, Strings.RunCommandWatermark12,
            Strings.RunCommandWatermark13, Strings.RunCommandWatermark14, Strings.RunCommandWatermark15,
            Strings.RunCommandWatermark16, Strings.RunCommandWatermark17, Strings.RunCommandWatermark18,
            Strings.RunCommandWatermark19, Strings.RunCommandWatermark20, Strings.RunCommandWatermark21,
            Strings.RunCommandWatermark22, Strings.RunCommandWatermark23, Strings.RunCommandWatermark24,
            Strings.RunCommandWatermark25, Strings.RunCommandWatermark26, Strings.RunCommandWatermark27,
            Strings.RunCommandWatermark28, Strings.RunCommandWatermark29, Strings.RunCommandWatermark30,
            Strings.RunCommandWatermark31, Strings.RunCommandWatermark32, Strings.RunCommandWatermark33,
            Strings.RunCommandWatermark34, Strings.RunCommandWatermark35
        ];

        return watermarks[Random.Shared.Next(watermarks.Length)];
    }

    private static HistoryEntry CreateHistoryEntry(string text, RunCommandParseResult result)
    {
        return new HistoryEntry(text, result.Command, result.ProfileName, RunCommandTarget.GetKind(result.Target));
    }

    /// <summary>
    ///     Gets the clipboard content if it is a target (e.g. an IP address, a subnet or a MAC address).
    /// </summary>
    /// <returns>Target, or null if the clipboard contains no target.</returns>
    private static string GetClipboardTarget()
    {
        try
        {
            if (!Clipboard.ContainsText())
                return null;

            // Whitespace around ";" is removed like in the input (e.g. "10.0.0.1; 10.0.0.2").
            var text = Regex.Replace(Clipboard.GetText().Trim(), @"\s*;\s*", ";");

            // A single word (host name without a domain) is not suggested, since it can be any copied text.
            return text.Length <= MaxClipboardLength &&
                   RunCommandTarget.GetKind(text) is not (RunCommandTargetKind.None or RunCommandTargetKind.Hostname)
                ? text
                : null;
        }
        catch (ExternalException)
        {
            // The clipboard is used by another application.
            return null;
        }
    }

    /// <summary>
    ///     Updates the suggestions, the hint and the parsed values for the input.
    /// </summary>
    private void UpdateSuggestions()
    {
        _result = RunCommandParser.Parse(Input, _commands);

        HasCommand = _result.Command != null;
        CommandApplication = _result.Command?.Application ?? ApplicationName.None;

        UpdateHint();
        UpdateChips();

        IEnumerable<RunCommandSuggestion> suggestions = _result.Position switch
        {
            RunCommandPosition.Command when string.IsNullOrWhiteSpace(Input) => GetEmptyInputSuggestions(),
            RunCommandPosition.Command => GetFirstWordSuggestions(),
            RunCommandPosition.Type =>
            [
                .. GetTypeSuggestions().Take(MaxSuggestions),
                .. GetHistorySuggestions().Take(MaxSuggestions)
            ],
            RunCommandPosition.Target when _result.ProfileName != null => GetProfileSuggestions().Take(MaxSuggestions),
            RunCommandPosition.Target =>
            [
                .. GetHistorySuggestions().Take(MaxCommandHistorySuggestions),
                .. GetProfileSuggestions().Take(MaxSuggestions)
            ],
            // In the order of the syntax (e.g. "[DNS server] [Type]")
            _ =>
            [
                .. GetDNSServerSuggestions().Take(MaxSuggestions), .. GetPortProfileSuggestions().Take(MaxSuggestions),
                .. GetTypeSuggestions().Take(MaxSuggestions), .. GetHistorySuggestions().Take(MaxSuggestions)
            ]
        };

        Suggestions.ReplaceAll(suggestions);

        SelectedSuggestion = GetPreselectedSuggestion();

        RestartValidation();
    }

    /// <summary>
    ///     Updates the hint with the syntax of the command and highlights the part that is being typed.
    /// </summary>
    private void UpdateHint()
    {
        var command = _result.Command;

        if (command == null)
        {
            Hint = null;
            HintParts = [];

            return;
        }

        Hint = $"{command.Command} {command.Syntax}".TrimEnd();

        var currentPart = _result.CurrentSyntaxPart;

        var parts = new List<RunCommandHintPart>
        {
            new(command.Command, _result.Position == RunCommandPosition.Command)
        };

        foreach (var part in command.SyntaxParts)
        {
            if (part.Kind == RunCommandSyntaxPartKind.Profile)
                parts.Add(new RunCommandHintPart("|", false));

            parts.Add(new RunCommandHintPart(part.Text, part.Kind == currentPart));
        }

        HintParts = parts;
    }

    /// <summary>
    ///     Updates the values of the input as they were understood, in the order of the syntax. The application is not
    ///     included, since its icon is shown in the input and its command in the hint.
    /// </summary>
    private void UpdateChips()
    {
        var command = _result.Command;

        if (command == null || _result.Position == RunCommandPosition.Command ||
            _result.Target == null && string.IsNullOrEmpty(_result.ProfileName) && _result.Type == null)
        {
            Chips = [];

            return;
        }

        var chips = new List<RunCommandChip>();

        foreach (var part in command.SyntaxParts)
        {
            var value = part.Kind switch
            {
                RunCommandSyntaxPartKind.Target => _result.Target,
                RunCommandSyntaxPartKind.Argument => _result.Argument,
                RunCommandSyntaxPartKind.Type => _result.Type?.ToUpperInvariant(),
                _ => string.IsNullOrEmpty(_result.ProfileName) ? null : "@" + _result.ProfileName
            };

            if (value != null)
                chips.Add(new RunCommandChip(part.Name, value));
        }

        Chips = chips;
    }

    /// <summary>
    ///     Shows the error of a run with the warning icon, like the validation while typing (without waiting).
    /// </summary>
    private void ShowRunError(string error)
    {
        // A failed suggestion was put into the input, which restarted the validation.
        _validationTimer.Stop();

        IsValid = false;
        IsInvalid = true;
        ValidationMessage = error;
    }

    /// <summary>
    ///     Validates what Enter runs (the selected suggestion or the input) after a short time without typing.
    /// </summary>
    private void RestartValidation()
    {
        StopValidation();

        if (ShouldValidate())
            _validationTimer.Start();
    }

    private void StopValidation()
    {
        _validationTimer.Stop();

        IsValid = false;
        IsInvalid = false;
        ValidationMessage = null;
    }

    /// <summary>
    ///     The input is validated after the command (or for an unknown command with arguments like "pin 10.0.0.1"), not
    ///     while the command is being typed.
    /// </summary>
    private bool ShouldValidate()
    {
        return !string.IsNullOrWhiteSpace(Input) &&
               (_result.Position != RunCommandPosition.Command || _result.Command == null && HasArguments());
    }

    private void Validate()
    {
        if (!ShouldValidate())
            return;

        var suggestion = SelectedSuggestion;
        var result = suggestion == null ? _result : RunCommandParser.Parse(suggestion.CompletionText, _commands);

        var error = RunCommandManager.TryCreateRequest(result, suggestion?.Profile, out _);

        IsValid = error == null;
        IsInvalid = error != null;
        ValidationMessage = error;
    }

    /// <summary>
    ///     Gets the suggestion that Enter runs without selecting one. Without a selected suggestion, Enter runs the input
    ///     as typed:
    ///     - Empty input: the first command for the clipboard content (if enabled), otherwise none.
    ///     - After the command: none (profiles, recent commands, types and DNS servers are selected with the arrow keys).
    ///     - While typing the command: the best matching command (or the first command for a target without a command,
    ///     e.g. "10.0.0.1", but not for a host name without a domain, which can also be a typo), but not if an unknown
    ///     command has arguments (e.g. a typo like "pin 10.0.0.1"), which would drop the input.
    ///     - While typing a profile ("@"): the profile, only if exactly one matches. Otherwise, the typed name is used,
    ///     which must be unique (e.g. two profiles with the same name in different groups).
    ///     - While typing a type before the target (e.g. "lookup po"): the matching type, like the command.
    /// </summary>
    private RunCommandSuggestion GetPreselectedSuggestion()
    {
        return _result.Position switch
        {
            RunCommandPosition.Command when string.IsNullOrWhiteSpace(Input) =>
                Suggestions.FirstOrDefault(x => x.Kind == RunCommandSuggestionKind.Clipboard),
            RunCommandPosition.Command when _result.Command == null && HasArguments() => null,
            RunCommandPosition.Target when _result.ProfileName != null => Suggestions.Count == 1 ? Suggestions[0] : null,
            RunCommandPosition.Type when _result.CurrentWord.Length > 0 =>
                Suggestions.FirstOrDefault(x => x.Kind == RunCommandSuggestionKind.Type),
            // The best matching command, otherwise the first command for a typed target (also if it is shown as recent
            // command). Other clipboard and recent commands are selected with the arrow keys.
            RunCommandPosition.Command =>
                Suggestions.FirstOrDefault(x => x.Kind == RunCommandSuggestionKind.Command) ??
                (_targetOnlyPreselection == null
                    ? null
                    : Suggestions.FirstOrDefault(x =>
                        string.Equals(x.CompletionText, _targetOnlyPreselection, StringComparison.OrdinalIgnoreCase))),
            _ => null
        };
    }

    /// <summary>
    ///     Checks if the input has arguments after the first word (e.g. "pin 10.0.0.1"). Like in the parser, whitespace
    ///     around ";" doesn't separate arguments, so "10.0.0.1; 10.0.0.2" is one word.
    /// </summary>
    private bool HasArguments()
    {
        return Regex.Replace(Input.Trim(), @"\s*;\s*", ";").Any(char.IsWhiteSpace);
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
    ///     Suggestions for an empty input: commands for the clipboard content, recent commands, examples (if there are
    ///     no recent commands yet) and all commands.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetEmptyInputSuggestions()
    {
        var suggestions = new List<RunCommandSuggestion>();

        if (_clipboardTarget != null)
            suggestions.AddRange(GetTargetSuggestions(_clipboardTarget, Strings.Clipboard,
                RunCommandSuggestionKind.Clipboard, commands: _clipboardCommands, targetKind: _clipboardKind)
                .Take(MaxClipboardSuggestions));

        suggestions.AddRange(GetHistorySuggestions().Take(MaxHistorySuggestions));

        if (_history.Count == 0)
            suggestions.AddRange(GetExampleSuggestions());

        suggestions.AddRange(GetCommandSuggestions());

        return Distinct(suggestions);
    }

    /// <summary>
    ///     Suggestions while typing the first word: the matching commands, the recent commands that contain the word
    ///     (e.g. a host), the commands for a target typed without a command (e.g. "10.0.0.1") and the commands for the
    ///     clipboard content that start with the word (e.g. "pi" → "ping 10.0.0.1").
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetFirstWordSuggestions()
    {
        var word = _result.CurrentWord;

        // Detected once per input (also used for the ranking and the commands of the target)
        var targetKind = RunCommandTarget.GetKind(_result.CommandText);

        var targetOnlySuggestions = GetTargetOnlySuggestions(CommandStartsWith(word), targetKind).ToList();

        // Not for a host name without a domain, which can also be a mistyped command (e.g. "pnig"), and not for a port,
        // which can also be the start of an IP address (e.g. "192").
        _targetOnlyPreselection = targetKind is RunCommandTargetKind.Hostname or RunCommandTargetKind.Ports
            ? null
            : targetOnlySuggestions.FirstOrDefault()?.CompletionText;

        var suggestions = new List<RunCommandSuggestion>(GetCommandSuggestions());

        suggestions.AddRange(GetFirstWordHistorySuggestions().Take(MaxHistorySuggestions));
        suggestions.AddRange(targetOnlySuggestions);

        if (_clipboardTarget != null)
            suggestions.AddRange(GetTargetSuggestions(_clipboardTarget, Strings.Clipboard,
                RunCommandSuggestionKind.Clipboard, word, _clipboardCommands, _clipboardKind));

        // A command for the target that was already run is shown as recent command (only once).
        return Distinct(suggestions);
    }

    /// <summary>
    ///     Removes suggestions with the same command as a previous one (e.g. a recent command that is also suggested
    ///     for the target or the clipboard). Not trimmed, so a command that is completed (e.g. "ping ") and a recent
    ///     command without a target (e.g. "ping") are both kept.
    /// </summary>
    private static IEnumerable<RunCommandSuggestion> Distinct(IEnumerable<RunCommandSuggestion> suggestions)
    {
        return suggestions.DistinctBy(x => x.CompletionText, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Checks if a command or alias starts with the text (e.g. "pi" for "ping").
    /// </summary>
    private bool CommandStartsWith(string text)
    {
        return _commandNames.Any(x => x.Names.Any(name => name.StartsWith(text, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    ///     Commands that match the typed command.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetCommandSuggestions()
    {
        var text = _result.CurrentWord;

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
                CompletionText = (name ?? command.Command) + " " + arguments,
                Highlight = text,
                Group = Strings.Applications
            }));
        }

        return commands.OrderBy(x => x.Rank).Select(x => x.Suggestion);
    }

    /// <summary>
    ///     Commands for a target typed without a command (e.g. "ping 10.0.0.1", "trace 10.0.0.1" for "10.0.0.1").
    /// </summary>
    /// <param name="commandStartsWithWord">A command starts with the typed word.</param>
    /// <param name="targetKind">Kind of the typed word as target.</param>
    private IEnumerable<RunCommandSuggestion> GetTargetOnlySuggestions(bool commandStartsWithWord,
        RunCommandTargetKind targetKind)
    {
        if (_result.Command != null || HasArguments())
            return [];

        // A host name without a domain can also be a command that is being typed (e.g. "pi").
        if (commandStartsWithWord && targetKind == RunCommandTargetKind.Hostname)
            return [];

        return GetTargetSuggestions(_result.CommandText, Strings.Commands, RunCommandSuggestionKind.Target,
            targetKind: targetKind);
    }

    /// <summary>
    ///     Commands that accept the target. Commands that were used with this kind of target (e.g. IP addresses) come
    ///     first, the most used first.
    /// </summary>
    /// <param name="target">Target (e.g. "10.0.0.1").</param>
    /// <param name="group">Group of the suggestions.</param>
    /// <param name="suggestionKind">Kind of the suggestions (typed target or clipboard).</param>
    /// <param name="commandPrefix">Only commands of which a name starts with it (e.g. "pi" for "ping"), or null.</param>
    /// <param name="commands">Commands for the target if they are already known (clipboard), otherwise null.</param>
    /// <param name="targetKind">Kind of the target if it is already known, otherwise it is detected.</param>
    private IEnumerable<RunCommandSuggestion> GetTargetSuggestions(string target, string group,
        RunCommandSuggestionKind suggestionKind, string commandPrefix = null,
        IEnumerable<(RunCommandDefinition Command, string Text)> commands = null, RunCommandTargetKind? targetKind = null)
    {
        var kind = targetKind ?? RunCommandTarget.GetKind(target);

        var usage = _history.Where(x => x.TargetKind == kind)
            .GroupBy(x => x.Command)
            .ToDictionary(x => x.Key, x => x.Count());

        return (commands ?? RunCommandManager.GetCommandsForTarget(target, _commands, kind))
            .Where(x => commandPrefix == null || _commandNames.Any(c =>
                c.Command == x.Command &&
                c.Names.Any(name => name.StartsWith(commandPrefix, StringComparison.OrdinalIgnoreCase))))
            .OrderByDescending(x => usage.GetValueOrDefault(x.Command))
            .Select(x => new RunCommandSuggestion
            {
                Kind = suggestionKind,
                Application = x.Command.Application,
                Title = x.Text,
                Description = RunCommandManager.GetTranslatedName(x.Command),
                CompletionText = x.Text,
                Group = group
            });
    }

    /// <summary>
    ///     Example commands of the available commands.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetExampleSuggestions()
    {
        return ExampleCommands
            .Select(x => (Text: x, RunCommandParser.Parse(x, _commands).Command))
            .Where(x => x.Command != null)
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Example,
                Application = x.Command.Application,
                Title = x.Text,
                Description = RunCommandManager.GetTranslatedName(x.Command),
                CompletionText = x.Text,
                Group = Strings.Examples
            });
    }

    /// <summary>
    ///     Profiles of the command that match the typed profile name (or target). Recently run profiles of the command
    ///     come first within the same rank, since the recent commands are not shown while typing a profile ("@").
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

        // Position of the profile name in the history of the command (most recent first)
        var recentProfileNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in _history.Where(x => x.Command == _result.Command && !string.IsNullOrEmpty(x.ProfileName)))
            recentProfileNames.TryAdd(entry.ProfileName, recentProfileNames.Count);

        // The name is compared like it is parsed (without surrounding whitespace and quotes).
        int GetRecentIndex(ProfileInfo profile) =>
            recentProfileNames.GetValueOrDefault(profile.Name.Trim().Trim('"').Trim(), int.MaxValue);

        return profiles
            .Select(x => (Profile: x, Target: x.GetTarget(application)))
            .Select(x => (x.Profile, x.Target, Rank: GetRank(x.Profile, x.Target, text)))
            .Where(x => x.Rank != null)
            .OrderBy(x => x.Rank)
            .ThenBy(x => GetRecentIndex(x.Profile))
            .Select(x => new RunCommandSuggestion
            {
                Kind = RunCommandSuggestionKind.Profile,
                Application = application,
                Title = x.Profile.Name,
                // The target of the application (e.g. the RDP host), which can differ from the general host.
                Description = string.IsNullOrEmpty(x.Target) ? x.Profile.Group : $"{x.Profile.Group} · {x.Target}",
                Info = "@" + x.Profile.Name,
                CompletionText = _result.InputBeforeCurrentWord + "@" + x.Profile.Name,
                Profile = x.Profile,
                Highlight = text,
                Group = Strings.Profiles
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
                CompletionText = _result.InputBeforeCurrentWord + x + " ",
                Highlight = _result.CurrentWord,
                Group = Strings.Type
            });
    }

    /// <summary>
    ///     Port profiles of the Port Scanner (e.g. "Webserver") that contain the typed word, if no ports were entered
    ///     yet. The ports can be a list of ports and port profiles (e.g. "webserver;database;22"), the last entry is
    ///     completed.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetPortProfileSuggestions()
    {
        return GetArgumentValueSuggestions(RunCommandArgument.Ports,
            RunCommandManager.GetPortProfiles().Select(x => (x.Name, x.Ports)), RunCommandSuggestionKind.PortProfile,
            Strings.PortProfiles, isList: true);
    }

    /// <summary>
    ///     DNS servers configured in the DNS lookup (e.g. "Cloudflare") that contain the typed word, if no DNS server was
    ///     entered yet.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetDNSServerSuggestions()
    {
        return GetArgumentValueSuggestions(RunCommandArgument.DNSServer,
            RunCommandManager.GetDNSServers().Select(x => (x, Strings.DNSServer)), RunCommandSuggestionKind.DNSServer,
            Strings.DNSServers);
    }

    /// <summary>
    ///     Named values of the second argument of the command (e.g. DNS servers, port profiles) that contain the typed
    ///     word, if the argument was not entered yet. Names with spaces are inserted in quotes.
    /// </summary>
    /// <param name="argument">Argument the values are for.</param>
    /// <param name="values">Names and descriptions of the values.</param>
    /// <param name="kind">Kind of the suggestions.</param>
    /// <param name="group">Group of the suggestions.</param>
    /// <param name="isList">
    ///     The argument can be a list separated by ";" (e.g. "webserver;database;22"): the entry after the last ";" is
    ///     completed, the entries before are kept as entered (with their quotes, e.g. <c>"DNS (via TCP)";web</c>).
    /// </param>
    private IEnumerable<RunCommandSuggestion> GetArgumentValueSuggestions(RunCommandArgument argument,
        IEnumerable<(string Name, string Description)> values, RunCommandSuggestionKind kind, string group,
        bool isList = false)
    {
        if (_result.Command.Argument != argument ||
            _result.Argument != null && !string.Equals(_result.Argument, _result.CurrentWord))
            return [];

        // Entries before the one that is being typed, as entered (e.g. "webserver;" of "webserver;dat")
        var separator = isList ? _result.CurrentToken.LastIndexOf(';') : -1;
        var previousEntries = _result.CurrentToken[..(separator + 1)];

        // Typed in the open quote of the previous entry (e.g. "DNS (via TCP); web) - close it before the ";"
        if (previousEntries.Count(x => x == '"') % 2 == 1)
            previousEntries = previousEntries[..^1].TrimEnd() + "\";";

        var entry = separator < 0
            ? _result.CurrentWord
            : _result.CurrentToken[(separator + 1)..].Replace("\"", string.Empty).Trim();

        return values
            .Where(x => x.Name.Contains(entry, StringComparison.OrdinalIgnoreCase))
            .Select(x => new RunCommandSuggestion
            {
                Kind = kind,
                Application = _result.Command.Application,
                Title = x.Name,
                Description = x.Description,
                CompletionText = _result.InputBeforeCurrentWord + previousEntries +
                                 (x.Name.Contains(' ') ? $"\"{x.Name}\"" : x.Name) + " ",
                Highlight = entry,
                Group = group
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
            .Select(CreateHistorySuggestion);
    }

    /// <summary>
    ///     Recent commands that contain the first word that is being typed (e.g. "server01" or "pi"), of any command.
    /// </summary>
    private IEnumerable<RunCommandSuggestion> GetFirstWordHistorySuggestions()
    {
        var input = Input.Trim();

        return _history
            .Where(x => x.Text.Contains(_result.CurrentWord, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(x.Text, input, StringComparison.OrdinalIgnoreCase))
            .Select(CreateHistorySuggestion);
    }

    private RunCommandSuggestion CreateHistorySuggestion(HistoryEntry entry)
    {
        return new RunCommandSuggestion
        {
            Kind = RunCommandSuggestionKind.History,
            Application = entry.Command.Application,
            Title = entry.Text,
            Description = RunCommandManager.GetTranslatedName(entry.Command),
            CompletionText = entry.Text,
            Highlight = _result.CurrentWord,
            Group = Strings.Recent
        };
    }

    #endregion
}
