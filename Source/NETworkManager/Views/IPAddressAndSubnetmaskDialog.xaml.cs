﻿namespace NETworkManager.Views;

public partial class IPAddressAndSubnetmaskDialog
{
    public IPAddressAndSubnetmaskDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxIPAddress.Focus();
    }
}
