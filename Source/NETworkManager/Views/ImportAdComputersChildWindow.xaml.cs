using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ImportAdComputersChildWindow
{
    public ImportAdComputersChildWindow(Window parentWindow)
    {
        InitializeComponent();

        ChildWindowMaxWidth = 720;
        ChildWindowWidth = Math.Min(640, Math.Max(480, parentWindow.ActualWidth * 0.55));

        parentWindow.SizeChanged += (_, _) =>
        {
            ChildWindowWidth = Math.Min(640, Math.Max(480, parentWindow.ActualWidth * 0.55));
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
