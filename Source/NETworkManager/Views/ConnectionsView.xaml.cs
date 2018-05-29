using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class ConnectionsView : UserControl
    {
        ConnectionsViewModel viewModel = new ConnectionsViewModel();

        public ConnectionsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}
