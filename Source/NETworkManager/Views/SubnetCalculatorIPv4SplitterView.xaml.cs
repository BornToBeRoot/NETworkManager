using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorIPv4SplitterView : UserControl
    {
        private SubnetCalculatorIPv4SplitterViewModel viewModel = new SubnetCalculatorIPv4SplitterViewModel(DialogCoordinator.Instance);

        public SubnetCalculatorIPv4SplitterView()
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
