using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
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
