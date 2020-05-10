using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;

namespace NETworkManager.Views
{
    public partial class PuTTYSettingsView
    {
        /// <summary>
        /// Variable to hold an instance of the view model.
        /// </summary>
        private readonly PuTTYSettingsViewModel _viewModel = new PuTTYSettingsViewModel(DialogCoordinator.Instance);

        /// <summary>
        /// Create a new instance of <see cref="PuTTYSettingsView"/>.
        /// </summary>
        public PuTTYSettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        /// <summary>
        /// Set the file from drag and drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxApplicationFilePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetApplicationFilePathFromDragDrop(files[0]);
        }

        /// <summary>
        /// Method to override the drag over effect.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxApplicationFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        /// <summary>
        /// Set the file from drag and drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxPrivateKeyFilePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetPrivateKeyFilePathFromDragDrop(files[0]);
        }

        /// <summary>
        /// Method to override the drag over effect.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxPrivateKeyFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        /// <summary>
        /// Set the folder from drag and drop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxLogPathFolderPath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var folder = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (folder != null)
                _viewModel.SetLogPathFolderPathFromDragDrop(folder[0]);
        }

        /// <summary>
        /// Method to override the drag over effect.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxLogPathFolderPath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }
}
