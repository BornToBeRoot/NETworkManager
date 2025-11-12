using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class PuTTYConnectChildWindow
{
    public PuTTYConnectChildWindow()
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
