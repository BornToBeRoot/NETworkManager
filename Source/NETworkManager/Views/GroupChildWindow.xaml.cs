using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class GroupChildWindow
{
    public GroupChildWindow(Window parentWindow)
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
        // Focus the PasswordBox when the child window is loaded 
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
        }));
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }
}