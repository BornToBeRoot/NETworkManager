namespace NETworkManager.Views;

public partial class AWSSessionManagerConnectDialog
{
    public AWSSessionManagerConnectDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        ComboBoxInstanceID.Focus();
    }
}
