namespace NETworkManager.Views;

public partial class WebConsoleConnectDialog
{
    public WebConsoleConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ComboBoxUrl.Focus();
    }
}
