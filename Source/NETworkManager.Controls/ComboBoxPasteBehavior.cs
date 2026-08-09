using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Controls;

/// <summary>
/// Attached property that converts multiline text (e.g. a column pasted from Excel) into a
/// semicolon-separated single line when pasted into an editable <see cref="ComboBox"/>.
///
/// Without this the WPF TextBox inside the ComboBox (<see cref="TextBox.AcceptsReturn"/> is
/// <c>false</c>) silently drops everything after the first line when multiline text is pasted.
/// The conversion happens in <see cref="CommandManager.PreviewExecutedEvent"/> before the paste
/// command actually runs, by rewriting the clipboard content to the semicolon-separated form.
/// </summary>
public static class ComboBoxPasteBehavior
{
    public static readonly DependencyProperty ConvertMultilineToSemicolonProperty =
        DependencyProperty.RegisterAttached(
            "ConvertMultilineToSemicolon",
            typeof(bool),
            typeof(ComboBoxPasteBehavior),
            new PropertyMetadata(false, OnConvertMultilineToSemicolonChanged));

    public static void SetConvertMultilineToSemicolon(UIElement element, bool value) =>
        element.SetValue(ConvertMultilineToSemicolonProperty, value);

    public static bool GetConvertMultilineToSemicolon(UIElement element) =>
        (bool)element.GetValue(ConvertMultilineToSemicolonProperty);

    private static void OnConvertMultilineToSemicolonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
            return;

        if ((bool)e.NewValue)
            CommandManager.AddPreviewExecutedHandler(comboBox, OnPreviewExecuted);
        else
            CommandManager.RemovePreviewExecutedHandler(comboBox, OnPreviewExecuted);
    }

    private static void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        // Keyboard shortcut (Ctrl+V) and the paste context menu item both route through
        // ApplicationCommands.Paste. Editing commands don't need to be handled here.
        if (e.Command != ApplicationCommands.Paste)
            return;

        if (!Clipboard.ContainsText())
            return;

        var text = Clipboard.GetText();

        // Only rewrite when there is actual multiline content (e.g. a column pasted from Excel).
        if (!text.Contains('\n') && !text.Contains('\r'))
            return;

        var converted = string.Join(";", text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()));

        Clipboard.SetText(converted);
    }
}
