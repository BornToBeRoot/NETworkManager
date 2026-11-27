using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    // Widgets are created when they are shown the first time (e.g. a hidden IP API widget sends no request)
    private NetworkConnectionWidgetView _networkConnectionWidgetView;
    private InternetLatencyWidgetView _internetLatencyWidgetView;
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

    // Width a widget needs to be shown side by side with another one (2/3 + 1/3, see ArrangeRow): its header and
    // columns fit without being squeezed. Otherwise the widgets of the row are shown one below the other with the full
    // width. Not a MinWidth of the widget: in its own row it gets the width of the window.
    private const double SpeedTestMinWidth = 600; // Three columns (ISP / server, download, upload)
    private const double InternetLatencyMinWidth = 280; // Dot, name, value and a short chart per line
    private const double IPGeolocationMinWidth = 600; // Location and network side by side
    private const double DNSResolverMinWidth = 400; // Header: title, "Reloaded at ...", copy and refresh buttons

    /// <summary>
    ///     Rows shown side by side (speed test / internet latency, IP geolocation / DNS resolver) at the current width:
    ///     the layout is only arranged again when this changes, not on every resize.
    /// </summary>
    private (bool SpeedTestRow, bool IPApiRow) _sideBySide;

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
        UpdateInternetLatency();

        // Side by side or one below the other: only when a row changes, not on every resize
        StackPanelWidgets.SizeChanged += (_, e) =>
        {
            if (e.WidthChanged && GetSideBySide(e.NewSize.Width) != _sideBySide)
                ApplyLayout();
        };

        // The settings are changed while the dashboard is hidden (settings view) - check again when it is shown
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        // Check on first open (the widgets don't check when they are created). Later re-opens and widgets shown the
        // first time (the setting marks the dashboard as outdated) are covered by OnViewVisible().
        CheckWidgets();
    }

    public void OnViewVisible()
    {
        _isViewVisible = true;

        _viewModel.OnViewVisible();

        // The widgets may have been shown or hidden in the settings
        ApplyWidgetSettings();
        UpdateInternetLatency();

        CheckWidgetsIfOutdated();
    }

    /// <summary>
    ///     Called by the main window when it is shown again (from the tray or minimized).
    /// </summary>
    public void OnMainWindowShown()
    {
        if (!_isViewVisible)
            return;

        CheckWidgetsIfOutdated();
        UpdateInternetLatency();
    }

    /// <summary>
    ///     Called by the main window when it is minimized or hidden to the tray: the internet latency widget stops
    ///     pinging (no live values in the background).
    /// </summary>
    public void OnMainWindowHidden()
    {
        UpdateInternetLatency();
    }

    public void OnViewHide()
    {
        _isViewVisible = false;

        _viewModel.OnViewHide();

        UpdateInternetLatency();
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

        // Resolve the host names again and remove the history (another route, e.g. a VPN) - only while it runs, it
        // starts fresh anyway when the dashboard is shown again
        _internetLatencyWidgetView?.Restart();
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
        _internetLatencyWidgetView = ShowWidget(ContentControlInternetLatency,
            settings.Dashboard_ShowInternetLatencyWidget, _internetLatencyWidgetView);
        _speedTestWidgetView = ShowWidget(ContentControlSpeedTest, settings.Dashboard_ShowSpeedTestWidget,
            _speedTestWidgetView);
        _ipApiIPGeolocationWidgetView = ShowWidget(ContentControlIPApiIPGeolocation,
            settings.Dashboard_ShowIPGeolocationWidget, _ipApiIPGeolocationWidgetView);
        _ipApiDNSResolverWidgetView = ShowWidget(ContentControlIPApiDNSResolver,
            settings.Dashboard_ShowDNSResolverWidget, _ipApiDNSResolverWidgetView);

        ApplyLayout();
    }

    /// <summary>
    ///     Arranges the visible widgets: side by side or one below the other (position setting, width of the dashboard)
    ///     and the gaps between them.
    /// </summary>
    private void ApplyLayout()
    {
        var settings = SettingsManager.Current;

        _sideBySide = GetSideBySide(StackPanelWidgets.ActualWidth);

        // Next to the speed test (right, 1/3) if the dashboard is wide enough, otherwise its own row below the speed
        // test. Own row: above the speed test (below the network connection).
        var isNextToSpeedTest = settings.Dashboard_InternetLatencyPosition == InternetLatencyPosition.OwnRow
            ? ArrangeRow(GridSpeedTestInternetLatency, ContentControlInternetLatency, ContentControlSpeedTest, false)
            : ArrangeRow(GridSpeedTestInternetLatency, ContentControlSpeedTest, ContentControlInternetLatency,
                _sideBySide.SpeedTestRow);

        // Next to the speed test the targets are always one below the other (not decided by the width of the widget:
        // 1/3 of a maximized window is wide enough for them side by side)
        _internetLatencyWidgetView?.SetNextToOtherWidget(isNextToSpeedTest);

        // IP geolocation (2/3) and DNS resolver (1/3) side by side if the dashboard is wide enough (the DNS resolver
        // is too narrow otherwise), the remaining one spans the row
        ArrangeRow(GridIPApi, ContentControlIPApiIPGeolocation, ContentControlIPApiDNSResolver, _sideBySide.IPApiRow);

        // Gap of 10 between the visible widgets (not below the last one, the scroll viewer has a margin of 10)
        var visibleWidgets = new FrameworkElement[]
                { ContentControlNetworkConnection, GridSpeedTestInternetLatency, GridIPApi }
            .Where(x => x.Visibility == Visibility.Visible)
            .ToList();

        for (var i = 0; i < visibleWidgets.Count; i++)
            visibleWidgets[i].Margin = new Thickness(0, 0, 0, i < visibleWidgets.Count - 1 ? 10 : 0);

        EmptyStateMessageNoWidgets.Visibility = visibleWidgets.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Which rows can be shown side by side at the width of the dashboard.
    /// </summary>
    /// <param name="width">Width of the dashboard (the widgets).</param>
    private static (bool SpeedTestRow, bool IPApiRow) GetSideBySide(double width)
    {
        return (FitsSideBySide(width, SpeedTestMinWidth, InternetLatencyMinWidth),
            FitsSideBySide(width, IPGeolocationMinWidth, DNSResolverMinWidth));
    }

    /// <summary>
    ///     Both widgets fit side by side: the first one in 2/3 and the second one in 1/3 of the width (columns
    ///     <c>* 10 * 10 *</c> of the row).
    /// </summary>
    /// <param name="width">Width of the row.</param>
    /// <param name="firstMinWidth">Width the first (left) widget needs.</param>
    /// <param name="secondMinWidth">Width the second (right) widget needs.</param>
    private static bool FitsSideBySide(double width, double firstMinWidth, double secondMinWidth)
    {
        var column = (width - 20) / 3;

        return 2 * column + 10 >= firstMinWidth && column >= secondMinWidth;
    }

    /// <summary>
    ///     Arranges two widgets of a row (grid with 5 columns: 2/3, gap, 1/3 and 2 rows): side by side if both are shown
    ///     and <paramref name="sideBySide" />, otherwise one below the other with the full width (gap of 10). A hidden
    ///     widget leaves no space; the row is hidden if both are.
    /// </summary>
    /// <param name="row">Grid of the row.</param>
    /// <param name="first">Left (2/3) or upper widget.</param>
    /// <param name="second">Right (1/3) or lower widget.</param>
    /// <param name="sideBySide">Show the widgets side by side (if both are shown).</param>
    /// <returns><c>true</c> if the widgets are shown side by side.</returns>
    private static bool ArrangeRow(FrameworkElement row, FrameworkElement first, FrameworkElement second,
        bool sideBySide)
    {
        var showFirst = first.Visibility == Visibility.Visible;
        var showSecond = second.Visibility == Visibility.Visible;

        row.Visibility = showFirst || showSecond ? Visibility.Visible : Visibility.Collapsed;

        var isSideBySide = sideBySide && showFirst && showSecond;

        Grid.SetRow(first, 0);
        Grid.SetColumn(first, 0);
        Grid.SetColumnSpan(first, isSideBySide ? 3 : 5);
        first.Margin = new Thickness(0);

        var isBelow = showFirst && !isSideBySide;

        Grid.SetRow(second, isBelow ? 1 : 0);
        Grid.SetColumn(second, isSideBySide ? 4 : 0);
        Grid.SetColumnSpan(second, isSideBySide ? 1 : 5);
        second.Margin = new Thickness(0, isBelow ? 10 : 0, 0, 0);

        return isSideBySide;
    }

    /// <summary>
    ///     Starts the pings of the internet latency widget while it is shown and enabled, the dashboard is the selected
    ///     application and the main window is shown (not minimized, not in the tray); stops them otherwise.
    /// </summary>
    private void UpdateInternetLatency()
    {
        var settings = SettingsManager.Current;

        if (_isViewVisible && IsMainWindowShown() && settings.Dashboard_ShowInternetLatencyWidget &&
            settings.Dashboard_CheckInternetLatency)
            _internetLatencyWidgetView?.Start();
        else
            _internetLatencyWidgetView?.Stop();
    }

    /// <summary>
    ///     The main window is shown (not minimized, not hidden to the tray).
    /// </summary>
    private static bool IsMainWindowShown()
    {
        return Application.Current?.MainWindow is not { } window ||
               (window.IsVisible && window.WindowState != WindowState.Minimized);
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
    ///     Settings used by the checks: a change marks them as outdated (checked when the dashboard is shown again, e.g.
    ///     the IP geolocation right after its check was enabled). Other dashboard settings (e.g. the speed test widget)
    ///     keep the recheck interval.
    /// </summary>
    private static readonly HashSet<string> CheckSettings =
    [
        nameof(SettingsInfo.Dashboard_PublicIPv4Address),
        nameof(SettingsInfo.Dashboard_PublicIPv6Address),
        nameof(SettingsInfo.Dashboard_CheckPublicIPAddress),
        nameof(SettingsInfo.Dashboard_UseCustomPublicIPv4AddressAPI),
        nameof(SettingsInfo.Dashboard_CustomPublicIPv4AddressAPI),
        nameof(SettingsInfo.Dashboard_UseCustomPublicIPv6AddressAPI),
        nameof(SettingsInfo.Dashboard_CustomPublicIPv6AddressAPI),
        nameof(SettingsInfo.Dashboard_ShowNetworkConnectionWidget),
        nameof(SettingsInfo.Dashboard_CheckIPApiIPGeolocation),
        nameof(SettingsInfo.Dashboard_ShowIPGeolocationWidget),
        nameof(SettingsInfo.Dashboard_CheckIPApiDNSResolver),
        nameof(SettingsInfo.Dashboard_ShowDNSResolverWidget)
    ];

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // The "Enable" button of an IP API widget (dashboard visible) checks the widget itself, the others are up to date
        var isEnabledInWidget = _isViewVisible && e.PropertyName is nameof(SettingsInfo.Dashboard_CheckIPApiIPGeolocation)
            or nameof(SettingsInfo.Dashboard_CheckIPApiDNSResolver);

        if (e.PropertyName != null && CheckSettings.Contains(e.PropertyName) && !isEnabledInWidget)
            _isOutdated = true;

        // Internet latency: start / stop right away (e.g. the "Enable" button of the widget, or hidden in the settings)
        if (e.PropertyName is nameof(SettingsInfo.Dashboard_ShowInternetLatencyWidget)
            or nameof(SettingsInfo.Dashboard_CheckInternetLatency))
            UpdateInternetLatency();

        // A hidden speed test widget doesn't measure (no data is transferred in the background): cancel it right away,
        // the dashboard is hidden while the settings are open
        if (e.PropertyName == nameof(SettingsInfo.Dashboard_ShowSpeedTestWidget) &&
            !SettingsManager.Current.Dashboard_ShowSpeedTestWidget)
            _speedTestWidgetView?.Cancel();
    }
}