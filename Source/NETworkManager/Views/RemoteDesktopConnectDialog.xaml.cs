using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class RemoteDesktopConnectDialog : UserControl
    {
        public RemoteDesktopConnectDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to be in loaded event, focusmanger won't work...
            cbHost.Focus();
        }
    }
}
