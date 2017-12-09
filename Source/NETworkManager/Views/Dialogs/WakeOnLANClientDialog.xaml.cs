using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class WakeOnLANClientDialog : UserControl
    {
        public WakeOnLANClientDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
