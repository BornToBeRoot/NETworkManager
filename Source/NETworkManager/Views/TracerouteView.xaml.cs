using System;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class TracerouteView : IDragablzTabItem
{
    private readonly TracerouteViewModel _viewModel;

    // Remembers the user-resized map height across collapse/expand, since the row is
    // switched to GridLength.Auto while collapsed so it doesn't reserve unused space.
    private double _mapRowHeight = 300;

    private const double MapRowMinHeight = 120;

    public TracerouteView(Guid tabId, string host = null)
    {
        InitializeComponent();

        _viewModel = new TracerouteViewModel(tabId, host);

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

        // The Expanded/Collapsed events only fire on a state change, so sync the row
        // height once for the case where the map starts collapsed (via settings).
        if (!MapExpander.IsExpanded)
        {
            MapRow.Height = GridLength.Auto;
            MapRow.MinHeight = 0;
        }
    }

    private void MapExpander_Expanded(object sender, RoutedEventArgs e)
    {
        MapRow.Height = new GridLength(_mapRowHeight);
        MapRow.MinHeight = MapRowMinHeight;
    }

    private void MapExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (MapRow.Height.IsAbsolute)
            _mapRowHeight = MapRow.Height.Value;

        // MinHeight is a separate constraint from Height and would otherwise still reserve
        // space for the (now hidden) map content even though Height is set to Auto.
        MapRow.Height = GridLength.Auto;
        MapRow.MinHeight = 0;
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