using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsDeveloperView : UserControl
    {
        SettingsDeveloperViewModel viewModel = new SettingsDeveloperViewModel();

        public SettingsDeveloperView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
