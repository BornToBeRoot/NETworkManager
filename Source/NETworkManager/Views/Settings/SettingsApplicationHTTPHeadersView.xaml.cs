using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class SettingsApplicationHTTPHeadersView : UserControl
    {
        SettingsApplicationHTTPHeadersViewModel viewModel = new SettingsApplicationHTTPHeadersViewModel();

        public SettingsApplicationHTTPHeadersView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
