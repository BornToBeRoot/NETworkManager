using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class BitCalculatorSettingsView
    {
        private readonly BitCalculatorSettingsViewModel _viewModel = new();

        public BitCalculatorSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
