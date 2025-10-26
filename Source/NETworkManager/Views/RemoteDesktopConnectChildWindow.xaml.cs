using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class RemoteDesktopConnectChildWindow
{
    public RemoteDesktopConnectChildWindow(Window parentWindow)
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            ComboBoxHost.Focus();
        }));
    }
}
