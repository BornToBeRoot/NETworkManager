using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class InternetLatencyWidgetView
{
    private readonly InternetLatencyWidgetViewModel _viewModel = new();

    public InternetLatencyWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    /// <summary>
    ///     Starts the pings (the dashboard is shown and the check is enabled).
    /// </summary>
    public void Start()
    {
        _viewModel.Start();
    }

    /// <summary>
    ///     Stops the pings (dashboard hidden, window minimized, widget hidden or check disabled).
    /// </summary>
    public void Stop()
    {
        _viewModel.Stop();
    }

    /// <summary>
    ///     Starts again if it runs (e.g. after a network change).
    /// </summary>
    public void Restart()
    {
        _viewModel.Restart();
    }
}
