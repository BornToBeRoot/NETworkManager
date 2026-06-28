using System;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class ImportCsvFileChildWindow
{
    public ImportCsvFileChildWindow()
    {
        InitializeComponent();
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate { TextBoxFilePath.Focus(); }));
    }

    /// <summary>
    ///     Set the file from drag and drop.
    /// </summary>
    private void TextBoxFilePath_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (files != null && DataContext is ImportCsvFileViewModel viewModel)
            viewModel.SetFilePathFromDragDrop(files[0]);
    }

    /// <summary>
    ///     Method to override the drag over effect.
    /// </summary>
    private void TextBoxFilePath_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }
}
