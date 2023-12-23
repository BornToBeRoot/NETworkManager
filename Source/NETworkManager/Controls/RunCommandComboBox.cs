using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.Controls;

/*
 * Modified from https://stackoverflow.com/a/58066259
 */

public class RunCommandComboBox : ComboBox
{
    private readonly UserChange<bool> _isDropDownOpenUc;
    private string _currentFilter = string.Empty;
    private bool _textBoxFreezed;

    public RunCommandComboBox()
    {
        _isDropDownOpenUc = new UserChange<bool>(v => IsDropDownOpen = v);
        DropDownOpened += FilteredComboBox_DropDownOpened;

        IsEditable = true;
        IsTextSearchEnabled = false;
        StaysOpenOnEdit = true;
        IsReadOnly = false;

        Loaded += (_, _) =>
        {
            if (EditableTextBox != null)
                new TextBoxBaseUserChangeTracker(EditableTextBox).UserTextChanged += FilteredComboBox_UserTextChange;
        };
    }

    private TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;

    /// <summary>
    ///     Open the dropdown when the control gets focus.
    /// </summary>
    /// <param name="e">Focus change information.</param>
    protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnPreviewGotKeyboardFocus(e);

        Width = 350;
        IsDropDownOpen = true;
    }

    protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnPreviewLostKeyboardFocus(e);

        Width = 250;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down when !IsDropDownOpen:
                IsDropDownOpen = true;
                e.Handled = true;
                break;
            case Key.Escape:
                ClearFilter();
                Text = string.Empty;
                IsDropDownOpen = true;
                e.Handled = true;
                break;
            // Auto-complete the text when the user presses the enter key, tab key, or right arrow
            // and the drop-down is open.
            case Key.Enter:
            case Key.Tab:
            case Key.Right:
                if (IsDropDownOpen)
                {
                    // Insert the selected item into the text box.
                    var result = InsertSelectedItem();

                    // Close the drop-down.
                    IsDropDownOpen = false;

                    // If the enter key was pressed and the item has no arguments then use the default
                    // behavior (which will fire the enter key command in the control - to redirect
                    // to another application).
                    if (e.Key == Key.Enter && result == 0)
                    {
                        e.Handled = false;
                        break;
                    }

                    // Handle the key press (tab, enter or right arrow) to auto-complete the text and 
                    // don't pass it to the control.
                    if (result != -1)
                        e.Handled = true;

                    // If no item was selected, the result will be -1 and the key press will be
                    // passed to the control.
                }

                break;
        }

        base.OnPreviewKeyDown(e);
    }

    /// <summary>
    ///     Inserts the selected item into the text box.
    /// </summary>
    /// <returns>
    ///     Returns 0 if the item was inserted, 1 if the item was inserted with example arguments, and -1 if the item was
    ///     not inserted.
    /// </returns>
    private int InsertSelectedItem()
    {
        if (SelectedItem is not RunCommandInfo info)
            return -1;

        Text = $"{info.Command}";

        if (info.CanHandleArguments)
        {
            Text += $" {info.ExampleArguments}";
            EditableTextBox.SelectionStart = Text.Length - info.ExampleArguments.Length;
            EditableTextBox.SelectionLength = info.ExampleArguments.Length;

            return 1;
        }

        EditableTextBox.SelectionStart = Text.Length;
        EditableTextBox.SelectionLength = 0;

        return 0;
    }

    private void ClearFilter()
    {
        if (string.IsNullOrEmpty(_currentFilter))
            return;

        _currentFilter = string.Empty;

        CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
    }

    private void FilteredComboBox_DropDownOpened(object sender, EventArgs e)
    {
        if (_isDropDownOpenUc.IsUserChange)
            ClearFilter();
    }

    private void FilteredComboBox_UserTextChange(object sender, EventArgs e)
    {
        if (_textBoxFreezed)
            return;

        var tb = EditableTextBox;
        _currentFilter = tb.SelectionStart + tb.SelectionLength == tb.Text.Length
            ? tb.Text[..tb.SelectionStart].ToLower()
            : tb.Text.ToLower();

        RefreshFilter();
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (newValue != null)
        {
            var view = CollectionViewSource.GetDefaultView(newValue);
            view.Filter += FilterItem;
        }

        if (oldValue != null)
        {
            var view = CollectionViewSource.GetDefaultView(oldValue);
            if (view != null) view.Filter -= FilterItem;
        }

        base.OnItemsSourceChanged(oldValue, newValue);
    }

    private void RefreshFilter()
    {
        if (ItemsSource == null)
            return;

        var view = CollectionViewSource.GetDefaultView(ItemsSource);

        FreezeTextBoxState(() =>
        {
            var isDropDownOpen = IsDropDownOpen;
            //always hide because showing it enables the user to pick with up and down keys, otherwise it's not working because of the glitch in view.Refresh()
            _isDropDownOpenUc.Set(false);
            view.Refresh();

            if (!string.IsNullOrEmpty(_currentFilter) || isDropDownOpen)
                _isDropDownOpenUc.Set(true);

            if (SelectedItem != null)
                return;

            // Loop through all items
            foreach (RunCommandInfo item in ItemsSource)
            {
                // Check if any item contains the text
                if (!item.TranslatedName.Contains(Text, StringComparison.OrdinalIgnoreCase) &&
                    !item.Name.Contains(Text, StringComparison.OrdinalIgnoreCase) &&
                    !item.Command.Contains(Text, StringComparison.OrdinalIgnoreCase))
                    continue;

                SelectedItem = item;
                break;
            }
        });
    }

    private void FreezeTextBoxState(Action action)
    {
        _textBoxFreezed = true;
        var tb = EditableTextBox;
        var text = Text;
        var selStart = tb.SelectionStart;
        var selLen = tb.SelectionLength;
        action();
        Text = text;
        tb.SelectionStart = selStart;
        tb.SelectionLength = selLen;
        _textBoxFreezed = false;
    }

    private bool FilterItem(object value)
    {
        if (value is not RunCommandInfo info)
            return false;

        return _currentFilter.Length == 0 ||
               info.TranslatedName.Contains(Text, StringComparison.OrdinalIgnoreCase) ||
               info.Name.Contains(Text, StringComparison.OrdinalIgnoreCase) ||
               info.Command.Contains(Text, StringComparison.OrdinalIgnoreCase);

        //value.ToString()!.ToLower().Contains(_currentFilter);
    }

    private class TextBoxBaseUserChangeTracker
    {
        private readonly List<Key> _pressedKeys = new();

        public TextBoxBaseUserChangeTracker(TextBoxBase textBoxBase)
        {
            TextBoxBase = textBoxBase;
            var lastText = TextBoxBase.ToString();

            textBoxBase.PreviewTextInput += (_, _) => { IsTextInput = true; };

            textBoxBase.TextChanged += (_, e) =>
            {
                var isUserChange = _pressedKeys.Count > 0 || IsTextInput || lastText == TextBoxBase.ToString();
                IsTextInput = false;
                lastText = TextBoxBase.ToString();
                if (isUserChange)
                    UserTextChanged?.Invoke(this, e);
            };

            textBoxBase.PreviewKeyDown += (_, e) =>
            {
                switch (e.Key)
                {
                    case Key.Back:
                    case Key.Space:
                        if (!_pressedKeys.Contains(e.Key))
                            _pressedKeys.Add(e.Key);
                        break;
                }

                if (e.Key != Key.Back)
                    return;

                var textBox = textBoxBase as TextBox;
                if (textBox!.SelectionStart <= 0 || textBox.SelectionLength <= 0 ||
                    textBox.SelectionStart + textBox.SelectionLength != textBox.Text.Length)
                    return;

                textBox.SelectionStart--;
                textBox.SelectionLength++;

                e.Handled = true;

                UserTextChanged?.Invoke(this, e);
            };

            textBoxBase.PreviewKeyUp += (_, e) =>
            {
                if (_pressedKeys.Contains(e.Key))
                    _pressedKeys.Remove(e.Key);
            };

            textBoxBase.LostFocus += (_, _) =>
            {
                _pressedKeys.Clear();
                IsTextInput = false;
            };
        }

        private bool IsTextInput { get; set; }

        private TextBoxBase TextBoxBase { get; }
        public event EventHandler UserTextChanged;
    }

    private class UserChange<T>
    {
        private readonly Action<T> _action;

        public UserChange(Action<T> action)
        {
            _action = action;
        }

        public bool IsUserChange { get; private set; } = true;

        public void Set(T val)
        {
            try
            {
                IsUserChange = false;
                _action(val);
            }
            finally
            {
                IsUserChange = true;
            }
        }
    }
}