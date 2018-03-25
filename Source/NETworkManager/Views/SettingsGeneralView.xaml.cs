using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsGeneralView : UserControl
    {
        SettingsGeneralViewModel viewModel = new SettingsGeneralViewModel();

        public SettingsGeneralView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
