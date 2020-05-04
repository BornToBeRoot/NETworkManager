using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;

namespace NETworkManager.Views
{
    public partial class PuTTYSettingsView
    {
        private readonly PuTTYSettingsViewModel _viewModel = new PuTTYSettingsViewModel(DialogCoordinator.Instance);

        public PuTTYSettingsView()
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
                _viewModel.SetApplicationFilePathFromDragDrop(files[0]);
        }

        private void TextBoxApplicationFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void TextBoxSSHPrivateKeyFilePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetSSHPrivateKeyFilePathFromDragDrop(files[0]);
        }

        private void TextBoxSSHPrivateKeyFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
