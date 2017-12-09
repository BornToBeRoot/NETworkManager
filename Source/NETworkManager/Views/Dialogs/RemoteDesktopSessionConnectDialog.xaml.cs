using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class RemoteDesktopSessionConnectDialog : UserControl
    {
        public RemoteDesktopSessionConnectDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtHostname.Focus();
        }
    }
}
