using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class LookupPortLookupView : UserControl
    {
        LookupPortLookupViewModel viewModel = new LookupPortLookupViewModel();

        public LookupPortLookupView()
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