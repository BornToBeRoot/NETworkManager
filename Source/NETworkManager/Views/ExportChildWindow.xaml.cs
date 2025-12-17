using System;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class ExportChildWindow
{
    public ExportChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxExportFilePath.Focus();
        }));
    }
}
