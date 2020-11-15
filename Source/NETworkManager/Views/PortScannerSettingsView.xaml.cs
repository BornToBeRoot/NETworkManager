using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views
{
    public partial class PortScannerSettingsView
    {
        private readonly PortScannerSettingsViewModel _viewModel = new PortScannerSettingsViewModel(DialogCoordinator.Instance);

        public PortScannerSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
        private void RowContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EditPortProfile();
        }
    }
}
