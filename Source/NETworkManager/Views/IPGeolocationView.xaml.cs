using System;
using System.Windows;
using NETworkManager.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPGeolocationView : IDragablzTabItem
{
    private readonly IPGeolocationViewModel _viewModel;

    public IPGeolocationView(Guid tabId, string domain = null)
    {
        InitializeComponent();

        _viewModel = new IPGeolocationViewModel(tabId, domain);

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
}