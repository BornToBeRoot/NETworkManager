using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NETworkManager.Controls;

/// <summary>
/// Attached property that keeps the text of an editable <see cref="ComboBox"/> that shows a history (e.g. the hosts of
/// the Traceroute) when its selected entry is removed from the history.
///
/// The history is changed in place (<c>ListHelper.Modify</c>): the oldest entries are removed and, if the number of
/// stored entries is 0, all of them. If the removed entry is the selected item (e.g. the host that was just used, or the
/// host of another tab that shares the history), WPF clears the text of the ComboBox. The text is restored after the
/// ComboBox has updated it. If the entry is only deselected because another text was typed, it is still in the list and
/// nothing is changed.
/// </summary>
public static class ComboBoxHistoryBehavior
{
    public static readonly DependencyProperty KeepTextWhenSelectedItemIsRemovedProperty =
        DependencyProperty.RegisterAttached(
            "KeepTextWhenSelectedItemIsRemoved",
            typeof(bool),
            typeof(ComboBoxHistoryBehavior),
            new PropertyMetadata(false, OnKeepTextWhenSelectedItemIsRemovedChanged));

    public static void SetKeepTextWhenSelectedItemIsRemoved(UIElement element, bool value) =>
        element.SetValue(KeepTextWhenSelectedItemIsRemovedProperty, value);

    public static bool GetKeepTextWhenSelectedItemIsRemoved(UIElement element) =>
        (bool)element.GetValue(KeepTextWhenSelectedItemIsRemovedProperty);

    private static void OnKeepTextWhenSelectedItemIsRemovedChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
            return;

        if ((bool)e.NewValue)
            comboBox.SelectionChanged += ComboBox_SelectionChanged;
        else
            comboBox.SelectionChanged -= ComboBox_SelectionChanged;
    }

    private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { IsEditable: true } comboBox || comboBox.SelectedItem != null ||
            e.RemovedItems.Count == 0)
            return;

        var removedItem = e.RemovedItems[0];

        // Deselected because another text was typed: the entry is still in the list.
        if (comboBox.Items.Contains(removedItem))
            return;

        var text = removedItem?.ToString();

        if (string.IsNullOrEmpty(text))
            return;

        // After the ComboBox has cleared its text (and only if it did)
        comboBox.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(delegate
        {
            if (string.IsNullOrEmpty(comboBox.Text))
                comboBox.Text = text;
        }));
    }
}
