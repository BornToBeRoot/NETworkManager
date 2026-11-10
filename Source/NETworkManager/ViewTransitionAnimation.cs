using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NETworkManager.Models;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Animation when the view of an application is shown in the main window (switching the applications, after the
/// settings are closed): the view fades in and rises 8 px (200 ms, quadratic ease-out). Starts after the first layout
/// of the view (a new view, e.g. the first opening of a tool, would otherwise be shown when the animation is almost
/// over). Not for the applications with embedded windows (Remote Desktop, PowerShell, PuTTY, TigerVNC, Web Console):
/// native windows can't be faded or moved with the WPF content. Checks whether animations are enabled each time (see
/// <see cref="AppearanceManager.AnimationsEnabled"/>).
/// </summary>
public static class ViewTransitionAnimation
{
    private const double OffsetY = 8;

    private static readonly Duration Duration = new(TimeSpan.FromMilliseconds(200));

    /// <summary>
    /// Animates the element (the content control of the application view) when the view of the application is shown.
    /// </summary>
    /// <param name="element">Element that shows the view of the application.</param>
    /// <param name="name">Application of the view.</param>
    public static void Enter(FrameworkElement element, ApplicationName name)
    {
        // Own transform (a transform of the template could be frozen / shared)
        if (element.RenderTransform is not TranslateTransform { IsFrozen: false } translate)
        {
            translate = new TranslateTransform();
            element.RenderTransform = translate;
        }

        if (!AppearanceManager.AnimationsEnabled || HasEmbeddedWindows(name))
        {
            Set(element, translate, 1, 0);
            return;
        }

        // Hold the start values until the view is laid out and rendered for the first time
        Set(element, translate, 0, OffsetY);

        element.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            element.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(1, Duration) { EasingFunction = easing });
            translate.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, Duration) { EasingFunction = easing });
        });
    }

    private static bool HasEmbeddedWindows(ApplicationName name) => name is ApplicationName.RemoteDesktop
        or ApplicationName.PowerShell or ApplicationName.PuTTY or ApplicationName.TigerVNC
        or ApplicationName.WebConsole;

    /// <summary>
    /// Sets the values immediately (removes a running animation).
    /// </summary>
    private static void Set(UIElement element, TranslateTransform translate, double opacity, double offsetY)
    {
        element.BeginAnimation(UIElement.OpacityProperty, null);
        translate.BeginAnimation(TranslateTransform.YProperty, null);
        element.Opacity = opacity;
        translate.Y = offsetY;
    }
}
