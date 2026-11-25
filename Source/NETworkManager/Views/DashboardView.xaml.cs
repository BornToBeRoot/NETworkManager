using System;
using System.ComponentModel;
using NETworkManager.Settings;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    private readonly NetworkConnectionWidgetView _networkConnectionWidgetView = new();
    private readonly IPApiIPGeolocationWidgetView _ipApiIPGeolocationWidgetView = new();
    private readonly IPApiDNSResolverWidgetView _ipApiDNSResolverWidgetView = new();
    private readonly SpeedTestWidgetView _speedTestWidgetView = new();

    /// <summary>
    ///     The dashboard is the selected application (see <see cref="OnViewVisible" /> / <see cref="OnViewHide" />).
    /// </summary>
    private bool _isViewVisible = true;

    /// <summary>
    ///     The network or a dashboard setting changed while the dashboard was hidden: check again when it is shown.
    /// </summary>
    private bool _isOutdated;

    /// <summary>
    ///     Start of the last network connection check, see <see cref="GlobalStaticConfiguration.Dashboard_RecheckInterval" />.
    /// </summary>
    private DateTime _lastCheck;

    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Load views
        ContentControlNetworkConnection.Content = _networkConnectionWidgetView;
        ContentControlIPApiIPGeolocation.Content = _ipApiIPGeolocationWidgetView;
        ContentControlIPApiDNSResolver.Content = _ipApiDNSResolverWidgetView;
        ContentControlSpeedTest.Content = _speedTestWidgetView;

        // The settings are changed while the dashboard is hidden (settings view) - check again when it is shown
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

        // Check network connection on first open. Later re-opens are covered by OnViewVisible().
        CheckNetworkConnection();
    }

    public void OnViewVisible()
    {
        _isViewVisible = true;

        _viewModel.OnViewVisible();

        // Check the network connection again if something changed, or if the last check is older than the recheck
        // interval (e.g. the public IP address may have changed) - not on every switch between the applications
        if (_isOutdated || DateTime.Now - _lastCheck >= GlobalStaticConfiguration.Dashboard_RecheckInterval)
            CheckNetworkConnection();
    }

    public void OnViewHide()
    {
        _isViewVisible = false;

        _viewModel.OnViewHide();
    }

    /// <summary>
    ///     Called by the main window when the network has changed (addresses, adapter connected or disconnected).
    /// </summary>
    public void OnNetworkChanged()
    {
        if (_isViewVisible)
            CheckNetworkConnection();
        else
            _isOutdated = true;
    }

    private void CheckNetworkConnection()
    {
        _isOutdated = false;
        _lastCheck = DateTime.Now;

        _networkConnectionWidgetView.Check();
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.StartsWith("Dashboard_", StringComparison.Ordinal) == true)
            _isOutdated = true;
    }
}