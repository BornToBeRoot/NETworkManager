using NETworkManager.ViewModels;
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
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            // Bulk edit only allows changing the comment
            if (DataContext is HostsFileEditorEntryViewModel { IsBulkEdit: true })
                TextBoxComment.Focus();
            else
                TextBoxIPAddress.Focus();
        }));
    }
}