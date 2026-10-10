using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.Controls;
using NETworkManager.Models.RDAP;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class RDAPView : IDragablzTabItem
{
    private readonly RDAPViewModel _viewModel;

    public RDAPView(Guid tabId, string query = null, RDAPQueryType? queryType = null)
    {
        InitializeComponent();

        _viewModel = new RDAPViewModel(tabId, query, queryType);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public void CloseTab()
    {
        // Detach the app-lifetime handler so this transient tab view (and its view model)
        // can be collected after the tab is closed.
        Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;

        _viewModel.OnClose();
    }

    private void UserControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnClose();
    }

    // Fix mouse wheel when using DataGrid (https://stackoverflow.com/a/16235785/4986782)
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;

        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);

        e.Handled = true;
    }
}
