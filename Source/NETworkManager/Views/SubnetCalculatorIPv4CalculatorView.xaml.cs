using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorIPv4CalculatorView : UserControl
    {
        private SubnetCalculatorIPv4CalculatorViewModel viewModel = new SubnetCalculatorIPv4CalculatorViewModel();

        public SubnetCalculatorIPv4CalculatorView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
