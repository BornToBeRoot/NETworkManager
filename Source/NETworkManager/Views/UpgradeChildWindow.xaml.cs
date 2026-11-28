using System;
using System.Windows;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class UpgradeChildWindow
{
    public UpgradeChildWindow(Window parentWindow)
    {
        InitializeComponent();

        // 85 % of the window, up to ChildWindowMaxWidth (750): fits the minimum window width with a margin, like the
        // other large dialogs
        ChildWindowSizeHelper.FollowParentSize(this, parentWindow, height: false);
    }

    private void ChildWindow_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            ButtonContinue.Focus();
        }));
    }
}
