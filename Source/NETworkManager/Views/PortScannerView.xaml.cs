using System;
using System.Windows;
using NETworkManager.ViewModels;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views;

public partial class PortScannerView
{
    private readonly PortScannerViewModel _viewModel;

    public PortScannerView(Guid tabId, string host = null, string ports = null)
    {
        InitializeComponent();

        _viewModel = new PortScannerViewModel(DialogCoordinator.Instance, tabId, host, ports);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnClose();
    }

    public void CloseTab()
    {
        _viewModel.OnClose();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        if (sender is DataGridColumnHeader columnHeader)
            _viewModel.SortResultByPropertyName(columnHeader.Column.SortMemberPath);
    }
}