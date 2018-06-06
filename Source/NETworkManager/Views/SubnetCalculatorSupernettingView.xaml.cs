using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorSupernettingView : UserControl
    {
        private SubnetCalculatorSupernettingViewModel viewModel = new SubnetCalculatorSupernettingViewModel();

        public SubnetCalculatorSupernettingView()
        {
            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            viewModel.OnShutdown();
        }
    }
}
