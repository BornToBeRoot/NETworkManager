using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SubnetCalculatorHostView : UserControl
    {
        SubnetCalculatorHostViewModel viewModel = new SubnetCalculatorHostViewModel();

        public SubnetCalculatorHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
