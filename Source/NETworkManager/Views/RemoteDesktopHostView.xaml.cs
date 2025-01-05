﻿using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views;

public partial class RemoteDesktopHostView
{
    private readonly RemoteDesktopHostViewModel _viewModel = new(DialogCoordinator.Instance);

    private bool _loaded;

    public RemoteDesktopHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu) menu.DataContext = _viewModel;
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            _viewModel.ConnectProfileCommand.Execute(null);
    }

    public async void AddTab(string host)
    {
        // Wait for the interface to load, before displaying the dialog to connect a new Profile... 
        // MahApps will throw an exception... 
        while (!_loaded)
            await Task.Delay(250);

        _viewModel.AddTab(host);
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }
}