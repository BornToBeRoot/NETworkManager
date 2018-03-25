using System.Windows.Controls;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class ARPTableView : UserControl
    {
        ARPTableViewModel viewModel = new ARPTableViewModel(DialogCoordinator.Instance);

        public ARPTableView()
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
