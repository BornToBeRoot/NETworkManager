using System;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class MessageConfirmationChildWindow
{
    public MessageConfirmationChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            ButtonConfirm.Focus();
        }));
    }
}
