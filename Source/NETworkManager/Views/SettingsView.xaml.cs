using NETworkManager.Models.Application;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsView
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView(Name applicationName)
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel(applicationName);

            DataContext = _viewModel;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        public void ChangeSettingsView(Name name)
        {
            _viewModel.ChangeSettingsView(name);

            // Scroll into view
            ListBoxSettings.ScrollIntoView(_viewModel.SelectedSettingsView);
        }

        public void Refresh()
        {
            ProfilesView.Refresh();
        }
    }
}