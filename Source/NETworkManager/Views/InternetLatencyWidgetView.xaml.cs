using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class InternetLatencyWidgetView
{
    /// <summary>
    ///     Below this width the targets are shown one below the other (e.g. next to the speed test), otherwise side by
    ///     side (own row).
    /// </summary>
    private const double CompactWidth = 500;

    public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(nameof(IsCompact),
        typeof(bool), typeof(InternetLatencyWidgetView), new PropertyMetadata(false));

    private readonly InternetLatencyWidgetViewModel _viewModel = new();

    public InternetLatencyWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        SizeChanged += (_, e) =>
        {
            if (e.WidthChanged)
                IsCompact = e.NewSize.Width < CompactWidth;
        };
    }

    /// <summary>
    ///     The widget is narrow: the targets are shown one below the other.
    /// </summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        private set => SetValue(IsCompactProperty, value);
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
