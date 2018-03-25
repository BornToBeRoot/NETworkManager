using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PuTTYSettingsView : UserControl
    {
        PuTTYSettingsViewModel viewModel = new PuTTYSettingsViewModel();

        public PuTTYSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
