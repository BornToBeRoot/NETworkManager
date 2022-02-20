using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupSettingsView
    {
        private readonly DNSLookupSettingsViewModel _viewModel = new DNSLookupSettingsViewModel(DialogCoordinator.Instance);

        public DNSLookupSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EditDNSServer();
        }
    }
}
