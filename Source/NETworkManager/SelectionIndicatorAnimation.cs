using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NETworkManager.Settings;

namespace NETworkManager;

/// <summary>
/// Attached property for the accent indicator of a selected item (e.g. the navigation): bind it to the selection of
/// the item. When the item is selected, the indicator grows from its center (height 0 to its size, 150 ms, ease-out).
/// Checks whether animations are enabled each time (see <see cref="AppearanceManager.AnimationsEnabled"/>).
/// </summary>
public static class SelectionIndicatorAnimation
{
    private static readonly Duration Duration = new(TimeSpan.FromMilliseconds(150));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(SelectionIndicatorAnimation),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public static void SetIsSelected(UIElement element, bool value) =>
        element.SetValue(IsSelectedProperty, value);

    public static bool GetIsSelected(UIElement element) =>
        (bool)element.GetValue(IsSelectedProperty);

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element || !(bool)e.NewValue || !AppearanceManager.AnimationsEnabled)
            return;

        // Own transform (a transform of the template could be frozen / shared)
        if (element.RenderTransform is not ScaleTransform { IsFrozen: false } scale)
        {
            scale = new ScaleTransform();
            element.RenderTransform = scale;
        }

        element.RenderTransformOrigin = new Point(0.5, 0.5);

        scale.BeginAnimation(ScaleTransform.ScaleYProperty,
            new DoubleAnimation(0, 1, Duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
    }
}
