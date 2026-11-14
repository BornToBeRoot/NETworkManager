using System.Windows;

namespace NETworkManager.Controls;

/// <summary>
/// Attached property for the layout of the main window with boxes (navigation, main view, profiles): a view that sets
/// <c>HasOwnBoxes</c> draws its own boxes (e.g. the tool area and the profile panel of a host view), the main window
/// doesn't draw a box around it.
/// </summary>
public static class ViewLayoutHelper
{
    public static readonly DependencyProperty HasOwnBoxesProperty =
        DependencyProperty.RegisterAttached(
            "HasOwnBoxes",
            typeof(bool),
            typeof(ViewLayoutHelper),
            new PropertyMetadata(false));

    public static void SetHasOwnBoxes(UIElement element, bool value) =>
        element.SetValue(HasOwnBoxesProperty, value);

    public static bool GetHasOwnBoxes(UIElement element) =>
        (bool)element.GetValue(HasOwnBoxesProperty);
}
