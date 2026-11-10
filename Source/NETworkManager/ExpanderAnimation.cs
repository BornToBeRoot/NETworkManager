using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Attached property for an <see cref="Expander"/> (DefaultExpander and the styles based on it): when it is opened or
/// closed, the chevron of the header (element named <c>Chevron</c> in the header template) rotates (150 ms) and, when
/// expanded, the content fades in (150 ms, quadratic ease-out). Collapsing doesn't fade. Only rotation and opacity (no
/// height animation, which would lay out the whole list for every frame).
///
/// Done on the Expanded / Collapsed events, which are only raised when the state changes - not when the view is
/// loaded (the header template sets the initial angle without animation), so many expanded items (e.g. Ping
/// Monitor hosts) don't all animate when the view is shown. Checks whether animations are enabled each time (see
/// <see cref="AppearanceManager.AnimationsEnabled"/>).
/// </summary>
public static class ExpanderAnimation
{
    private const string ChevronName = "Chevron";

    private static readonly Duration Duration = new(TimeSpan.FromMilliseconds(150));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ExpanderAnimation),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(UIElement element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(UIElement element) =>
        (bool)element.GetValue(IsEnabledProperty);

    /// <summary>
    /// The chevron of the collapsed and of the expanded state are different elements (e.g. the profile panel: ‹ in the
    /// header when collapsed, a › button in the content when expanded): the chevron that is shown after the change
    /// starts in the other direction and rotates into its own (looks like one chevron that turns). Set with
    /// <see cref="IsEnabledProperty"/>.
    /// </summary>
    public static readonly DependencyProperty SwapChevronProperty =
        DependencyProperty.RegisterAttached(
            "SwapChevron",
            typeof(bool),
            typeof(ExpanderAnimation),
            new PropertyMetadata(false));

    public static void SetSwapChevron(UIElement element, bool value) =>
        element.SetValue(SwapChevronProperty, value);

    public static bool GetSwapChevron(UIElement element) =>
        (bool)element.GetValue(SwapChevronProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Expander expander)
            return;

        if ((bool)e.NewValue)
        {
            expander.Expanded += Expander_Expanded;
            expander.Collapsed += Expander_Collapsed;
        }
        else
        {
            expander.Expanded -= Expander_Expanded;
            expander.Collapsed -= Expander_Collapsed;
        }
    }

    private static void Expander_Expanded(object sender, RoutedEventArgs e)
    {
        // Expanded is a routed event: only the expander itself, not a nested one
        if (!ReferenceEquals(e.OriginalSource, sender) || sender is not Expander expander)
            return;

        if (GetSwapChevron(expander))
            SwapChevron(expander);
        else
            RotateChevron(expander, 180, 0);

        if (AppearanceManager.AnimationsEnabled && expander.Content is UIElement content)
            content.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, Duration)
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
    }

    private static void Expander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, sender) || sender is not Expander expander)
            return;

        if (GetSwapChevron(expander))
            SwapChevron(expander);
        else
            RotateChevron(expander, 0, 180);
    }

    /// <summary>
    /// <see cref="SwapChevronProperty"/>: after the layout of the new state (the header template / content changes),
    /// the visible chevron turns from the other direction (180 degrees) into its own (0).
    /// </summary>
    private static void SwapChevron(Expander expander)
    {
        if (!AppearanceManager.AnimationsEnabled)
            return;

        expander.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            if (FindChevron(expander) is not { } chevron)
                return;

            var rotate = new RotateTransform(180);
            chevron.RenderTransformOrigin = new Point(0.5, 0.5);
            chevron.RenderTransform = rotate;

            rotate.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(180, 0, Duration)
                { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
        });
    }

    /// <summary>
    /// Rotates the chevron of the header. Uses an own transform (the one of the template is frozen), which then also
    /// keeps the angle (the trigger of the template is only for the initial state).
    /// </summary>
    private static void RotateChevron(Expander expander, double from, double to)
    {
        if (FindChevron(expander) is not { } chevron)
            return;

        var animate = AppearanceManager.AnimationsEnabled;

        // Without animations: the trigger of the template sets the angle, unless an own transform was set before
        // (animations turned off later) - then set its angle
        if (!animate)
        {
            if (chevron.RenderTransform is RotateTransform { IsFrozen: false } ownRotate)
            {
                ownRotate.BeginAnimation(RotateTransform.AngleProperty, null);
                ownRotate.Angle = to;
            }

            return;
        }

        if (chevron.RenderTransform is not RotateTransform { IsFrozen: false } rotate)
        {
            rotate = new RotateTransform(from);
            chevron.RenderTransform = rotate;
        }

        rotate.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(from, to, Duration)
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
    }

    /// <summary>
    /// The chevron: the first visible element named <see cref="ChevronName"/> or with this tag (if several in one view,
    /// e.g. the profile panel) - breadth-first, the header is above the chevrons of nested expanders in the content.
    /// </summary>
    private static FrameworkElement FindChevron(DependencyObject root)
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var element = queue.Dequeue();

            // Hidden part (e.g. the other state): not its chevrons
            if (element is UIElement { IsVisible: false })
                continue;

            if (element is FrameworkElement chevron && !ReferenceEquals(element, root) &&
                (chevron.Name == ChevronName || chevron.Tag as string == ChevronName))
                return chevron;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                queue.Enqueue(VisualTreeHelper.GetChild(element, i));
        }

        return null;
    }
}
