using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views
{
    public partial class IPScannerSettingsView
    {
        private readonly IPScannerSettingsViewModel _viewModel = new IPScannerSettingsViewModel(DialogCoordinator.Instance);

        public IPScannerSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
        private void RowContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EditCustomCommand();
        }
    }
}
