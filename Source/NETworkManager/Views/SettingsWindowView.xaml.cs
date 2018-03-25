using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsWindowView : UserControl
    {
        SettingsWindowViewModel viewModel = new SettingsWindowViewModel();

        public SettingsWindowView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
