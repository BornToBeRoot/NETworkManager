using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private ApplicationViewManager.Name _selectedApplicationName;
        public ApplicationViewManager.Name SelectedApplicationName
        {
            get { return _selectedApplicationName; }
            set
            {
                viewModel.SelectedApplicationName = value;

                if (value == _selectedApplicationName)
                    return;

                _selectedApplicationName = value;
            }
        }

        private SettingsViewModel viewModel = new SettingsViewModel();

        public SettingsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}