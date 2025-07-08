using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class HostsFileEditorEntryChildWindow
{
    public HostsFileEditorEntryChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Focus the PasswordBox when the child window is loaded 
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxIPAddress.Focus();
        }));
    }
}