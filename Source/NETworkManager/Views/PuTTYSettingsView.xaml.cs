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

        private void TextBoxPuTTYLocation_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetFilePathFromDragDrop(files[0]);
        }

        private void TextBoxPuTTYLocation_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
