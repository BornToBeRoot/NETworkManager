using NETworkManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class SNMPOIDProfilesChildWindow
{
    public SNMPOIDProfilesChildWindow(Window parentWindow)
    {
        InitializeComponent();

        // Set the width and height of the child window based on the parent window size
        ChildWindowMaxWidth = 850;
        ChildWindowMaxHeight = 650;
        ChildWindowSizeHelper.FollowParentSize(this, parentWindow, height: false);
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxSearch.Focus();
        }));
    }

    private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var x = (SNMPOIDProfilesViewModel)DataContext;

        if (x.OKCommand.CanExecute(null))
            x.OKCommand.Execute(null);
    }
}
