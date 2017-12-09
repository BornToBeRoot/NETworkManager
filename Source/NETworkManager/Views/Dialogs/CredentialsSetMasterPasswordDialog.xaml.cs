using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class CredentialsSetMasterPasswordDialog : UserControl
    {
        public CredentialsSetMasterPasswordDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            passwordBoxPassword.Focus();
        }
    }
}
