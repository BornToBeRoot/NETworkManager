namespace NETworkManager.Views;

public partial class IPAddressDialog
{
    public IPAddressDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxIPAddress.Focus();
    }
}
