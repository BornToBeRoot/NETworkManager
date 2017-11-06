using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class WikiOUILookupView : UserControl
    {
        WikiOUILookupViewModel viewModel = new WikiOUILookupViewModel();

        public WikiOUILookupView()
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
