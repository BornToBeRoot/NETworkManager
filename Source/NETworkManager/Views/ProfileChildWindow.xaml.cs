using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ProfileChildWindow
{
    private readonly Window _parentWindow;

    // Set name as hostname (if empty or identical)
    private string _oldName = string.Empty;

    public ProfileChildWindow(Window parentWindow)
    {
        InitializeComponent();

        // Set the width and height of the child window based on the parent window size
        ChildWindowMaxWidth = 1050;
        ChildWindowMaxHeight = 650;
        ChildWindowWidth = parentWindow.ActualWidth * 0.85;
        ChildWindowHeight = parentWindow.ActualHeight * 0.85;

        _parentWindow = parentWindow;

        // Update the size of the child window when the parent window is resized (while the dialog is open,
        // unsubscribed when it is closed - otherwise the parent window keeps every closed dialog in memory)
        _parentWindow.SizeChanged += ParentWindow_SizeChanged;
        Unloaded += (_, _) => _parentWindow.SizeChanged -= ParentWindow_SizeChanged;
    }

    private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ChildWindowWidth = _parentWindow.ActualWidth * 0.85;
        ChildWindowHeight = _parentWindow.ActualHeight * 0.85;
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
            TextBoxName.CaretIndex = TextBoxName.Text.Length;
        }));
    }

    private void TextBoxName_OnGotFocus(object sender, RoutedEventArgs e)
    {
        _oldName = TextBoxName.Text;
    }

    private void TextBoxName_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_oldName == TextBoxHost.Text)
            TextBoxHost.Text = TextBoxName.Text;

        _oldName = TextBoxName.Text;
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }
}