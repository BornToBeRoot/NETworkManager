using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Controls;

/// <summary>
/// Attached property that prevents changing the value of a read-only <see cref="ComboBox"/> (e.g. the host of the
/// IP Scanner while a scan is running).
///
/// <see cref="ComboBox.IsReadOnly"/> only prevents typing into an editable ComboBox, an entry can still be selected
/// with the keyboard (Up / Down, Page Up / Page Down, F4 and Alt + Up / Down to open the drop-down) and, for a ComboBox
/// that is not editable, by typing the first letters. These keys are blocked while the ComboBox is read-only. The
/// ComboBox keeps the focus and the text can still be selected and copied, unlike a disabled ComboBox. Tab, Enter and
/// Esc still work (e.g. Esc to cancel a scan).
///
/// Opening the drop-down with the mouse is prevented by the drop-down toggle in <c>DefaultComboBox</c>
/// (not hit-test visible while read-only).
/// </summary>
public static class ComboBoxReadOnlyBehavior
{
    /// <summary>
    /// An editable ComboBox selects the next / previous entry in its own <c>OnPreviewKeyDown</c>, which runs before
    /// handlers added to the instance. A class handler of <see cref="ComboBox"/> runs before it.
    /// </summary>
    static ComboBoxReadOnlyBehavior()
    {
        EventManager.RegisterClassHandler(typeof(ComboBox), Keyboard.PreviewKeyDownEvent,
            new KeyEventHandler(ComboBox_PreviewKeyDown));
    }

    public static readonly DependencyProperty PreventSelectionWhenReadOnlyProperty =
        DependencyProperty.RegisterAttached(
            "PreventSelectionWhenReadOnly",
            typeof(bool),
            typeof(ComboBoxReadOnlyBehavior),
            new PropertyMetadata(false, OnPreventSelectionWhenReadOnlyChanged));

    public static void SetPreventSelectionWhenReadOnly(UIElement element, bool value) =>
        element.SetValue(PreventSelectionWhenReadOnlyProperty, value);

    public static bool GetPreventSelectionWhenReadOnly(UIElement element) =>
        (bool)element.GetValue(PreventSelectionWhenReadOnlyProperty);

    private static void OnPreventSelectionWhenReadOnlyChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
            return;

        if ((bool)e.NewValue)
        {
            comboBox.PreviewTextInput += ComboBox_PreviewTextInput;
            comboBox.DropDownOpened += ComboBox_DropDownOpened;
        }
        else
        {
            comboBox.PreviewTextInput -= ComboBox_PreviewTextInput;
            comboBox.DropDownOpened -= ComboBox_DropDownOpened;
        }
    }

    private static void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not ComboBox { IsReadOnly: true } comboBox || !GetPreventSelectionWhenReadOnly(comboBox))
            return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        switch (key)
        {
            // Select another entry or open the drop-down (Alt + Up / Down)
            case Key.Up:
            case Key.Down:
            case Key.PageUp:
            case Key.PageDown:
                e.Handled = true;
                break;
            // Open the drop-down - only F4 itself, Alt + F4 closes the window
            case Key.F4 when e.Key != Key.System:
                e.Handled = true;
                break;
            // Not editable: select the first / last entry (editable: move the caret in the text)
            case Key.Home:
            case Key.End:
                if (!comboBox.IsEditable)
                    e.Handled = true;
                break;
        }
    }

    private static void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Not editable: typing selects the entry that starts with the text (editable: the text box is read-only)
        if (sender is ComboBox { IsReadOnly: true, IsEditable: false })
            e.Handled = true;
    }

    private static void ComboBox_DropDownOpened(object sender, System.EventArgs e)
    {
        // Fallback if the drop-down is opened in another way
        if (sender is ComboBox { IsReadOnly: true } comboBox)
            comboBox.IsDropDownOpen = false;
    }
}
