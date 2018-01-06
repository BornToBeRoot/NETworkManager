using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;
using System;

namespace NETworkManager.Views.Settings
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private SettingsViewModel viewModel;

        public SettingsView(ApplicationViewManager.Name applicationName)
        {
            InitializeComponent();
            viewModel = new SettingsViewModel(applicationName);

            DataContext = viewModel;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        public void ChangeSettingsView(ApplicationViewManager.Name name)
        {
            viewModel.ChangeSettingsView(name);

            // Scroll into view
            listBoxSettings.ScrollIntoView(viewModel.SelectedSettingsView);
        }
    }
}