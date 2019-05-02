using NETworkManager.ViewModels;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class LookupPortLookupView
    {
        private readonly LookupPortLookupViewModel _viewModel = new LookupPortLookupViewModel(DialogCoordinator.Instance);

        public LookupPortLookupView()
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