namespace NETworkManager.Views;

public partial class CredentialsPasswordDialog
{
    public CredentialsPasswordDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        PasswordBoxPassword.Focus();
    }
}
