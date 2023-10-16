namespace NETworkManager.Views;

public partial class CredentialsSetPasswordDialog
{
    public CredentialsSetPasswordDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        PasswordBoxPassword.Focus();
    }
}
