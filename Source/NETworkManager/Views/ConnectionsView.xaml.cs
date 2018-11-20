using NETworkManager.ViewModels;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class ConnectionsView
    {
        private readonly ConnectionsViewModel _viewModel = new ConnectionsViewModel(DialogCoordinator.Instance);

        public ConnectionsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }
    }
}
