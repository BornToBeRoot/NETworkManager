namespace NETworkManager.Views
{
    public partial class TightVNCConnectDialog
    {
        public TightVNCConnectDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to be in loaded event, focusmanger won't work...
            ComboBoxHost.Focus();
        }
    }
}
