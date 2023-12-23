using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNTPLookupView
{
    private readonly SNTPLookupViewModel _viewModel;

    public SNTPLookupView(Guid tabId)
    {
        InitializeComponent();

        _viewModel = new SNTPLookupViewModel(DialogCoordinator.Instance, tabId);

        DataContext = _viewModel;
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
}