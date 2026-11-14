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
        ChildWindowSizeHelper.FollowParentSize(this, parentWindow, height: true);
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
            TextBoxName.CaretIndex = TextBoxName.Text.Length;
        }));
    }

    private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;
    }
}