using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class LookupOUILookupView : UserControl
    {
        LookupOUILookupViewModel viewModel = new LookupOUILookupViewModel();

        public LookupOUILookupView()
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
