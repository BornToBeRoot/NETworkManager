using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NETworkManager.Controls;

/// <summary>
///     Panel for the title of a window: the first child (title) on the left, the second child (e.g. the run command)
///     centered in the window (not only in the title area between the window commands, like the command center of VS
///     Code). If there is not enough space in the center (e.g. wide window commands on the right), the second child is
///     moved only as far as needed and only made smaller (e.g. text is trimmed) if the free space is smaller than its
///     <see cref="FrameworkElement.MaxWidth" />, so it doesn't overlap the title or the end of the panel.
/// </summary>
public class TitleBarPanel : Panel
{
    /// <summary>
    ///     Minimum space between the title, the centered child and the end of the panel.
    /// </summary>
    private const double Spacing = 10;

    /// <summary>
    ///     Position of the panel in the window and width of the window at the last measure.
    /// </summary>
    private double _measuredOffset = double.NaN;

    private double _measuredWindowWidth = double.NaN;

    private Window _window;

    public TitleBarPanel()
    {
        // The position in the window and the width of the window are only known after the layout, so the children are
        // measured again if they changed: after the first layout, when the window is resized (e.g. maximized) and when
        // the panel is resized (e.g. the window commands change). Not on every layout pass (LayoutUpdated), which would
        // also run while e.g. scan results are added.
        Loaded += (_, _) =>
        {
            _window = Window.GetWindow(this);

            if (_window != null)
                _window.SizeChanged += Window_SizeChanged;

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(MeasureIfMoved));
        };

        Unloaded += (_, _) =>
        {
            if (_window != null)
                _window.SizeChanged -= Window_SizeChanged;

            _window = null;
        };

        SizeChanged += (_, _) => MeasureIfMoved();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        MeasureIfMoved();
    }

    /// <summary>
    ///     Measures the children again if the position of the panel in the window or the width of the window changed
    ///     since the last measure.
    /// </summary>
    private void MeasureIfMoved()
    {
        var offset = GetOffsetInWindow();
        var windowWidth = Window.GetWindow(this)?.ActualWidth ?? double.NaN;

        if (double.IsNaN(offset) || double.IsNaN(windowWidth))
            return;

        // "!(... <= 0.5)" is also true if it was not measured yet (NaN).
        if (!(Math.Abs(offset - _measuredOffset) <= 0.5) || !(Math.Abs(windowWidth - _measuredWindowWidth) <= 0.5))
            InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        var height = 0.0;

        _measuredOffset = GetOffsetInWindow();
        _measuredWindowWidth = Window.GetWindow(this)?.ActualWidth ?? double.NaN;

        if (InternalChildren.Count > 0)
        {
            var title = InternalChildren[0];

            title.Measure(availableSize);
            height = title.DesiredSize.Height;

            if (InternalChildren.Count > 1)
            {
                var center = InternalChildren[1];
                var (_, centerWidth) = GetCenterLayout(width, title.DesiredSize.Width, GetPreferredWidth(center));

                center.Measure(new Size(centerWidth, availableSize.Height));
                height = Math.Max(height, center.DesiredSize.Height);
            }
        }

        // Use the full width (the title area of the window).
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count == 0)
            return finalSize;

        var title = InternalChildren[0];
        var titleWidth = Math.Min(title.DesiredSize.Width, finalSize.Width);

        title.Arrange(new Rect(0, 0, titleWidth, finalSize.Height));

        if (InternalChildren.Count < 2)
            return finalSize;

        var center = InternalChildren[1];
        var (x, width) = GetCenterLayout(finalSize.Width, titleWidth, GetPreferredWidth(center));

        center.Arrange(new Rect(x, 0, width, finalSize.Height));

        return finalSize;
    }

    /// <summary>
    ///     Gets the position and width of the centered child.
    /// </summary>
    /// <param name="panelWidth">Width of the panel.</param>
    /// <param name="titleWidth">Width of the title.</param>
    /// <param name="preferredWidth">Width of the child if there is enough space.</param>
    private (double X, double Width) GetCenterLayout(double panelWidth, double titleWidth, double preferredWidth)
    {
        // Center of the window in the coordinates of the panel (center of the panel until the window is measured)
        var windowCenter = double.IsNaN(_measuredOffset) || !(_measuredWindowWidth > 0)
            ? panelWidth / 2
            : _measuredWindowWidth / 2 - _measuredOffset;

        // Free space between the title and the end of the panel
        var left = titleWidth + Spacing;
        var right = panelWidth - Spacing;

        // Only smaller than preferred if the free space is smaller, then as close to the center as possible.
        var width = Math.Max(0, Math.Min(preferredWidth, right - left));
        var x = Math.Clamp(windowCenter - width / 2, left, Math.Max(left, right - width));

        return (x, width);
    }

    /// <summary>
    ///     Gets the width of the child if there is enough space: its maximum width, or the width of its content.
    /// </summary>
    private static double GetPreferredWidth(UIElement child)
    {
        if (child is FrameworkElement { MaxWidth: var maxWidth } && !double.IsInfinity(maxWidth))
            return maxWidth;

        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        return child.DesiredSize.Width;
    }

    /// <summary>
    ///     Gets the horizontal position of the panel in its window, NaN if it is not in a window.
    /// </summary>
    private double GetOffsetInWindow()
    {
        var window = Window.GetWindow(this);

        return window != null && window.IsAncestorOf(this) ? TranslatePoint(new Point(0, 0), window).X : double.NaN;
    }
}
