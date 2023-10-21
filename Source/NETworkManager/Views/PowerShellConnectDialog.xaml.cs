namespace NETworkManager.Views;

public partial class PowerShellConnectDialog
{
    public PowerShellConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}
