using System.Windows.Controls;
using NETworkManager.ViewModels.Applications;

namespace NETworkManager.Views.Applications
{
    public partial class ARPTableView : UserControl
    {
        ARPTableViewModel viewModel = new ARPTableViewModel();

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
