namespace NETworkManager.Views;

public partial class AWSProfileDialog
{
    public AWSProfileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        TextBoxProfile.Focus();
    }
}
