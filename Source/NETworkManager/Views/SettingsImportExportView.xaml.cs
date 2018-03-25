using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Settings;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsImportExportView : UserControl
    {
        SettingsImportExportViewModel viewModel = new SettingsImportExportViewModel(DialogCoordinator.Instance);

        public SettingsImportExportView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel.CloseAction == null)
                viewModel.CloseAction = new System.Action(Window.GetWindow(this).Close);
        }

        public void SaveAndCheckSettings()
        {
            viewModel.SaveAndCheckSettings();
        }
    }
}
