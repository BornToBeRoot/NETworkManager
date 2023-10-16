namespace NETworkManager.Views;

public partial class ARPTableAddEntryDialog
{
    public ARPTableAddEntryDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxIPAddress.Focus();
    }
}
