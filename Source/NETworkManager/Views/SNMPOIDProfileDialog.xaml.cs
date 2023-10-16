namespace NETworkManager.Views;

public partial class SNMPOIDProfileDialog
{
    public SNMPOIDProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxName.Focus();
    }
}
