using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsGeneralLanguageView : UserControl
    {
        SettingsGeneralLanguageViewModel viewModel = new SettingsGeneralLanguageViewModel();

        public SettingsGeneralLanguageView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
