using NETworkManager.ViewModels;
using System.Windows;

namespace NETworkManager.Views;

public partial class SettingsSettingsView
{
    private readonly SettingsSettingsViewModel _viewModel = new();

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

    private void TextBoxLocation_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (files is { Length: > 0 })
            _viewModel.SetLocationPathFromDragDrop(files[0]);
    }

    private void TextBoxLocation_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
          ? DragDropEffects.Copy
          : DragDropEffects.None;
        e.Handled = true;
    }
}
