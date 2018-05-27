using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PortScannerHostView : UserControl
    {
        PortScannerHostViewModel viewModel = new PortScannerHostViewModel(DialogCoordinator.Instance);

        public PortScannerHostView()
        {
            InitializeComponent();
            DataContext = viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.PortScanner.ToString();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                viewModel.ScanProfileCommand.Execute(null);
        }

        public void AddTab(string host)
        {
            viewModel.AddTab(host);
        }
    }
}
