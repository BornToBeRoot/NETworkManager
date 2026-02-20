using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SettingsProfilesView
{
    private readonly SettingsProfilesViewModel _viewModel = new();

    public SettingsProfilesView()
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

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.EditProfileFileCommand.CanExecute(null))
            _viewModel.EditProfileFileCommand.Execute(null);
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
}