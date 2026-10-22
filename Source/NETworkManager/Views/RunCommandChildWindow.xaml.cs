using NETworkManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NETworkManager.Views;

/// <summary>
///     Run command dialog. The input keeps the focus; Up/Down select a suggestion, Tab completes it, Enter runs it
///     (Ctrl+Enter keeps the dialog open, Shift+Enter only fills in), Shift+Del removes a recent command and Escape
///     closes the dialog.
/// </summary>
public partial class RunCommandChildWindow
{
    /// <summary>
    ///     Width of the dialog in relation to the main window, between <see cref="MinDialogWidth" /> and
    ///     <see cref="MaxDialogWidth" />.
    /// </summary>
    private const double DialogWidthFactor = 0.5;

    private const double MinDialogWidth = 650;
    private const double MaxDialogWidth = 800;

    /// <summary>
    ///     Height of the main window that is not available for the dialog (title bar of the main window, the space
    ///     above the dialog (padding) and below it).
    /// </summary>
    private const double ReservedHeight = 160;

    /// <summary>
    ///     Maximum height of the dialog: at least the default of the child windows, at most
    ///     <see cref="MaxDialogHeight" />. The suggestions get the height that is left after the other rows and scroll,
    ///     so the key hints at the bottom stay visible.
    /// </summary>
    private const double MinDialogHeight = 500;

    private const double MaxDialogHeight = 900;

    /// <summary>
    ///     Width of the dialog that is not available for the text on the right of a suggestion (e.g. the syntax of a
    ///     command): icon, name and description, margins and scroll bar.
    /// </summary>
    private const double ReservedInfoWidth = 300;

    /// <summary>
    ///     Maximum width of the text on the right of a suggestion, grows with the dialog (see <see cref="UpdateSize" />).
    /// </summary>
    public static readonly DependencyProperty InfoMaxWidthProperty = DependencyProperty.Register(
        nameof(InfoMaxWidth), typeof(double), typeof(RunCommandChildWindow), new PropertyMetadata(250.0));

    public double InfoMaxWidth
    {
        get => (double)GetValue(InfoMaxWidthProperty);
        set => SetValue(InfoMaxWidthProperty, value);
    }

    private readonly Window _parentWindow;

    public RunCommandChildWindow(Window parentWindow)
    {
        InitializeComponent();

        _parentWindow = parentWindow;

        UpdateSize();

        // Update the size when the main window is resized (while the dialog is open)
        _parentWindow.SizeChanged += ParentWindow_SizeChanged;
        Unloaded += (_, _) =>
        {
            _parentWindow.SizeChanged -= ParentWindow_SizeChanged;
            ToolTipInvalid.IsOpen = false;
        };
    }

    private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSize();
    }

    /// <summary>
    ///     Uses more space for the dialog and the suggestions in a large main window.
    /// </summary>
    private void UpdateSize()
    {
        ChildWindowWidth = Math.Clamp(_parentWindow.ActualWidth * DialogWidthFactor, MinDialogWidth, MaxDialogWidth);
        InfoMaxWidth = ChildWindowWidth - ReservedInfoWidth;
        ChildWindowMaxHeight = Math.Clamp(_parentWindow.ActualHeight - ReservedHeight, MinDialogHeight,
            MaxDialogHeight);
    }

    private RunCommandViewModel ViewModel => (RunCommandViewModel)DataContext;

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        FocusInput();
    }

    /// <summary>
    ///     Focuses the input with the caret at the end, after the opened application (if the dialog is kept open) is
    ///     loaded and may have taken the focus.
    /// </summary>
    private void FocusInput()
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxInput.Focus();
            TextBoxInput.CaretIndex = TextBoxInput.Text.Length;
        }));
    }

    private void TextBoxInput_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // The error of a failed run is shown until the next key.
        ToolTipInvalid.IsOpen = false;

        switch (e.Key)
        {
            case Key.Down:
                ViewModel.SelectNext();
                break;
            case Key.Up:
                ViewModel.SelectPrevious();
                break;
            case Key.Tab:
                ViewModel.Complete();
                TextBoxInput.CaretIndex = TextBoxInput.Text.Length;
                break;
            case Key.Enter:
                RunAndShowError();
                break;
            // Only for a recent command, otherwise Shift+Del cuts the text.
            case Key.Delete when Keyboard.Modifiers == ModifierKeys.Shift && ViewModel.CanRemoveSelectedSuggestion:
                ViewModel.RemoveSuggestion(ViewModel.SelectedSuggestion);
                break;
            case Key.Escape:
                ViewModel.CancelCommand.Execute(null);
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void ListBoxSuggestions_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // A button in the suggestion (e.g. remove from history) handles the click itself.
        if (IsInButton((DependencyObject)e.OriginalSource))
            return;

        if (ItemsControl.ContainerFromElement(ListBoxSuggestions, (DependencyObject)e.OriginalSource) is not
            ListBoxItem item)
            return;

        ViewModel.SelectedSuggestion = (RunCommandSuggestion)item.DataContext;

        RunAndShowError();
    }

    /// <summary>
    ///     Runs the selected suggestion or the input (Enter or click), with Ctrl to keep the dialog open and with Shift to
    ///     only fill in. The dialog stays open if the input is not valid (the error is shown) or with Ctrl, then the input
    ///     gets the focus again (otherwise the opened application or dialog gets the focus).
    /// </summary>
    private void RunAndShowError()
    {
        var success = ViewModel.Run(Keyboard.Modifiers switch
        {
            ModifierKeys.Control => RunCommandRunMode.RunAndKeepOpen,
            ModifierKeys.Shift => RunCommandRunMode.FillIn,
            _ => RunCommandRunMode.Run
        });

        if (!success)
            ShowError();

        if (IsOpen)
            FocusInput();
    }

    /// <summary>
    ///     Opens the tooltip of the warning icon with the error of a failed run, so it is visible without hovering.
    /// </summary>
    private void ShowError()
    {
        // After the layout, since the icon may have just been shown.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
        {
            if (!RectangleInvalid.IsVisible)
                return;

            ToolTipInvalid.PlacementTarget = RectangleInvalid;
            ToolTipInvalid.Placement = PlacementMode.Bottom;
            ToolTipInvalid.IsOpen = true;
        }));
    }

    private void RectangleInvalid_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // The input is valid again or was cleared (e.g. with the clear button).
        if (!RectangleInvalid.IsVisible)
            ToolTipInvalid.IsOpen = false;
    }

    private void ButtonRemoveSuggestion_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveSuggestion((RunCommandSuggestion)((FrameworkElement)sender).DataContext);

        TextBoxInput.Focus();
    }

    private bool IsInButton(DependencyObject element)
    {
        while (element != null && element != ListBoxSuggestions)
        {
            if (element is Button)
                return true;

            // Text elements (e.g. a Run in a TextBlock) are not visuals
            element = element is Visual ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element);
        }

        return false;
    }

    private void ListBoxSuggestions_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListBoxSuggestions.SelectedItem != null)
            ListBoxSuggestions.ScrollIntoView(ListBoxSuggestions.SelectedItem);
    }
}
