using System;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DNSLookupView : IDragablzTabItem
{
    private readonly DNSLookupViewModel _viewModel;

    public DNSLookupView(Guid tabId, string host = null)
    {
        InitializeComponent();

        _viewModel = new DNSLookupViewModel(tabId, host);

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

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnClose();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }
}