using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsLanguageView : UserControl
    {
        SettingsLanguageViewModel viewModel = new SettingsLanguageViewModel();
        public SettingsLanguageView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
