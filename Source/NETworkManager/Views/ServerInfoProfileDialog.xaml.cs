using NETworkManager.Models.Network;

namespace NETworkManager.Views
{
    public partial class ServerInfoProfileDialog
    {
        private (string Server, int Port) _newItemOptions;
        public ServerInfoProfileDialog((string Server, int Port)NewItemOptions)
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
            e.NewItem = new ServerInfo
            {
                Server = _newItemOptions.Server,
                Port = _newItemOptions.Port
            };
        }
    }
}
