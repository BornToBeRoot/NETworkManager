using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SettingsImportExportView
    {
        private readonly SettingsImportExportViewModel _viewModel = new SettingsImportExportViewModel(DialogCoordinator.Instance);

        public SettingsImportExportView()
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

        private void txtImportFilePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetImportLocationFilePathFromDragDrop(files[0]);
        }

        private void txtImportFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
