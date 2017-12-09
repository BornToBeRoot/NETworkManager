using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class RemoteDesktopSessionDialog : UserControl
    {
        public RemoteDesktopSessionDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
