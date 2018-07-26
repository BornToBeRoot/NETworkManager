using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorSupernettingView
    {
        private readonly SubnetCalculatorSupernettingViewModel _viewModel = new SubnetCalculatorSupernettingViewModel();

        public SubnetCalculatorSupernettingView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            _viewModel.OnShutdown();
        }
    }
}
