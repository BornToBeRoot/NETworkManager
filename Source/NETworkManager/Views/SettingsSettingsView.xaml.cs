using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;

namespace NETworkManager.Views
{
    public partial class SettingsSettingsView
    {
        private readonly SettingsSettingsViewModel _viewModel = new SettingsSettingsViewModel(DialogCoordinator.Instance);

        public SettingsSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CloseAction != null)
                return;

            var window = Window.GetWindow(this);

            if (window != null)
                _viewModel.CloseAction = window.Close;
        }

        public void SaveAndCheckSettings()
        {
            _viewModel.SaveAndCheckSettings();
        }

        private void txtLocation_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetLocationPathFromDragDrop(files[0]);
        }

        private void txtLocation_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
