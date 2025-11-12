using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class WebConsoleConnectChildWindow
{
    public WebConsoleConnectChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            ComboBoxUrl.Focus();
        }));
    }
}
