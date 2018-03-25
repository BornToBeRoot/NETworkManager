using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersSettingsView : UserControl
    {
        HTTPHeadersSettingsViewModel viewModel = new HTTPHeadersSettingsViewModel();

        public HTTPHeadersSettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
