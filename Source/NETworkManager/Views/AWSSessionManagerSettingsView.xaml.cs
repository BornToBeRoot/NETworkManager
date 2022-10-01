using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views
{
    public partial class AWSSessionManagerSettingsView
    {
        private readonly AWSSessionManagerSettingsViewModel _viewModel = new(DialogCoordinator.Instance);

        public AWSSessionManagerSettingsView()
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

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EditAWSProfile();
        }
    }
}
