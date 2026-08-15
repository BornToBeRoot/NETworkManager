using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Controls;

/// <summary>
/// Attached property that converts multiline text (e.g. a column pasted from Excel) into a
/// semicolon-separated single line when pasted into an editable <see cref="ComboBox"/>.
///
/// Without this the WPF TextBox inside the ComboBox (<see cref="TextBox.AcceptsReturn"/> is
/// <c>false</c>) silently drops everything after the first line when multiline text is pasted.
/// The conversion happens in <see cref="CommandManager.PreviewExecutedEvent"/> before the paste
/// command actually runs, by rewriting the clipboard content to the semicolon-separated form.
/// The original clipboard content is restored afterwards, and clipboard access is guarded since
/// the Windows clipboard can throw when briefly locked by another process.
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

        string originalText;

        try
        {
            if (!Clipboard.ContainsText())
                return;

            originalText = Clipboard.GetText();
        }
        catch (ExternalException)
        {
            // Clipboard is temporarily locked by another process - fall back to the default paste.
            return;
        }

        // Only rewrite when there is actual multiline content (e.g. a column pasted from Excel).
        if (!originalText.Contains('\n') && !originalText.Contains('\r'))
            return;

        var converted = string.Join(";", originalText
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()));

        try
        {
            Clipboard.SetText(converted);
        }
        catch (ExternalException)
        {
            return;
        }

        // Restore the original clipboard content once the paste command has consumed the
        // rewritten text, so pasting elsewhere afterwards still yields what the user actually copied.
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () =>
        {
            try
            {
                Clipboard.SetText(originalText);
            }
            catch (ExternalException)
            {
                // Best effort - leave the rewritten text on the clipboard if restoring fails.
            }
        });
    }
}
