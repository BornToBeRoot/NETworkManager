using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
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
