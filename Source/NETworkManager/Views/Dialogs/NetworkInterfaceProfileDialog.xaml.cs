using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class NetworkInterfaceProfileDialog : UserControl
    {
        public NetworkInterfaceProfileDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
