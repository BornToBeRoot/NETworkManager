using NETworkManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ServerConnectionInfoProfileChildWindow
{
    public ServerConnectionInfoProfileChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var isNameReadOnly = (DataContext as ServerConnectionInfoProfileViewModel)?.IsNameReadOnly ?? false;

        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            if (isNameReadOnly)
                TextBoxServer.Focus();
            else
                TextBoxName.Focus();
        }));
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = (ServerConnectionInfoProfileViewModel)DataContext;
    }
}