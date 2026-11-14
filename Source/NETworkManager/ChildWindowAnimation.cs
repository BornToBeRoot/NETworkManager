using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Attached properties that animate a dialog (<see cref="ChildWindow"/> with the <c>DefaultChildWindowTemplate</c>)
/// when it is opened and closed. The overlay fades in / out in the template (Show / Hide visual states), the animation
/// runs at the same time. See <see cref="ChildWindowAnimationKind"/> for the kinds.
///
/// Done in code instead of the storyboards of the template: the animation starts after the first layout of the dialog
/// (otherwise a heavy dialog, e.g. the profile dialog, is shown when the animation is almost over) and checks
/// whether animations are enabled each time a dialog is opened (see <see cref="AppearanceManager.AnimationsEnabled"/>).
///
/// The animation is applied to the <c>WindowScale</c> grid inside <c>PART_Window</c>: the ChildWindow sets the
/// RenderTransform of the window itself (TranslateTransform to move the dialog).
/// </summary>
public static class ChildWindowAnimation
{
    // Same as the fade out of the overlay (HideStoryboard), the dialog is removed when it ends
    private static readonly Duration HideDuration = new(TimeSpan.FromMilliseconds(100));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ChildWindowAnimation),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(UIElement element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(UIElement element) =>
        (bool)element.GetValue(IsEnabledProperty);

    /// <summary>
    /// Kind of the animation (default: <see cref="ChildWindowAnimationKind.Zoom"/>).
    /// </summary>
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.RegisterAttached(
            "Kind",
            typeof(ChildWindowAnimationKind),
            typeof(ChildWindowAnimation),
            new PropertyMetadata(ChildWindowAnimationKind.Zoom));

    public static void SetKind(UIElement element, ChildWindowAnimationKind value) =>
        element.SetValue(KindProperty, value);

    public static ChildWindowAnimationKind GetKind(UIElement element) =>
        (ChildWindowAnimationKind)element.GetValue(KindProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ChildWindow childWindow)
            return;

        if ((bool)e.NewValue)
            childWindow.IsOpenChanged += ChildWindow_IsOpenChanged;
        else
            childWindow.IsOpenChanged -= ChildWindow_IsOpenChanged;
    }

    private static void ChildWindow_IsOpenChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not ChildWindow childWindow)
            return;

        childWindow.ApplyTemplate();

        if (childWindow.Template?.FindName("WindowScale", childWindow) is not FrameworkElement windowScale)
            return;

        // Own transforms (a transform of the template could be frozen / shared)
        if (windowScale.RenderTransform is not TransformGroup { IsFrozen: false, Children: [ScaleTransform scale, TranslateTransform translate] })
        {
            scale = new ScaleTransform();
            translate = new TranslateTransform();
            windowScale.RenderTransform = new TransformGroup { Children = { scale, translate } };
        }

        if (!IsAnimationEnabled())
        {
            Set(scale, translate, 1, 0);
            return;
        }

        var (showScale, showOffsetY, showDuration, hideScale, showEasing) = GetKind(childWindow) switch
        {
            // Command palette: slides down a little from the top. Quadratic: visible over the whole duration (with a
            // cubic ease-out the short slide was over after a few milliseconds)
            ChildWindowAnimationKind.SlideDown => (0.98, -12.0, TimeSpan.FromMilliseconds(250), 1.0,
                (IEasingFunction)new QuadraticEase { EasingMode = EasingMode.EaseOut }),
            _ => (0.92, 0.0, TimeSpan.FromMilliseconds(220), 0.98,
                new CubicEase { EasingMode = EasingMode.EaseOut })
        };

        if (childWindow.IsOpen)
        {
            // Hold the start values until the dialog is laid out and rendered for the first time
            Set(scale, translate, showScale, showOffsetY);

            childWindow.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                if (childWindow.IsOpen)
                    Animate(scale, translate, 1, 0, new Duration(showDuration), showEasing);
            });
        }
        else
        {
            // Faster, without a slide (only the fade of the overlay for SlideDown)
            Animate(scale, translate, hideScale, 0, HideDuration, new CubicEase { EasingMode = EasingMode.EaseIn });
        }
    }

    /// <summary>
    /// Sets the values immediately (removes a running animation).
    /// </summary>
    private static void Set(ScaleTransform scale, TranslateTransform translate, double scaleValue, double offsetY)
    {
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        translate.BeginAnimation(TranslateTransform.YProperty, null);
        scale.ScaleX = scaleValue;
        scale.ScaleY = scaleValue;
        translate.Y = offsetY;
    }

    private static void Animate(ScaleTransform scale, TranslateTransform translate, double scaleValue, double offsetY,
        Duration duration, IEasingFunction easingFunction)
    {
        var scaleAnimation = new DoubleAnimation(scaleValue, duration) { EasingFunction = easingFunction };

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        translate.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(offsetY, duration) { EasingFunction = easingFunction });
    }

    /// <summary>
    /// Settings > Appearance > Animations (or the system-wide policy), by default the Windows setting.
    /// </summary>
    private static bool IsAnimationEnabled() => AppearanceManager.AnimationsEnabled;
}

/// <summary>
/// Kind of the animation of a dialog, see <see cref="ChildWindowAnimation"/>.
/// </summary>
public enum ChildWindowAnimationKind
{
    /// <summary>
    /// Grows from 92 % to its size (220 ms, ease-out), shrinks to 98 % when closed (100 ms, ease-in).
    /// </summary>
    Zoom,

    /// <summary>
    /// Slides down 12 px and grows from 98 % (250 ms, quadratic ease-out), only fades out when closed (e.g. run
    /// command).
    /// </summary>
    SlideDown
}
