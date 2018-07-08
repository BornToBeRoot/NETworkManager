namespace NETworkManager.Views
{
    public partial class RemoteDesktopConnectDialog
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
