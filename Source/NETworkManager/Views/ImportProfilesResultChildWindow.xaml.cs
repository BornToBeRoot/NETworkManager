using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ImportProfilesResultChildWindow
{
    public ImportProfilesResultChildWindow(Window parentWindow)
    {
        InitializeComponent();

        ChildWindowMaxWidth = 1050;
        ChildWindowMaxHeight = 650;
        ChildWindowWidth = parentWindow.ActualWidth * 0.85;
        //ChildWindowHeight = parentWindow.ActualHeight * 0.85;

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
            TextBoxFilter.Focus();
        }));
    }
}