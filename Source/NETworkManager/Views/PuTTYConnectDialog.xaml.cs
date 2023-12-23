using System.Windows;

namespace NETworkManager.Views;

public partial class PuTTYConnectDialog
{
    public PuTTYConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}