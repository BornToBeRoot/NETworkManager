using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Attached property that zooms a dialog (<see cref="ChildWindow"/> with the <c>DefaultChildWindowTemplate</c>) when
/// it is opened (grows from 92 % to its size, ease-out) and closed (shrinks to 98 %, ease-in). The overlay fades in /
/// out in the template (Show / Hide visual states), the zoom runs at the same time.
///
/// Done in code instead of the storyboards of the template: the zoom starts after the first layout of the dialog
/// (otherwise a heavy dialog, e.g. the profile dialog, is shown when the animation is almost over) and checks
/// whether animations are enabled each time a dialog is opened (see <see cref="AppearanceManager.AnimationsEnabled"/>).
///
/// The zoom is applied to the <c>WindowScale</c> grid inside <c>PART_Window</c>: the ChildWindow sets the
/// RenderTransform of the window itself (TranslateTransform to move the dialog).
/// </summary>
public static class ChildWindowAnimation
{
    private const double ShowScale = 0.92;
    private const double HideScale = 0.98;

    private static readonly Duration ShowDuration = new(TimeSpan.FromMilliseconds(220));

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

        // Own transform (a transform of the template could be frozen / shared)
        if (windowScale.RenderTransform is not ScaleTransform { IsFrozen: false } scaleTransform)
        {
            scaleTransform = new ScaleTransform();
            windowScale.RenderTransform = scaleTransform;
        }

        if (!IsAnimationEnabled())
        {
            Animate(scaleTransform, 1, null, null);
            return;
        }

        if (childWindow.IsOpen)
        {
            // Hold the start value until the dialog is laid out and rendered for the first time
            Animate(scaleTransform, ShowScale, null, null);

            childWindow.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                if (childWindow.IsOpen)
                    Animate(scaleTransform, 1, ShowDuration, new CubicEase { EasingMode = EasingMode.EaseOut });
            });
        }
        else
        {
            Animate(scaleTransform, HideScale, HideDuration, new CubicEase { EasingMode = EasingMode.EaseIn });
        }
    }

    /// <summary>
    /// Animates the scale to the value, or sets it immediately without a duration.
    /// </summary>
    private static void Animate(ScaleTransform scaleTransform, double to, Duration? duration,
        IEasingFunction easingFunction)
    {
        if (duration == null)
        {
            // Remove a running animation, then set the value
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            scaleTransform.ScaleX = to;
            scaleTransform.ScaleY = to;
            return;
        }

        var animation = new DoubleAnimation(to, duration.Value) { EasingFunction = easingFunction };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    /// <summary>
    /// Settings > Appearance > Animations (or the system-wide policy), by default the Windows setting.
    /// </summary>
    private static bool IsAnimationEnabled() => AppearanceManager.AnimationsEnabled;
}
