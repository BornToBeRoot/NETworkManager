using System.Windows.Controls;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class ARPTableView
    {
        private readonly ARPTableViewModel _viewModel = new ARPTableViewModel(DialogCoordinator.Instance);

        public ARPTableView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }
    }
}
