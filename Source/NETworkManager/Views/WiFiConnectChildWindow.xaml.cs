using System;
using System.Windows;
using System.Windows.Threading;

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
            if(PasswordBoxPreSharedKey.Visibility == Visibility.Visible)
                PasswordBoxPreSharedKey.Focus();
            else
                PasswordBoxPreSharedKey.Focus();
        }));
    }
}