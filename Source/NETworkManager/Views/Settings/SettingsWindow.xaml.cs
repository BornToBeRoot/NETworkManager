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
        private SettingsViewModel viewModel = new SettingsViewModel();

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }              

        private void MetroWindowSettings_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}