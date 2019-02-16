using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;

namespace NETworkManager.Views
{
    public partial class PowerShellSettingsView
    {
        private readonly PowerShellSettingsViewModel _viewModel = new PowerShellSettingsViewModel(DialogCoordinator.Instance);

        public PowerShellSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void TextBoxApplicationFilePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetFilePathFromDragDrop(files[0]);
        }

        private void TextBoxApplicationFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
