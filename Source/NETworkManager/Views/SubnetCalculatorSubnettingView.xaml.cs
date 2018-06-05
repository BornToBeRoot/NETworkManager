using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorSubnettingView : UserControl
    {
        private SubnetCalculatorSubnettingViewModel viewModel = new SubnetCalculatorSubnettingViewModel(DialogCoordinator.Instance);

        public SubnetCalculatorSubnettingView()
        {
            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            viewModel.OnShutdown();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}
