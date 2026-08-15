using System;
using System.Linq;
using System.Runtime.InteropServices;
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
/// The conversion is applied by reading the clipboard, replacing the editable text box's
/// selection with the converted text, and marking <see cref="ApplicationCommands.Paste"/> as
/// handled - the system clipboard itself is never modified, so other applications (or a
/// subsequent paste elsewhere) are unaffected.
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

        // The command originates from the focused editable text box inside the ComboBox
        // template, which is where the pasted text actually needs to be inserted.
        if (e.OriginalSource is not TextBox textBox)
            return;

        string text;

        try
        {
            if (!Clipboard.ContainsText())
                return;

            text = Clipboard.GetText();
        }
        catch (ExternalException)
        {
            // Clipboard is temporarily locked by another process - fall back to the default paste.
            return;
        }

        // Only intervene when there is actual multiline content (e.g. a column pasted from
        // Excel). Otherwise let the default paste command handle it as usual.
        if (!text.Contains('\n') && !text.Contains('\r'))
            return;

        var converted = string.Join(";", text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()));

        // Replace the current selection with the converted text ourselves and mark the command
        // handled, instead of letting the default paste run - this avoids touching the system
        // clipboard entirely (no risk of losing other clipboard formats or racing a paste
        // elsewhere).
        var selectionStart = textBox.SelectionStart;
        var textBefore = textBox.Text[..selectionStart];
        var textAfter = textBox.Text[(selectionStart + textBox.SelectionLength)..];

        textBox.Text = textBefore + converted + textAfter;
        textBox.SelectionStart = selectionStart + converted.Length;
        textBox.SelectionLength = 0;

        e.Handled = true;
    }
}
