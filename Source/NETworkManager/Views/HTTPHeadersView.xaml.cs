using System.Windows.Controls;
using NETworkManager.ViewModels.Applications;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersView : UserControl
    {
        HTTPHeadersViewModel viewModel = new HTTPHeadersViewModel();

        public HTTPHeadersView()
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
