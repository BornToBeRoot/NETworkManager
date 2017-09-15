using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private SettingsViewModel viewModel;

        public SettingsView() : this(ApplicationViewManager.Name.None)
        {

        }

        public SettingsView(ApplicationViewManager.Name selectedApplicationName)
        {
            InitializeComponent();

            viewModel = new SettingsViewModel(selectedApplicationName);
            DataContext = viewModel;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        //private void MetroWindowSettings_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //        Close();
        //}
    }
}