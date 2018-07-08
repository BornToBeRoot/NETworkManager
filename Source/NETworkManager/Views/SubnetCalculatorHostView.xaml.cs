using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorHostView
    {
        private readonly SubnetCalculatorHostViewModel _viewModel = new SubnetCalculatorHostViewModel();

        public SubnetCalculatorHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
