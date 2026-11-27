using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class InternetLatencyWidgetView
{
    /// <summary>
    ///     Below this width the targets are shown one below the other also in their own row (narrow window).
    /// </summary>
    private const double CompactWidth = 500;

    /// <summary>
    ///     The widget is shown next to another widget (1/3 of the width), see <see cref="SetNextToOtherWidget" />.
    /// </summary>
    private bool _isNextToOtherWidget;

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
                UpdateIsCompact();
        };
    }

    /// <summary>
    ///     The targets are shown one below the other: next to another widget (always, also in a wide window) or if the
    ///     widget is narrow; otherwise side by side (own row).
    /// </summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        private set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    ///     Called by the dashboard when the widget is placed next to another widget (the speed test) or in its own row.
    /// </summary>
    /// <param name="isNextToOtherWidget">The widget is shown next to another widget.</param>
    public void SetNextToOtherWidget(bool isNextToOtherWidget)
    {
        _isNextToOtherWidget = isNextToOtherWidget;

        UpdateIsCompact();
    }

    private void UpdateIsCompact()
    {
        IsCompact = _isNextToOtherWidget || ActualWidth < CompactWidth;
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
