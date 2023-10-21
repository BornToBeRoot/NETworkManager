namespace NETworkManager.Views;

public partial class PuTTYConnectDialog
{
    public PuTTYConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ComboBoxHost.Focus();
    }
}
