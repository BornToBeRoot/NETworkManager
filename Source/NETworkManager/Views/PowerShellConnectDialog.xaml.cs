using System.Windows;

namespace NETworkManager.Views;

public partial class PowerShellConnectDialog
{
    public PowerShellConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}