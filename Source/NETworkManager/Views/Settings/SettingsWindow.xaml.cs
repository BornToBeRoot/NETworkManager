using MahApps.Metro.Controls;
using NETworkManager.ViewModels.Settings;
using System.Windows.Input;

namespace NETworkManager.Views.Settings
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private SettingsViewModel viewModel;

        public SettingsWindow() : this(ApplicationViewManager.Name.None)
        {

        }

        public SettingsWindow(ApplicationViewManager.Name selectedApplicationName)
        {
            InitializeComponent();

            viewModel = new SettingsViewModel(selectedApplicationName);
            DataContext = viewModel;
        }

        private void MetroWindowSettings_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}