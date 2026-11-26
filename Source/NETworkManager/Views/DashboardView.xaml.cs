using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Settings;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    // Widgets are created when they are shown the first time (e.g. a hidden IP API widget sends no request)
    private NetworkConnectionWidgetView _networkConnectionWidgetView;
    private IPApiIPGeolocationWidgetView _ipApiIPGeolocationWidgetView;
    private IPApiDNSResolverWidgetView _ipApiDNSResolverWidgetView;
    private SpeedTestWidgetView _speedTestWidgetView;

    /// <summary>
    ///     The dashboard is the selected application (see <see cref="OnViewVisible" /> / <see cref="OnViewHide" />).
    /// </summary>
    private bool _isViewVisible = true;

    /// <summary>
    ///     The network or a setting of a check changed while the dashboard was hidden: check again when it is shown.
    /// </summary>
    private bool _isOutdated;

    /// <summary>
    ///     Start of the last check, see <see cref="GlobalStaticConfiguration.Dashboard_RecheckInterval" />.
    /// </summary>
    private DateTime _lastCheck;

    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Show the widgets
        ApplyWidgetSettings();

        // The settings are changed while the dashboard is hidden (settings view) - check again when it is shown
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        // Check on first open (the IP API widgets are already checking since they were created). Later re-opens are
        // covered by OnViewVisible().
        CheckWidgets();
    }

    public void OnViewVisible()
    {
        _isViewVisible = true;

        _viewModel.OnViewVisible();

        // The widgets may have been shown or hidden in the settings
        ApplyWidgetSettings();

        CheckWidgetsIfOutdated();
    }

    /// <summary>
    ///     Called by the main window when it is shown again (from the tray or minimized).
    /// </summary>
    public void OnMainWindowShown()
    {
        if (_isViewVisible)
            CheckWidgetsIfOutdated();
    }

    public void OnViewHide()
    {
        _isViewVisible = false;

        _viewModel.OnViewHide();
    }

    /// <summary>
    ///     Called by the main window when the network has changed (addresses, adapter connected or disconnected).
    /// </summary>
    /// <param name="isMainWindowShown">The main window is shown (not in the tray or minimized). Otherwise the check runs
    ///     when the window is shown again (the status window checks the network connection meanwhile).</param>
    public void OnNetworkChanged(bool isMainWindowShown)
    {
        if (_isViewVisible && isMainWindowShown)
            CheckWidgets();
        else
            _isOutdated = true;
    }

    /// <summary>
    ///     Checks the widgets again if something changed, or if the last check is older than the recheck interval (e.g.
    ///     the public IP address may have changed) - not on every switch between the applications.
    /// </summary>
    private void CheckWidgetsIfOutdated()
    {
        if (_isOutdated || DateTime.Now - _lastCheck >= GlobalStaticConfiguration.Dashboard_RecheckInterval)
            CheckWidgets();
    }

    /// <summary>
    ///     Checks the network connection and the IP API widgets (the public IP address, and with it the IP geolocation
    ///     and the DNS resolver, may have changed).
    /// </summary>
    private void CheckWidgets()
    {
        _isOutdated = false;
        _lastCheck = DateTime.Now;

        // Hidden or not created yet: checked when it is shown (a changed setting marks the data as outdated, the widget
        // is created in OnViewVisible). A widget whose check is disabled sends no request.
        var settings = SettingsManager.Current;

        if (settings.Dashboard_ShowNetworkConnectionWidget)
            _networkConnectionWidgetView?.Check();

        if (settings.Dashboard_ShowIPGeolocationWidget)
            _ipApiIPGeolocationWidgetView?.Check();

        if (settings.Dashboard_ShowDNSResolverWidget)
            _ipApiDNSResolverWidgetView?.Check();
    }

    /// <summary>
    ///     Shows / hides the widgets (settings <c>Dashboard_Show*Widget</c>) and creates them when they are shown the
    ///     first time.
    /// </summary>
    private void ApplyWidgetSettings()
    {
        var settings = SettingsManager.Current;

        _networkConnectionWidgetView = ShowWidget(ContentControlNetworkConnection,
            settings.Dashboard_ShowNetworkConnectionWidget, _networkConnectionWidgetView);
        _speedTestWidgetView = ShowWidget(ContentControlSpeedTest, settings.Dashboard_ShowSpeedTestWidget,
            _speedTestWidgetView);
        _ipApiIPGeolocationWidgetView = ShowWidget(ContentControlIPApiIPGeolocation,
            settings.Dashboard_ShowIPGeolocationWidget, _ipApiIPGeolocationWidgetView);
        _ipApiDNSResolverWidgetView = ShowWidget(ContentControlIPApiDNSResolver,
            settings.Dashboard_ShowDNSResolverWidget, _ipApiDNSResolverWidgetView);

        // IP geolocation (2/3) and DNS resolver (1/3) in one row, the remaining one spans the row
        var showIPGeolocation = settings.Dashboard_ShowIPGeolocationWidget;
        var showDNSResolver = settings.Dashboard_ShowDNSResolverWidget;

        GridIPApi.Visibility = showIPGeolocation || showDNSResolver ? Visibility.Visible : Visibility.Collapsed;

        Grid.SetColumnSpan(ContentControlIPApiIPGeolocation, showDNSResolver ? 3 : 5);
        Grid.SetColumn(ContentControlIPApiDNSResolver, showIPGeolocation ? 4 : 0);
        Grid.SetColumnSpan(ContentControlIPApiDNSResolver, showIPGeolocation ? 1 : 5);

        // Gap of 10 between the visible widgets (not below the last one, the scroll viewer has a margin of 10)
        var visibleWidgets = new FrameworkElement[]
                { ContentControlNetworkConnection, ContentControlSpeedTest, GridIPApi }
            .Where(x => x.Visibility == Visibility.Visible)
            .ToList();

        for (var i = 0; i < visibleWidgets.Count; i++)
            visibleWidgets[i].Margin = new Thickness(0, 0, 0, i < visibleWidgets.Count - 1 ? 10 : 0);

        EmptyStateMessageNoWidgets.Visibility = visibleWidgets.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Shows or hides a widget. The view of the widget is created when it is shown the first time.
    /// </summary>
    /// <param name="contentControl">Content control of the widget.</param>
    /// <param name="show">Show the widget.</param>
    /// <param name="view">View of the widget, <c>null</c> if not created yet.</param>
    /// <typeparam name="T">Type of the view.</typeparam>
    /// <returns>View of the widget, <c>null</c> if it was never shown.</returns>
    private static T ShowWidget<T>(ContentControl contentControl, bool show, T view) where T : UserControl, new()
    {
        contentControl.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        if (!show || view != null)
            return view;

        view = new T();
        contentControl.Content = view;

        return view;
    }

    /// <summary>
    ///     Settings used by the network connection check: a change marks it as outdated (checked when the dashboard is
    ///     shown again). Other dashboard settings (e.g. the other widgets) keep the recheck interval.
    /// </summary>
    private static readonly HashSet<string> NetworkConnectionSettings =
    [
        nameof(SettingsInfo.Dashboard_PublicIPv4Address),
        nameof(SettingsInfo.Dashboard_PublicIPv6Address),
        nameof(SettingsInfo.Dashboard_CheckPublicIPAddress),
        nameof(SettingsInfo.Dashboard_UseCustomPublicIPv4AddressAPI),
        nameof(SettingsInfo.Dashboard_CustomPublicIPv4AddressAPI),
        nameof(SettingsInfo.Dashboard_UseCustomPublicIPv6AddressAPI),
        nameof(SettingsInfo.Dashboard_CustomPublicIPv6AddressAPI),
        nameof(SettingsInfo.Dashboard_ShowNetworkConnectionWidget)
    ];

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null && NetworkConnectionSettings.Contains(e.PropertyName))
            _isOutdated = true;

        // A hidden speed test widget doesn't measure (no data is transferred in the background): cancel it right away,
        // the dashboard is hidden while the settings are open
        if (e.PropertyName == nameof(SettingsInfo.Dashboard_ShowSpeedTestWidget) &&
            !SettingsManager.Current.Dashboard_ShowSpeedTestWidget)
            _speedTestWidgetView?.Cancel();
    }
}