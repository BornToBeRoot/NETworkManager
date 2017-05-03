using NETworkManager.ViewModel.Applications;
using System.Windows.Controls;

namespace NETworkManager.View.Applications
{
    public partial class SubnetCalculatorUserControl : UserControl
    {
        private SubnetCalculatorViewModel viewModel = new SubnetCalculatorViewModel();

        public SubnetCalculatorUserControl()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
