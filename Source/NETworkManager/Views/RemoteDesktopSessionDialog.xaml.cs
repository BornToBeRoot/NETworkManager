using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class RemoteDesktopSessionDialog : UserControl
    {
        public RemoteDesktopSessionDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to be in loaded event, focusmanger won't work...
            txtName.Focus();
        }
    }
}
