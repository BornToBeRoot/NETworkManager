using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ProfileChildWindow
{
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

        // Update the size of the child window when the parent window is resized
        parentWindow.SizeChanged += (_, _) =>
            {
                ChildWindowWidth = parentWindow.ActualWidth * 0.85;
                ChildWindowHeight = parentWindow.ActualHeight * 0.85;
            };
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
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