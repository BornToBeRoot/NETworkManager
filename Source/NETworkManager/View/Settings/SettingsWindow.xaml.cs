using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModel.Settings;
using System.ComponentModel;
using System.Windows.Input;

namespace NETworkManager.View.Settings
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private SettingsViewModel viewModel = new SettingsViewModel(DialogCoordinator.Instance);

        public bool HotKeysChanged
        {
            get { return viewModel.HotKeysChanged; }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            viewModel.OnClosing();
        }

        private void MetroWindowSettings_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}