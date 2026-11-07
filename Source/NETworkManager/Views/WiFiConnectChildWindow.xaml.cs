using System;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WiFiConnectChildWindow
{
    public WiFiConnectChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            if (PasswordBoxPreSharedKey.Visibility == Visibility.Visible)
                PasswordBoxPreSharedKey.Focus();
            else
                PasswordBoxPreSharedKey.Focus();
        }));

        // Shows the WPS button if WPS is available for the network
        if (DataContext is WiFiConnectViewModel viewModel)
            _ = viewModel.CheckWpsAsync();
    }
}