using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsUpdateView : UserControl
    {
        SettingsUpdateViewModel viewModel = new SettingsUpdateViewModel();

        public SettingsUpdateView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
