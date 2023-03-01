using NETworkManager.Models.Network;

namespace NETworkManager.Views
{
    public partial class ServerConnectionInfoProfileDialog
    {
        private (string Server, int Port, TransportProtocol TransportProtocol) _newItemOptions;
        public ServerConnectionInfoProfileDialog((string Server, int Port, TransportProtocol TransportProtocol) NewItemOptions)
        {
            InitializeComponent();

            _newItemOptions = NewItemOptions;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Need to be in loaded event, focusmanger won't work...
            TextBoxName.Focus();
        }

        private void DataGridServers_AddingNewItem(object sender, System.Windows.Controls.AddingNewItemEventArgs e)
        {
            e.NewItem = new ServerConnectionInfo
            {
                Server = _newItemOptions.Server,
                Port = _newItemOptions.Port,
                TransportProtocol = _newItemOptions.TransportProtocol
            };
        }
    }
}
