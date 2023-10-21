namespace NETworkManager.Views;

public partial class CredentialsPasswordProfileFileDialog
{
    public CredentialsPasswordProfileFileDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        PasswordBoxPassword.Focus();
    }
}
