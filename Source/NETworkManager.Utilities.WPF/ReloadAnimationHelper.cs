using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NETworkManager.Utilities.WPF;

public static class ReloadAnimationHelper
{
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
            var rotate = new RotateTransform
            {
                CenterX = rect.Width / 2, 
                CenterY = rect.Height / 2
            };
            
            rect.RenderTransform = rotate;

            var animation = new DoubleAnimation(0, 720, new Duration(TimeSpan.FromSeconds(2)));
            
            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
        else
        {
            rect.RenderTransform = null;
        }
    }
}
