using System.Windows;
using System.Windows.Media;

namespace NETworkManager;

/// <summary>
/// Attached properties for the button template of <c>DefaultButton</c> (ButtonStyles.xaml): the layers above the
/// background that fade in on mouse over and while the button is pressed (instead of switching the background, so the
/// change can be animated - the theme brushes are shared and frozen).
/// </summary>
public static class ButtonHelper
{
    public static readonly DependencyProperty MouseOverOverlayProperty =
        DependencyProperty.RegisterAttached("MouseOverOverlay", typeof(Brush), typeof(ButtonHelper),
            new FrameworkPropertyMetadata(Brushes.Transparent));

    public static void SetMouseOverOverlay(DependencyObject element, Brush value) =>
        element.SetValue(MouseOverOverlayProperty, value);

    public static Brush GetMouseOverOverlay(DependencyObject element) =>
        (Brush)element.GetValue(MouseOverOverlayProperty);

    public static readonly DependencyProperty PressedOverlayProperty =
        DependencyProperty.RegisterAttached("PressedOverlay", typeof(Brush), typeof(ButtonHelper),
            new FrameworkPropertyMetadata(Brushes.Transparent));

    public static void SetPressedOverlay(DependencyObject element, Brush value) =>
        element.SetValue(PressedOverlayProperty, value);

    public static Brush GetPressedOverlay(DependencyObject element) =>
        (Brush)element.GetValue(PressedOverlayProperty);
}
