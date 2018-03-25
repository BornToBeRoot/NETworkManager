using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsHotKeysView : UserControl
    {
        SettingsHotKeysViewModel viewModel = new SettingsHotKeysViewModel();

        public SettingsHotKeysView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
