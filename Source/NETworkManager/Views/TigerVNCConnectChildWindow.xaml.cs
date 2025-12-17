using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NETworkManager.Views;

public partial class TigerVNCConnectChildWindow
{
    public TigerVNCConnectChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            // Workaround to focus the editable part of the ComboBox
            var textBox = (TextBox)ComboBoxHost.Template.FindName("PART_EditableTextBox", ComboBoxHost);
            textBox?.Focus();
        }));
    }
}