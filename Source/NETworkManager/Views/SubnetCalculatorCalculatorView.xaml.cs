using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorCalculatorView
    {
        private readonly SubnetCalculatorCalculatorViewModel _viewModel = new SubnetCalculatorCalculatorViewModel();

        public SubnetCalculatorCalculatorView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
