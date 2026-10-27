using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

/// <summary>
///     Tab completion for an editable <see cref="ComboBox" /> with a list separated by ";" (e.g. the ports of the Port
///     Scanner with port profile names): <c>Tab</c> completes the last entry with a name of <see cref="ItemsProperty" />
///     (e.g. "webserver; dat" -> "webserver; Database"), the entries before are kept. <c>Tab</c> again shows the next
///     name that matches the typed entry, after the last one it moves the focus. If there is nothing to complete (empty
///     entry, no match, already the name), <c>Tab</c> moves the focus as usual. See
///     <see cref="ListEntryCompletionHelper" />.
/// </summary>
public static class ComboBoxListCompletionBehavior
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.RegisterAttached(
            "Items",
            typeof(IEnumerable<string>),
            typeof(ComboBoxListCompletionBehavior),
            new PropertyMetadata(null, OnItemsChanged));

    public static void SetItems(UIElement element, IEnumerable<string> value) =>
        element.SetValue(ItemsProperty, value);

    public static IEnumerable<string> GetItems(UIElement element) =>
        (IEnumerable<string>)element.GetValue(ItemsProperty);

    // Completion that is shown, to show the next name with the next Tab
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "State",
            typeof(CompletionState),
            typeof(ComboBoxListCompletionBehavior),
            new PropertyMetadata(null));

    private sealed record CompletionState(string Prefix, List<string> Candidates, int Index, string Text);

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
            return;

        comboBox.PreviewKeyDown -= ComboBox_PreviewKeyDown;

        if (e.NewValue != null)
            comboBox.PreviewKeyDown += ComboBox_PreviewKeyDown;
    }

    private static void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || Keyboard.Modifiers != ModifierKeys.None ||
            sender is not ComboBox { IsEditable: true, IsReadOnly: false } comboBox)
            return;

        var textBox = comboBox.Template?.FindName("PART_EditableTextBox", comboBox) as TextBox;
        var text = comboBox.Text ?? string.Empty;

        // Tab again: the next name for the same entry, after the last one Tab moves the focus
        if (comboBox.GetValue(StateProperty) is CompletionState state && state.Text == text)
        {
            if (state.Index + 1 >= state.Candidates.Count)
            {
                comboBox.ClearValue(StateProperty);
                return;
            }

            Complete(comboBox, textBox, state with { Index = state.Index + 1 }, e);
            return;
        }

        // The text typed by the user, without a rest that the ComboBox completed and selected (e.g. "Webs[erver]")
        if (textBox is { SelectionLength: > 0 } &&
            textBox.SelectionStart + textBox.SelectionLength == text.Length)
            text = text[..textBox.SelectionStart];

        var (prefix, entry) = ListEntryCompletionHelper.SplitLastEntry(text);
        var candidates = ListEntryCompletionHelper.GetCandidates(entry, GetItems(comboBox) ?? []);

        // Nothing to complete: Tab moves the focus
        if (candidates.Count == 0)
            return;

        // The entry is already the first name: start with the next one (if there is none, Tab moves the focus)
        var index = string.Equals(candidates[0], entry.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        if (index >= candidates.Count)
            return;

        Complete(comboBox, textBox, new CompletionState(prefix, candidates, index, null), e);
    }

    private static void Complete(ComboBox comboBox, TextBox textBox, CompletionState state, KeyEventArgs e)
    {
        var text = state.Prefix + state.Candidates[state.Index];

        comboBox.Text = text;
        comboBox.SetValue(StateProperty, state with { Text = text });

        // Caret at the end, nothing selected - also after the ComboBox updated its text box
        if (textBox != null)
        {
            textBox.Select(text.Length, 0);
            textBox.Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new Action(() => textBox.Select(textBox.Text.Length, 0)));
        }

        e.Handled = true;
    }
}
