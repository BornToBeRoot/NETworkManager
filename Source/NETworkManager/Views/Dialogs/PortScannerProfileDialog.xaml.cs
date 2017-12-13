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
            // Need to be in loaded event, focusmanger won't work...
            txtName.Focus();
        }
    }
}
