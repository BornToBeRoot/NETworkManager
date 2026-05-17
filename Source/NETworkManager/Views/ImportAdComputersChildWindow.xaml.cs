using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ImportAdComputersChildWindow
{
    public ImportAdComputersChildWindow(Window parentWindow)
    {
        InitializeComponent();

        // Set the width and height of the child window based on the parent window size
        ChildWindowMaxWidth = 850;
        ChildWindowMaxHeight = 650;
        ChildWindowWidth = parentWindow.ActualWidth * 0.85;
        //ChildWindowHeight = parentWindow.ActualHeight * 0.85;

        // Update the size of the child window when the parent window is resized
        parentWindow.SizeChanged += (_, _) =>
        {
            ChildWindowWidth = parentWindow.ActualWidth * 0.85;
            //ChildWindowHeight = parentWindow.ActualHeight * 0.85;
        };
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxLdapSearchBase.Focus();
        }));
    }
}
