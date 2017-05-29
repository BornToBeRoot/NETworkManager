using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class SubnetCalculatorView : UserControl
    {
        private SubnetCalculatorViewModel viewModel = new SubnetCalculatorViewModel();

        public SubnetCalculatorView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
