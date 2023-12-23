using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class AboutView
{
    private readonly AboutViewModel _viewModel = new();

    public AboutView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu) menu.DataContext = _viewModel;
    }

    // Fix mouse wheel when using DataGrid (https://stackoverflow.com/a/16235785/4986782)
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scv = (ScrollViewer)sender;

        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);

        e.Handled = true;
    }
}