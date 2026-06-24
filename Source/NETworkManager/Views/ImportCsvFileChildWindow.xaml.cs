using System;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class ImportCsvFileChildWindow
{
    public ImportCsvFileChildWindow(Window parentWindow)
    {
        InitializeComponent();

        // Set the width and height of the child window based on the parent window size
        ChildWindowMaxWidth = 850;
        ChildWindowMaxHeight = 650;
        ChildWindowWidth = parentWindow.ActualWidth * 0.85;

        // Update the size of the child window when the parent window is resized
        parentWindow.SizeChanged += (_, _) => { ChildWindowWidth = parentWindow.ActualWidth * 0.85; };
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
