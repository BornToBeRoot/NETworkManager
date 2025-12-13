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
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxName.Focus();
        }));
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = (ServerConnectionInfoProfileViewModel)DataContext;
    }
}