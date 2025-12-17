using System;
using System.Windows;
using System.Windows.Controls;
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
            // Workaround to focus the editable part of the ComboBox
            var textBox = (TextBox)ComboBoxUrl.Template.FindName("PART_EditableTextBox", ComboBoxUrl);
            textBox?.Focus();
        }));
    }
}
