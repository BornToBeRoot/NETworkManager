using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SettingsProfilesView
    {
        private readonly SettingsProfilesViewModel _viewModel = new SettingsProfilesViewModel(DialogCoordinator.Instance);

        public SettingsProfilesView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void TextBoxLocation_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null)
                _viewModel.SetLocationPathFromDragDrop(files[0]);
        }

        private void TextBoxLocation_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        public void OnVisible()
        {
            _viewModel.SaveAndCheckSettings();
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.EditProfileFileCommand.Execute(null);
        }
    }
}
