using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NETworkManager.Utilities.WPF;

/// <summary>
///     Spins the icon of a refresh button while <c>IsReloading</c> is true: one turn per second, repeated until the
///     reload is finished - also if it takes longer (e.g. the Firewall rules). When it is finished, the current turn is
///     completed instead of snapping back to the start.
/// </summary>
public static class ReloadAnimationHelper
{
    /// <summary>
    ///     Duration of one turn.
    /// </summary>
    private static readonly TimeSpan TurnDuration = TimeSpan.FromSeconds(1);

    public static readonly DependencyProperty IsReloadingProperty =
        DependencyProperty.RegisterAttached(
            "IsReloading",
            typeof(bool),
            typeof(ReloadAnimationHelper),
            new PropertyMetadata(false, OnIsReloadingChanged));

    public static void SetIsReloading(UIElement element, bool value) =>
        element.SetValue(IsReloadingProperty, value);

    public static bool GetIsReloading(UIElement element) =>
        (bool)element.GetValue(IsReloadingProperty);

    private static void OnIsReloadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Rectangle rect || e.NewValue is not bool isReloading)
            return;

        if (isReloading)
        {
            // Continue a turn that is still being completed (reloaded again right after the last reload)
            if (rect.RenderTransform is not RotateTransform rotate)
            {
                rotate = new RotateTransform();
                rect.RenderTransform = rotate;
            }

            rotate.CenterX = rect.Width / 2;
            rotate.CenterY = rect.Height / 2;

            var from = rotate.Angle % 360;

            rotate.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(from, from + 360, new Duration(TurnDuration))
                {
                    RepeatBehavior = RepeatBehavior.Forever
                });
        }
        else
        {
            if (rect.RenderTransform is not RotateTransform rotate)
                return;

            // Complete the current turn with the same speed
            var angle = rotate.Angle % 360;

            if (angle <= 0)
            {
                Reset(rect, rotate);
                return;
            }

            var finish = new DoubleAnimation(angle, 360,
                new Duration(TimeSpan.FromSeconds((360 - angle) / 360 * TurnDuration.TotalSeconds)));

            finish.Completed += (_, _) =>
            {
                // Not if it is reloading again in the meantime
                if (!GetIsReloading(rect))
                    Reset(rect, rotate);
            };

            rotate.BeginAnimation(RotateTransform.AngleProperty, finish);
        }
    }

    /// <summary>
    ///     Removes the animation and the rotation of the icon.
    /// </summary>
    private static void Reset(Rectangle rect, RotateTransform rotate)
    {
        rotate.BeginAnimation(RotateTransform.AngleProperty, null);

        if (ReferenceEquals(rect.RenderTransform, rotate))
            rect.RenderTransform = null;
    }
}
