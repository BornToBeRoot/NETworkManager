using System.Windows.Controls;

namespace NETworkManager.Views.Dialogs
{
    public partial class PortScannerProfileDialog : UserControl
    {
        public PortScannerProfileDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
