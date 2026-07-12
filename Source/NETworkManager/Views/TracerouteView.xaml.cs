using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.Controls;
using NETworkManager.Settings;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class TracerouteView : IDragablzTabItem
{
    private readonly TracerouteViewModel _viewModel;

    private const double MapRowMinHeight = 120;

    // Fraction (not a fixed pixel value) so the cap in RouteMapGrid_SizeChanged scales with the
    // window size.
    private const double MapRowMaxHeightFraction = 2.0 / 3.0;

    public TracerouteView(Guid tabId, string host = null)
    {
        InitializeComponent();

        _viewModel = new TracerouteViewModel(tabId, host);

        DataContext = _viewModel;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

        // GridSplitter marks the event Handled once it's done resizing, so a plain XAML handler
        // would never run - handledEventsToo:true is required.
        MapGridSplitter.AddHandler(MouseLeftButtonUpEvent,
            new MouseButtonEventHandler(MapGridSplitter_MouseLeftButtonUp), true);
    }

    public void CloseTab()
    {
        // Detach so this transient tab view can be garbage-collected after the tab is closed.
        Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;

        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;

        _viewModel.OnClose();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();

        // PropertyChanged only fires on an actual change, so sync the row height once here.
        if (_viewModel.ExpandMapView)
            ExpandMapRow();
        else
            CollapseMapRowWithoutCapturingHeight();
    }

    private void RouteMapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Caps how far MapGridSplitter can grow MapRow, so the results grid above always keeps
        // some space too.
        MapRow.MaxHeight = Math.Max(MapRowMinHeight, e.NewSize.Height * MapRowMaxHeightFraction);
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TracerouteViewModel.ExpandMapView))
            return;

        if (_viewModel.ExpandMapView)
            ExpandMapRow();
        else
            CollapseMapRow();
    }

    private void ExpandMapRow()
    {
        MapRow.Height = new GridLength(SettingsManager.Current.Traceroute_MapHeight);
        MapRow.MinHeight = MapRowMinHeight;
    }

    private void CollapseMapRow()
    {
        // Only call this once MapRow has actually been expanded - otherwise MapRow.Height is
        // still just its XAML-declared default (300), which would clobber the persisted
        // Traceroute_MapHeight. That's why UserControl_Loaded uses
        // CollapseMapRowWithoutCapturingHeight for a tab that starts collapsed.
        PersistMapRowHeight();

        CollapseMapRowWithoutCapturingHeight();
    }

    private void CollapseMapRowWithoutCapturingHeight()
    {
        // MinHeight is a separate constraint from Height and must be reset too, or it would
        // still reserve space MapView's collapsed toggle button doesn't need.
        MapRow.Height = GridLength.Auto;
        MapRow.MinHeight = 0;
    }

    // GridSplitter has no DragCompleted event (unlike a Thumb) - MouseLeftButtonUp is the
    // closest equivalent.
    private void MapGridSplitter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        PersistMapRowHeight();
    }

    private void PersistMapRowHeight()
    {
        if (MapRow.Height.IsAbsolute)
            SettingsManager.Current.Traceroute_MapHeight = MapRow.Height.Value;
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