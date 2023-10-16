namespace NETworkManager.Views;

public partial class CredentialsChangePasswordDialog
{
    public CredentialsChangePasswordDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        PasswordBoxPassword.Focus();
    }
}
