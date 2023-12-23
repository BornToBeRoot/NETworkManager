using System.Windows;

namespace NETworkManager.Views;

public partial class IPAddressAndSubnetmaskDialog
{
    public IPAddressAndSubnetmaskDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        TextBoxIPAddress.Focus();
    }
}