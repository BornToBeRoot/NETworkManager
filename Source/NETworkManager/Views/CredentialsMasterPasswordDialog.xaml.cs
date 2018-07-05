namespace NETworkManager.Views
{
    public partial class CredentialsMasterPasswordDialog
    {
        public CredentialsMasterPasswordDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to be in loaded event, focusmanger won't work...
            PasswordBoxPassword.Focus();
        }
    }
}
