using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsAppearanceView : UserControl
    {
        SettingsAppearanceViewModel viewModel = new SettingsAppearanceViewModel();

        public SettingsAppearanceView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
