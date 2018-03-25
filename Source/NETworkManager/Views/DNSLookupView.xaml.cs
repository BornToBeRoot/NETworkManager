using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupView : UserControl
    {
        DNSLookupViewModel viewModel = new DNSLookupViewModel();

        public DNSLookupView()
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
