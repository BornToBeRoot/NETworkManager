using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsSettingsView : UserControl
    {
        SettingsSettingsViewModel viewModel = new SettingsSettingsViewModel(DialogCoordinator.Instance);

        public SettingsSettingsView()
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

        private void txtLocation_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                viewModel.SetLocationPathFromDragDrop(files[0]);
            }
        }

        private void txtLocation_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
