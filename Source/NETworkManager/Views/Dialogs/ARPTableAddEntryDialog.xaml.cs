using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class ARPTableAddEntryDialog : UserControl
    {
        public ARPTableAddEntryDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtIPAddress.Focus();
        }
    }
}
