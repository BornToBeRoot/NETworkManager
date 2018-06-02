using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorCalculatorView : UserControl
    {
        private SubnetCalculatorCalculatorViewModel viewModel = new SubnetCalculatorCalculatorViewModel();

        public SubnetCalculatorCalculatorView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
