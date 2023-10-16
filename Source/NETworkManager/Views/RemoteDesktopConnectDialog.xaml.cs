namespace NETworkManager.Views;

public partial class RemoteDesktopConnectDialog
{
    public RemoteDesktopConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}
