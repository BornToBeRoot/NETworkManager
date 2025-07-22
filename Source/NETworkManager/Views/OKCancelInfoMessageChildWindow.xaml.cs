using System;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class OKCancelInfoMessageChildWindow
{
    public OKCancelInfoMessageChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            ButtonOK.Focus();
        }));
    }
}
