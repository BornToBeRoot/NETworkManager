using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Attached-property helpers for search <see cref="TextBox"/>es: <c>Enter</c> in a search box only filters. It is
/// handled, so it doesn't trigger the default button (<see cref="Button.IsDefault"/>) of the view, dialog or window
/// (e.g. start a scan or close a dialog with OK). With <see cref="EnterCommandProperty"/> the command runs first, e.g.
/// to filter right away instead of after the delay while typing.
/// </summary>
public static class SearchTextBoxHelper
{
    /// <summary>
    /// When set to <see langword="true"/>, <c>Enter</c> in the text box is handled (see <see cref="SearchTextBoxHelper"/>).
    /// </summary>
    public static readonly DependencyProperty HandleEnterProperty =
        DependencyProperty.RegisterAttached(
            "HandleEnter",
            typeof(bool),
            typeof(SearchTextBoxHelper),
            new PropertyMetadata(false, OnChanged));

    public static void SetHandleEnter(TextBox element, bool value) =>
        element.SetValue(HandleEnterProperty, value);

    public static bool GetHandleEnter(TextBox element) =>
        (bool)element.GetValue(HandleEnterProperty);

    /// <summary>
    /// Command executed on <c>Enter</c> in the text box (e.g. apply the search right away). Setting it also handles
    /// <c>Enter</c> (see <see cref="HandleEnterProperty"/>).
    /// </summary>
    public static readonly DependencyProperty EnterCommandProperty =
        DependencyProperty.RegisterAttached(
            "EnterCommand",
            typeof(ICommand),
            typeof(SearchTextBoxHelper),
            new PropertyMetadata(null, OnChanged));

    public static void SetEnterCommand(TextBox element, ICommand value) =>
        element.SetValue(EnterCommandProperty, value);

    public static ICommand GetEnterCommand(TextBox element) =>
        (ICommand)element.GetValue(EnterCommandProperty);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        // Subscribe once, whichever of the two properties is set
        textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;

        if (GetHandleEnter(textBox) || GetEnterCommand(textBox) != null)
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
    }

    private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox textBox)
            return;

        // Handled in the preview, so neither the default button nor other key bindings of the window get it
        e.Handled = true;

        var command = GetEnterCommand(textBox);

        if (command?.CanExecute(null) == true)
            command.Execute(null);
    }
}
