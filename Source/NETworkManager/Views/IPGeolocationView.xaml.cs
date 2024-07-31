using System;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPGeolocationView : IDragablzTabItem
{
    private readonly IPGeolocationViewModel _viewModel;

    public IPGeolocationView(Guid tabId, string domain = null)
    {
        InitializeComponent();

        _viewModel = new IPGeolocationViewModel(DialogCoordinator.Instance, tabId, domain);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public void CloseTab()
    {
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