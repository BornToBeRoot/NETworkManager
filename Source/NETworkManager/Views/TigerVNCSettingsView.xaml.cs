using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class TigerVNCSettingsView
{
    private readonly TigerVNCSettingsViewModel _viewModel = new(DialogCoordinator.Instance);

    public TigerVNCSettingsView()
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