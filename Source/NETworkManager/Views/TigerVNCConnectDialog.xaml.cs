namespace NETworkManager.Views;

public partial class TigerVNCConnectDialog
{
    public TigerVNCConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}
