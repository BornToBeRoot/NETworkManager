using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorHostView : UserControl
    {
        private SubnetCalculatorIPv4CalculatorViewModel viewModel = new SubnetCalculatorIPv4CalculatorViewModel();

        public SubnetCalculatorHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
