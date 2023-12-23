using System.Windows;

namespace NETworkManager.Views;

public partial class CredentialsSetPasswordDialog
{
    public CredentialsSetPasswordDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        PasswordBoxPassword.Focus();
    }
}